using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using ST10398576_TechMoveGLMS.Models;

namespace ST10398576_TechMoveGLMS.Controllers
{
    public class ClientsController : Controller
    {
        private readonly IHttpClientFactory _httpFactory;

        public ClientsController(IHttpClientFactory httpFactory)
        {
            _httpFactory = httpFactory;
        }

        private HttpClient CreateApiClient()
        {
            var client = _httpFactory.CreateClient("Api");
            string? token = HttpContext?.Session?.GetString("JwtToken");
            if (string.IsNullOrEmpty(token))
            {
                var auth = HttpContext?.Request?.Headers["Authorization"].FirstOrDefault();
                if (!string.IsNullOrEmpty(auth) && auth.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                {
                    token = auth.Substring("Bearer ".Length).Trim();
                }
            }

            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
            return client;
        }

        public async Task<IActionResult> Index()
        {
            var client = CreateApiClient();
            if (client == null) return RedirectToAction("Login", "Account");

            var resp = await client.GetAsync("api/clients");
            if (!resp.IsSuccessStatusCode) return StatusCode((int)resp.StatusCode);

            var clients = await resp.Content.ReadFromJsonAsync<Client[]>();
            return View(clients);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Client clientModel)
        {
            if (!ModelState.IsValid) return View(clientModel);

            var client = CreateApiClient();
            if (client == null) return RedirectToAction("Login", "Account");

            var resp = await client.PostAsJsonAsync("api/clients", clientModel);
            if (!resp.IsSuccessStatusCode)
            {
                ModelState.AddModelError(string.Empty, "Unable to create client.");
                return View(clientModel);
            }

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Details(int id)
        {
            var client = CreateApiClient();
            if (client == null) return RedirectToAction("Login", "Account");

            var resp = await client.GetAsync($"api/clients/{id}");
            if (!resp.IsSuccessStatusCode) return NotFound();

            var model = await resp.Content.ReadFromJsonAsync<Client>();
            return View(model);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var client = CreateApiClient();
            if (client == null) return RedirectToAction("Login", "Account");

            var resp = await client.GetAsync($"api/clients/{id}");
            if (!resp.IsSuccessStatusCode) return NotFound();

            var model = await resp.Content.ReadFromJsonAsync<Client>();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Client clientModel)
        {
            if (id != clientModel.ClientId) return BadRequest();
            if (!ModelState.IsValid) return View(clientModel);

            var client = CreateApiClient();
            if (client == null) return RedirectToAction("Login", "Account");

            var resp = await client.PutAsJsonAsync($"api/clients/{id}", clientModel);
            if (!resp.IsSuccessStatusCode)
            {
                ModelState.AddModelError(string.Empty, "Unable to update client.");
                return View(clientModel);
            }

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Delete(int id)
        {
            var client = CreateApiClient();
            if (client == null) return RedirectToAction("Login", "Account");

            var resp = await client.GetAsync($"api/clients/{id}");
            if (!resp.IsSuccessStatusCode) return NotFound();

            var model = await resp.Content.ReadFromJsonAsync<Client>();
            return View(model);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var client = CreateApiClient();
            if (client == null) return RedirectToAction("Login", "Account");

            var resp = await client.DeleteAsync($"api/clients/{id}");
            if (!resp.IsSuccessStatusCode) return StatusCode((int)resp.StatusCode);

            return RedirectToAction("Index");
        }
    }
}
