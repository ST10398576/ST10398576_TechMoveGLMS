using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;

namespace ST10398576_TechMoveGLMS.Controllers
{
    public class AccountController : Controller
    {
        private readonly IHttpClientFactory _httpFactory;
        private readonly IConfiguration _config;
        private readonly ILogger<AccountController> _logger;

        public AccountController(IHttpClientFactory httpFactory, IConfiguration config, ILogger<AccountController> logger)
        {
            _httpFactory = httpFactory;
            _config = config;
            _logger = logger;
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(Models.LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var loginModel = new { Username = model.Username, Password = model.Password };
            var client = _httpFactory.CreateClient("Api");

            HttpResponseMessage response;
            try
            {
                response = await client.PostAsJsonAsync("api/auth/login", loginModel);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Error contacting API for login");
                ModelState.AddModelError(string.Empty, "Unable to contact authentication server.");
                return View();
            }

            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                return View(model);
            }

            // API returns JSON { token: "..." }
            try
            {
                var obj = await response.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
                if (obj.ValueKind == System.Text.Json.JsonValueKind.Object && obj.TryGetProperty("token", out var tok))
                {
                    var token = tok.GetString();
                    if (!string.IsNullOrEmpty(token))
                    {
                        // store token in server-side session (session cookie persists until browser closed)
                        HttpContext.Session.SetString("JwtToken", token);
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Invalid authentication response from server.");
                        return View(model);
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid authentication response from server.");
                    return View(model);
                }
            }
            catch
            {
                ModelState.AddModelError(string.Empty, "Invalid authentication response from server.");
                return View(model);
            }

            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> Logout()
        {
            HttpContext.Session.Remove("JwtToken");
            return RedirectToAction("Index", "Home");
        }
    }
}
