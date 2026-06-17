using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ST10398576_TechMoveGLMS.Models;
using System.Text.Json;
using System.Text;

public class ServiceRequestsController : Controller
{
    private readonly HttpClient _httpClient;

    public ServiceRequestsController(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient();
        _httpClient.BaseAddress = new Uri("https://localhost:7066"); // API base URL
    }

    // GET: ServiceRequests
    public async Task<IActionResult> Index(string? status, DateTime? startDate, DateTime? endDate)
    {
        var query = new List<string>();
        if (!string.IsNullOrEmpty(status)) query.Add($"status={status}");
        if (startDate.HasValue) query.Add($"startDate={startDate.Value:O}");
        if (endDate.HasValue) query.Add($"endDate={endDate.Value:O}");

        var url = "/api/servicerequests" + (query.Count > 0 ? "?" + string.Join("&", query) : "");

        var response = await _httpClient.GetAsync(url);
        if (!response.IsSuccessStatusCode) return View(new List<ServiceRequest>());

        var json = await response.Content.ReadAsStringAsync();
        var requests = JsonSerializer.Deserialize<List<ServiceRequest>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        return View(requests);
    }


    // GET: ServiceRequests/Details/5
    public async Task<IActionResult> Details(int id)
    {
        var response = await _httpClient.GetAsync($"/api/servicerequests/{id}");
        if (!response.IsSuccessStatusCode) return NotFound();

        var json = await response.Content.ReadAsStringAsync();
        var request = JsonSerializer.Deserialize<ServiceRequest>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        return View(request);
    }

    // GET: ServiceRequests/Create
    public async Task<IActionResult> Create()
    {
        // Populate contracts for the ContractId dropdown. Include client name if available for clarity.
        var response = await _httpClient.GetAsync("/api/contracts");
        if (!response.IsSuccessStatusCode)
        {
            ViewBag.Contracts = new List<SelectListItem>();
            return View();
        }

        var json = await response.Content.ReadAsStringAsync();
        var contracts = JsonSerializer.Deserialize<List<ST10398576_TechMoveGLMS.Models.Contract>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<ST10398576_TechMoveGLMS.Models.Contract>();

        var items = contracts.Select(c => new SelectListItem
        {
            Value = c.ContractId.ToString(),
            Text = c.Client != null ? $"{c.ContractId} - {c.Client.ClientName}" : c.ContractId.ToString()
        }).ToList();

        ViewBag.Contracts = items;
        return View();
    }

    // POST: ServiceRequests/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ServiceRequest request)
    {
        if (ModelState.IsValid)
        {
            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("/api/servicerequests", content);
            if (response.IsSuccessStatusCode)
                return RedirectToAction(nameof(Index));
        }
        // repopulate contract dropdown on error
        var resp = await _httpClient.GetAsync("/api/contracts");
        var json2 = await resp.Content.ReadAsStringAsync();
        var contracts2 = JsonSerializer.Deserialize<List<ST10398576_TechMoveGLMS.Models.Contract>>(json2, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<ST10398576_TechMoveGLMS.Models.Contract>();
        ViewBag.Contracts = contracts2.Select(c => new SelectListItem { Value = c.ContractId.ToString(), Text = c.Client != null ? $"{c.ContractId} - {c.Client.ClientName}" : c.ContractId.ToString() }).ToList();
        return View(request);
    }

    // GET: ServiceRequests/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        var response = await _httpClient.GetAsync($"/api/servicerequests/{id}");
        if (!response.IsSuccessStatusCode) return NotFound();

        var json = await response.Content.ReadAsStringAsync();
        var request = JsonSerializer.Deserialize<ServiceRequest>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        // populate contracts dropdown (include client name when available)
        var resp = await _httpClient.GetAsync("/api/contracts");
        if (resp.IsSuccessStatusCode)
        {
            var json2 = await resp.Content.ReadAsStringAsync();
            var contracts = JsonSerializer.Deserialize<List<ST10398576_TechMoveGLMS.Models.Contract>>(json2, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<ST10398576_TechMoveGLMS.Models.Contract>();
            ViewBag.ContractId = contracts.Select(c => new SelectListItem { Value = c.ContractId.ToString(), Text = c.Client != null ? $"{c.ContractId} - {c.Client.ClientName}" : c.ContractId.ToString() }).ToList();
        }

        // status list
        ViewBag.StatusList = new SelectList(new[] { "Pending", "Completed" }, request?.ServiceStatus);
        return View(request);
    }

    // POST: ServiceRequests/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ServiceRequest request)
    {
        if (id != request.ServiceRequestId) return NotFound();

        if (ModelState.IsValid)
        {
            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PutAsync($"/api/servicerequests/{id}", content);
            if (response.IsSuccessStatusCode)
                return RedirectToAction(nameof(Index));
        }
        // repopulate contract dropdown and status list on error
        var resp2 = await _httpClient.GetAsync("/api/contracts");
        if (resp2.IsSuccessStatusCode)
        {
            var json3 = await resp2.Content.ReadAsStringAsync();
            var contracts2 = JsonSerializer.Deserialize<List<ST10398576_TechMoveGLMS.Models.Contract>>(json3, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<ST10398576_TechMoveGLMS.Models.Contract>();
            ViewBag.ContractId = contracts2.Select(c => new SelectListItem { Value = c.ContractId.ToString(), Text = c.Client != null ? $"{c.ContractId} - {c.Client.ClientName}" : c.ContractId.ToString() }).ToList();
        }
        ViewBag.StatusList = new SelectList(new[] { "Pending", "Completed" }, request.ServiceStatus);
        return View(request);
    }

    // GET: ServiceRequests/Delete/5
    public async Task<IActionResult> Delete(int id)
    {
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
        var response = await _httpClient.DeleteAsync($"/api/servicerequests/{id}");
        if (response.IsSuccessStatusCode)
            return RedirectToAction(nameof(Index));

        return NotFound();
    }
}
