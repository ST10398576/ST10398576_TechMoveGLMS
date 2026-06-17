using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ST10398576_TechMoveGLMS.Models;
using System.Text.Json;
using System.Text;
using System.Net.Http.Headers;

public class ServiceRequestsController : Controller
{
    private readonly HttpClient _httpClient;

    public ServiceRequestsController(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient();
        _httpClient.BaseAddress = new Uri("https://localhost:7066"); // API base URL
    }

    private void AttachToken()
    {
        var token = HttpContext.Session.GetString("JwtToken");
        if (!string.IsNullOrEmpty(token))
        {
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
        }
    }

    // GET: ServiceRequests
    public async Task<IActionResult> Index(string? status, DateTime? startDate, DateTime? endDate)
    {
        AttachToken();
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
        AttachToken();
        var response = await _httpClient.GetAsync($"/api/servicerequests/{id}");
        if (!response.IsSuccessStatusCode) return NotFound();

        var json = await response.Content.ReadAsStringAsync();
        var request = JsonSerializer.Deserialize<ServiceRequest>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        return View(request);
    }

    // GET: ServiceRequests/Create
    public async Task<IActionResult> Create()
    {
        AttachToken();
        var response = await _httpClient.GetAsync("/api/contracts");
        if (!response.IsSuccessStatusCode)
        {
            ViewBag.Contracts = new List<SelectListItem>();
            return View();
        }

        var json = await response.Content.ReadAsStringAsync();
        var contracts = JsonSerializer.Deserialize<List<Contract>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<Contract>();

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
        AttachToken();
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
        if (resp.IsSuccessStatusCode)
        {
            var json2 = await resp.Content.ReadAsStringAsync();
            var contracts2 = JsonSerializer.Deserialize<List<Contract>>(json2, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<Contract>();
            ViewBag.Contracts = contracts2.Select(c => new SelectListItem { Value = c.ContractId.ToString(), Text = c.Client != null ? $"{c.ContractId} - {c.Client.ClientName}" : c.ContractId.ToString() }).ToList();
        }
        return View(request);
    }

    // GET: ServiceRequests/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        AttachToken();
        var response = await _httpClient.GetAsync($"/api/servicerequests/{id}");
        if (!response.IsSuccessStatusCode) return NotFound();

        var json = await response.Content.ReadAsStringAsync();
        var request = JsonSerializer.Deserialize<ServiceRequest>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        // populate contracts dropdown
        var resp = await _httpClient.GetAsync("/api/contracts");
        if (resp.IsSuccessStatusCode)
        {
            var json2 = await resp.Content.ReadAsStringAsync();
            var contracts = JsonSerializer.Deserialize<List<Contract>>(json2, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<Contract>();
            ViewBag.ContractId = contracts.Select(c => new SelectListItem { Value = c.ContractId.ToString(), Text = c.Client != null ? $"{c.ContractId} - {c.Client.ClientName}" : c.ContractId.ToString() }).ToList();
        }

        ViewBag.StatusList = new SelectList(new[] { "Pending", "Completed" }, request?.ServiceStatus);
        return View(request);
    }

    // POST: ServiceRequests/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ServiceRequest request)
    {
        AttachToken();
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
        var resp2 = await _httpClient.GetAsync("/api/contracts");
        if (resp2.IsSuccessStatusCode)
        {
            var json3 = await resp2.Content.ReadAsStringAsync();
            var contracts2 = JsonSerializer.Deserialize<List<Contract>>(json3, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<Contract>();
            ViewBag.ContractId = contracts2.Select(c => new SelectListItem { Value = c.ContractId.ToString(), Text = c.Client != null ? $"{c.ContractId} - {c.Client.ClientName}" : c.ContractId.ToString() }).ToList();
        }
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
