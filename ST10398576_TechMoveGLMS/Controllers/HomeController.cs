using Microsoft.AspNetCore.Mvc;
using ST10398576_TechMoveGLMS.Models;
using System.Text.Json;
using System.Net.Http.Headers;

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

        private void AttachToken()
        {
            var token = HttpContext.Session.GetString("JwtToken");
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }
        }

        public async Task<IActionResult> Index()
        {
            AttachToken();
            var response = await _httpClient.GetAsync("/api/home/dashboard");

            // If not authorized, redirect to login
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                return RedirectToAction("Login", "Account");
            }

            if (!response.IsSuccessStatusCode)
            {
                return View(new DashboardSummary()); // fallback empty object
            }

            var json = await response.Content.ReadAsStringAsync();
            var dashboard = JsonSerializer.Deserialize<DashboardSummary>(
                json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return View(dashboard);
        }
    }
}
