using Microsoft.AspNetCore.Mvc;
using ST10398576_TechMoveGLMS.Models;
using System.Text.Json;

namespace ST10398576_TechMoveGLMS.Controllers
{
    public class HomeController : Controller
    {
        private readonly HttpClient _httpClient;

        public HomeController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient();
            _httpClient.BaseAddress = new Uri("https://localhost:7066");
        }

        public async Task<IActionResult> Index()
        {
            var response = await _httpClient.GetAsync("/api/home/dashboard");
            if (!response.IsSuccessStatusCode)
            {
                return View(new DashboardSummary()); // empty object instead of null
            }

            var json = await response.Content.ReadAsStringAsync();
            var dashboard = JsonSerializer.Deserialize<DashboardSummary>(
                json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return View(dashboard);
        }
    }
}
