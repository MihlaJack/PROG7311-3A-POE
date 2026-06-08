using GLMS.Web.Models;
using GLMS.Web.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GLMS.Web.Controllers
{
    public class AuthController : Controller
    {
        private readonly IGLMSApiClient _apiClient;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IGLMSApiClient apiClient, ILogger<AuthController> logger)
        {
            _apiClient = apiClient;
            _logger = logger;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var response = await _apiClient.LoginAsync(model);

            if (response == null)
            {
                ModelState.AddModelError("", "Invalid username or password.");
                return View(model);
            }

            // Store JWT token in session
            HttpContext.Session.SetString("JWTToken", response.Token);

            // Create cookie-based authentication
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, response.Username),
                new Claim(ClaimTypes.Role, response.Role),
                new Claim("JWTToken", response.Token)
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = model.RememberMe,
                ExpiresUtc = response.ExpiresAt
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            _logger.LogInformation("User {Username} logged in successfully", response.Username);

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var success = await _apiClient.RegisterAsync(model);

            if (!success)
            {
                ModelState.AddModelError("", "Registration failed. Username or email may already exist.");
                return View(model);
            }

            _logger.LogInformation("User {Username} registered successfully", model.Username);
            TempData["Success"] = "Registration successful! Please log in.";
            return RedirectToAction("Login");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.Session.Remove("JWTToken");
            _logger.LogInformation("User logged out");
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}