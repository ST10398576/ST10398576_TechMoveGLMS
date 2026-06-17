using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ST10398576_TechMoveGLMS.Models;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using System;

public class ContractsController : Controller
{
    private readonly HttpClient _httpClient;
    private readonly IWebHostEnvironment? _env;

    public ContractsController(IHttpClientFactory httpClientFactory, IWebHostEnvironment? env = null)
    {
        _httpClient = httpClientFactory.CreateClient();
        _httpClient.BaseAddress = new Uri("https://localhost:7066");
        _env = env;
    }

    public async Task<IActionResult> Index(string? status, string? serviceLevel, DateTime? startDate, DateTime? endDate)
    {
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
        var response = await _httpClient.GetAsync($"/api/contracts/{id}");
        if (!response.IsSuccessStatusCode) return NotFound();

        var json = await response.Content.ReadAsStringAsync();
        var contract = JsonSerializer.Deserialize<Contract>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        return View(contract);
    }

    public async Task<IActionResult> Create()
    {
        // Populate clients for the client dropdown when creating a contract
        var response = await _httpClient.GetAsync("/api/clients");
        var json = await response.Content.ReadAsStringAsync();
        var clients = JsonSerializer.Deserialize<List<ST10398576_TechMoveGLMS.Models.Client>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<ST10398576_TechMoveGLMS.Models.Client>();

        ViewBag.ClientId = new SelectList(clients, "ClientId", "ClientName");
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Contract contract, IFormFile? AgreementPdf = null)
    {
        if (ModelState.IsValid)
        {
            if (AgreementPdf != null && AgreementPdf.Length > 0)
            {
                var originalFileName = Path.GetFileName(AgreementPdf.FileName);
                var ext = Path.GetExtension(originalFileName);
                if (!string.Equals(ext, ".pdf", StringComparison.OrdinalIgnoreCase))
                {
                    ModelState.AddModelError("AgreementPdf", "Only PDF files are allowed.");
                }
                else
                {
                    var webRoot = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
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
        // If we got this far something failed; re-populate the clients dropdown and return view
        var resp = await _httpClient.GetAsync("/api/clients");
        var json2 = await resp.Content.ReadAsStringAsync();
        var clients2 = JsonSerializer.Deserialize<List<ST10398576_TechMoveGLMS.Models.Client>>(json2, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<ST10398576_TechMoveGLMS.Models.Client>();
        ViewBag.ClientId = new SelectList(clients2, "ClientId", "ClientName", contract.ClientId);
        return View(contract);
    }


    public async Task<IActionResult> Edit(int id)
    {
        var response = await _httpClient.GetAsync($"/api/contracts/{id}");
        if (!response.IsSuccessStatusCode) return NotFound();

        var json = await response.Content.ReadAsStringAsync();
        var contract = JsonSerializer.Deserialize<Contract>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        // populate clients for client dropdown
        var clientsResp = await _httpClient.GetAsync("/api/clients");
        var clientsJson = await clientsResp.Content.ReadAsStringAsync();
        var clients = JsonSerializer.Deserialize<List<ST10398576_TechMoveGLMS.Models.Client>>(clientsJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<ST10398576_TechMoveGLMS.Models.Client>();
        ViewBag.ClientId = new SelectList(clients, "ClientId", "ClientName", contract?.ClientId);

        // status and service level lists
        ViewBag.StatusList = new SelectList(new[] { "Active", "Completed", "On Hold", "Expired" }, contract?.ContractStatus);
        ViewBag.ServiceLevelList = new SelectList(new[] { "High", "Medium", "Low" }, contract?.ContractServiceLevel);

        return View(contract);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Contract contract, IFormFile? AgreementPdf = null)
    {
        if (id != contract.ContractId) return NotFound();

        if (ModelState.IsValid)
        {
            if (AgreementPdf != null && AgreementPdf.Length > 0)
            {
                var originalFileName = Path.GetFileName(AgreementPdf.FileName);
                var ext = Path.GetExtension(originalFileName);
                if (!string.Equals(ext, ".pdf", StringComparison.OrdinalIgnoreCase))
                {
                    ModelState.AddModelError("AgreementPdf", "Only PDF files are allowed.");
                }
                else
                {
                    var webRoot = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
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

            if (ModelState.IsValid)
            {
                var json = JsonSerializer.Serialize(contract);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PutAsync($"/api/contracts/{id}", content);
                if (response.IsSuccessStatusCode)
                    return RedirectToAction(nameof(Index));
            }
        }
        // repopulate dropdowns on error
        var resp = await _httpClient.GetAsync("/api/clients");
        var json2 = await resp.Content.ReadAsStringAsync();
        var clients2 = JsonSerializer.Deserialize<List<ST10398576_TechMoveGLMS.Models.Client>>(json2, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<ST10398576_TechMoveGLMS.Models.Client>();
        ViewBag.ClientId = new SelectList(clients2, "ClientId", "ClientName", contract.ClientId);
        ViewBag.StatusList = new SelectList(new[] { "Active", "Completed", "On Hold", "Expired" }, contract.ContractStatus);
        ViewBag.ServiceLevelList = new SelectList(new[] { "High", "Medium", "Low" }, contract.ContractServiceLevel);
        return View(contract);
    }

    public async Task<IActionResult> Delete(int id)
    {
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
        var response = await _httpClient.DeleteAsync($"/api/contracts/{id}");
        if (response.IsSuccessStatusCode)
            return RedirectToAction(nameof(Index));

        return NotFound();
    }
}
