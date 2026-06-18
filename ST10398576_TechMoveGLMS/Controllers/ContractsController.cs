using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ST10398576_TechMoveGLMS.Models;
using System;
using System.IO;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using System.Linq;

namespace ST10398576_TechMoveGLMS.Controllers
{
    public class ContractsController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly IWebHostEnvironment? _env;

        public ContractsController(IHttpClientFactory httpClientFactory, IWebHostEnvironment? env = null)
        {
            _httpClient = httpClientFactory.CreateClient("Api");
            _env = env;
        }

        // Attach token from server-side session. Returns false if no token is present.
        private bool AttachToken()
        {
            // Try session first (normal runtime), fall back to Authorization header (tests or proxy)
            string? token = HttpContext?.Session?.GetString("JwtToken");
            if (string.IsNullOrEmpty(token))
            {
                var auth = HttpContext?.Request?.Headers["Authorization"].FirstOrDefault();
                if (!string.IsNullOrEmpty(auth) && auth.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                {
                    token = auth.Substring("Bearer ".Length).Trim();
                }
            }

            // If no token and no HttpContext (unit tests), allow operation to continue without redirecting
            if (string.IsNullOrEmpty(token))
            {
                if (HttpContext == null) return true;
                return false;
            }

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return true;
        }

        public async Task<IActionResult> Index(string? status, string? serviceLevel, DateTime? startDate, DateTime? endDate)
        {
            if (!AttachToken())
                return RedirectToAction("Login", "Account");
            var query = new List<string>();
            if (!string.IsNullOrEmpty(status)) query.Add($"status={status}");
            if (!string.IsNullOrEmpty(serviceLevel)) query.Add($"serviceLevel={serviceLevel}");
            if (startDate.HasValue) query.Add($"startDate={startDate.Value:O}");
            if (endDate.HasValue) query.Add($"endDate={endDate.Value:O}");

            var url = "/api/contracts" + (query.Count > 0 ? "?" + string.Join("&", query) : "");
            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode) return View(new List<Contract>());

            var json = await response.Content.ReadAsStringAsync();
            var contracts = JsonSerializer.Deserialize<List<Contract>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return View(contracts);
        }

        public async Task<IActionResult> Details(int id)
        {
            if (!AttachToken()) return RedirectToAction("Login", "Account");
            var response = await _httpClient.GetAsync($"/api/contracts/{id}");
            if (!response.IsSuccessStatusCode) return NotFound();

            var json = await response.Content.ReadAsStringAsync();
            var contract = JsonSerializer.Deserialize<Contract>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return View(contract);
        }

        public async Task<IActionResult> Create()
        {
            if (!AttachToken()) return RedirectToAction("Login", "Account");
            await PopulateClientsSelectListAsync();
            PopulateStatusAndServiceLevelLists();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Contract contract, IFormFile? AgreementPdf = null)
        {
            if (!AttachToken()) return RedirectToAction("Login", "Account");
            if (ModelState.IsValid)
            {
                if (AgreementPdf != null && AgreementPdf.Length > 0)
                {
                    var originalFileName = Path.GetFileName(AgreementPdf.FileName);
                    var ext = Path.GetExtension(originalFileName);
                    if (string.Equals(ext, ".pdf", StringComparison.OrdinalIgnoreCase))
                    {
                        var webRoot = _env?.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                        var uploadsDir = Path.Combine(webRoot, "uploads", "contracts");
                        Directory.CreateDirectory(uploadsDir);
                        var safeFileName = Guid.NewGuid().ToString("N") + ext;
                        var filePath = Path.Combine(uploadsDir, safeFileName);
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await AgreementPdf.CopyToAsync(stream);
                        }
                        contract.PdfFilePath = "/uploads/contracts/" + safeFileName;
                    }
                }

                var json = JsonSerializer.Serialize(contract);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync("/api/contracts", content);

                if (response.IsSuccessStatusCode)
                    return RedirectToAction(nameof(Index));
            }
            await PopulateClientsSelectListAsync(contract.ClientId);
            PopulateStatusAndServiceLevelLists(contract.ContractStatus, contract.ContractServiceLevel);
            return View(contract);
        }

        public async Task<IActionResult> Edit(int id)
        {
            if (!AttachToken()) return RedirectToAction("Login", "Account");
            var response = await _httpClient.GetAsync($"/api/contracts/{id}");
            if (!response.IsSuccessStatusCode) return NotFound();

            var json = await response.Content.ReadAsStringAsync();
            var contract = JsonSerializer.Deserialize<Contract>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            await PopulateClientsSelectListAsync(contract?.ClientId);
            PopulateStatusAndServiceLevelLists(contract?.ContractStatus, contract?.ContractServiceLevel);
            return View(contract);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Contract contract)
        {
            if (!AttachToken()) return RedirectToAction("Login", "Account");
            if (id != contract.ContractId) return NotFound();

            if (ModelState.IsValid)
            {
                var json = JsonSerializer.Serialize(contract);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PutAsync($"/api/contracts/{id}", content);
                if (response.IsSuccessStatusCode)
                    return RedirectToAction(nameof(Index));
            }
            await PopulateClientsSelectListAsync(contract.ClientId);
            PopulateStatusAndServiceLevelLists(contract.ContractStatus, contract.ContractServiceLevel);
            return View(contract);
        }

        private void PopulateStatusAndServiceLevelLists(string? selectedStatus = null, string? selectedServiceLevel = null)
        {
            var statuses = new[] { "Active", "Expired", "On Hold", "ToDelete", "Updated" };
            var levels = new[] { "High", "Medium", "Low" };

            ViewBag.StatusList = new SelectList(statuses, selectedStatus);
            ViewBag.ServiceLevelList = new SelectList(levels, selectedServiceLevel);
        }

        private async Task PopulateClientsSelectListAsync(int? selectedClientId = null)
        {
            // assumes AttachToken already called by caller
            var response = await _httpClient.GetAsync("/api/clients");
            var json = await response.Content.ReadAsStringAsync();
            var clients = JsonSerializer.Deserialize<List<Client>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<Client>();

            ViewBag.ClientId = new SelectList(clients, "ClientId", "ClientName", selectedClientId);
        }

        public async Task<IActionResult> Delete(int id)
        {
            if (!AttachToken()) return RedirectToAction("Login", "Account");
            var response = await _httpClient.GetAsync($"/api/contracts/{id}");
            if (!response.IsSuccessStatusCode) return NotFound();

            var json = await response.Content.ReadAsStringAsync();
            var contract = JsonSerializer.Deserialize<Contract>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return View(contract);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (!AttachToken()) return RedirectToAction("Login", "Account");
            var response = await _httpClient.DeleteAsync($"/api/contracts/{id}");
            if (response.IsSuccessStatusCode)
                return RedirectToAction(nameof(Index));

            return NotFound();
        }
    }
}
