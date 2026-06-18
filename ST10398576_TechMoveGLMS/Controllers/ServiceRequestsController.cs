using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using ST10398576_TechMoveGLMS.Models;

namespace ST10398576_TechMoveGLMS.Controllers
{
    public class ServiceRequestsController : Controller
    {
        private readonly HttpClient _httpClient;

        public ServiceRequestsController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("Api");
        }

        private bool AttachToken()
        {
            string? token = HttpContext?.Session?.GetString("JwtToken");
            if (string.IsNullOrEmpty(token))
            {
                var auth = HttpContext?.Request?.Headers["Authorization"].FirstOrDefault();
                if (!string.IsNullOrEmpty(auth) && auth.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                {
                    token = auth.Substring("Bearer ".Length).Trim();
                }
            }

            if (string.IsNullOrEmpty(token))
            {
                if (HttpContext == null) return true;
                return false;
            }

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return true;
        }

        // GET: ServiceRequests
        public async Task<IActionResult> Index(string? status, DateTime? startDate, DateTime? endDate)
        {
            if (!AttachToken()) return RedirectToAction("Login", "Account");
            var query = new List<string>();
            if (!string.IsNullOrEmpty(status)) query.Add($"status={status}");
            if (startDate.HasValue) query.Add($"startDate={startDate.Value:O}");
            if (endDate.HasValue) query.Add($"endDate={endDate.Value:O}");

            var url = "/api/servicerequests" + (query.Count > 0 ? "?" + string.Join("&", query) : "");

            List<ServiceRequest> requests;
            var response = await _httpClient.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                requests = JsonSerializer.Deserialize<List<ServiceRequest>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<ServiceRequest>();
            }
            else
            {
                requests = new List<ServiceRequest>();
            }

            return View(requests);
        }

        // GET: ServiceRequests/Details/5
        public async Task<IActionResult> Details(int id)
        {
            if (!AttachToken()) return RedirectToAction("Login", "Account");
            var response = await _httpClient.GetAsync($"/api/servicerequests/{id}");
            if (!response.IsSuccessStatusCode) return NotFound();

            var json = await response.Content.ReadAsStringAsync();
            var request = JsonSerializer.Deserialize<ServiceRequest>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return View(request);
        }

        // GET: ServiceRequests/Create
        public async Task<IActionResult> Create()
        {
            if (!AttachToken()) return RedirectToAction("Login", "Account");
            var response = await _httpClient.GetAsync("/api/contracts");
            if (!response.IsSuccessStatusCode)
            {
                ViewBag.Contracts = new List<SelectListItem>();
                return View();
            }

            var json = await response.Content.ReadAsStringAsync();
            var contracts = JsonSerializer.Deserialize<List<Contract>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<Contract>();

            ViewBag.Contracts = contracts.Select(c => new SelectListItem
            {
                Value = c.ContractId.ToString(),
                Text = c.Client != null ? $"{c.ContractId} - {c.Client.ClientName}" : c.ContractId.ToString(),
                Selected = false
            }).ToList();
            return View();
        }

        private async Task PopulateContractsSelectListAsync(int? selectedContractId = null)
        {
            if (!AttachToken()) return;
            var resp = await _httpClient.GetAsync("/api/contracts");
            if (!resp.IsSuccessStatusCode)
            {
                ViewBag.ContractId = new List<SelectListItem>();
                return;
            }

            var json = await resp.Content.ReadAsStringAsync();
            var contracts = JsonSerializer.Deserialize<List<Contract>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<Contract>();

            ViewBag.ContractId = contracts.Select(c => new SelectListItem
            {
                Value = c.ContractId.ToString(),
                Text = c.Client != null ? $"{c.ContractId} - {c.Client.ClientName}" : c.ContractId.ToString(),
                Selected = selectedContractId.HasValue && c.ContractId == selectedContractId.Value
            }).ToList();
        }

        // POST: ServiceRequests/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ServiceRequest request)
        {
            if (!AttachToken()) return RedirectToAction("Login", "Account");
            if (ModelState.IsValid)
            {
                var json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("/api/servicerequests", content);
                if (response.IsSuccessStatusCode)
                    return RedirectToAction(nameof(Index));
            }

            // repopulate contract dropdown on error
            await PopulateContractsSelectListAsync(request.ContractId);
            return View(request);
        }

        // GET: ServiceRequests/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            if (!AttachToken()) return RedirectToAction("Login", "Account");
            var response = await _httpClient.GetAsync($"/api/servicerequests/{id}");
            if (!response.IsSuccessStatusCode) return NotFound();

            var json = await response.Content.ReadAsStringAsync();
            var request = JsonSerializer.Deserialize<ServiceRequest>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            // populate contracts dropdown
            await PopulateContractsSelectListAsync(request?.ContractId);

            ViewBag.StatusList = new SelectList(new[] { "Pending", "Completed" }, request?.ServiceStatus);
            return View(request);
        }

        // POST: ServiceRequests/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ServiceRequest request)
        {
            if (!AttachToken()) return RedirectToAction("Login", "Account");
            if (id != request.ServiceRequestId) return NotFound();

            if (ModelState.IsValid)
            {
                var json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PutAsync($"/api/servicerequests/{id}", content);
                if (response.IsSuccessStatusCode)
                    return RedirectToAction(nameof(Index));
            }

            // repopulate dropdowns on error
            await PopulateContractsSelectListAsync(request.ContractId);
            ViewBag.StatusList = new SelectList(new[] { "Pending", "Completed" }, request.ServiceStatus);
            return View(request);
        }

        // GET: ServiceRequests/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            AttachToken();
            var response = await _httpClient.GetAsync($"/api/servicerequests/{id}");
            if (!response.IsSuccessStatusCode) return NotFound();

            var json = await response.Content.ReadAsStringAsync();
            var request = JsonSerializer.Deserialize<ServiceRequest>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return View(request);
        }

        // POST: ServiceRequests/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            AttachToken();
            var response = await _httpClient.DeleteAsync($"/api/servicerequests/{id}");
            if (response.IsSuccessStatusCode)
                return RedirectToAction(nameof(Index));

            return NotFound();
        }
    }
}
