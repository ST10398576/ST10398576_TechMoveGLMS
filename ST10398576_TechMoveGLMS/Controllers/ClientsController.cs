using Microsoft.AspNetCore.Mvc;
using ST10398576_TechMoveGLMS.Models;
using System.Text.Json;
using System.Text;

public class ClientsController : Controller
{
    private readonly HttpClient _httpClient;

    public ClientsController(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient();
        _httpClient.BaseAddress = new Uri("https://localhost:7066"); // API base URL
    }

    public async Task<IActionResult> Index()
    {
        var response = await _httpClient.GetAsync("/api/clients");
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        var clients = JsonSerializer.Deserialize<List<Client>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        return View(clients);
    }

    public async Task<IActionResult> Details(int id)
    {
        var response = await _httpClient.GetAsync($"/api/clients/{id}");
        if (!response.IsSuccessStatusCode) return NotFound();

        var json = await response.Content.ReadAsStringAsync();
        var client = JsonSerializer.Deserialize<Client>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        return View(client);
    }

    public IActionResult Create() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Client client)
    {
        if (ModelState.IsValid)
        {
            var json = JsonSerializer.Serialize(client);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("/api/clients", content);
            if (response.IsSuccessStatusCode)
                return RedirectToAction(nameof(Index));
        }
        return View(client);
    }

    public async Task<IActionResult> Edit(int id)
    {
        var response = await _httpClient.GetAsync($"/api/clients/{id}");
        if (!response.IsSuccessStatusCode) return NotFound();

        var json = await response.Content.ReadAsStringAsync();
        var client = JsonSerializer.Deserialize<Client>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        return View(client);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Client client)
    {
        if (id != client.ClientId) return NotFound();

        if (ModelState.IsValid)
        {
            var json = JsonSerializer.Serialize(client);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PutAsync($"/api/clients/{id}", content);
            if (response.IsSuccessStatusCode)
                return RedirectToAction(nameof(Index));
        }
        return View(client);
    }

    public async Task<IActionResult> Delete(int id)
    {
        var response = await _httpClient.GetAsync($"/api/clients/{id}");
        if (!response.IsSuccessStatusCode) return NotFound();

        var json = await response.Content.ReadAsStringAsync();
        var client = JsonSerializer.Deserialize<Client>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        return View(client);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var response = await _httpClient.DeleteAsync($"/api/clients/{id}");
        if (response.IsSuccessStatusCode)
            return RedirectToAction(nameof(Index));

        return NotFound();
    }
}
