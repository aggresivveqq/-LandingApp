using LandingApp.Models;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace LandingApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        private static readonly string[] SupportedCultures = { "ru", "kk" };

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        [Route("/")]
        public IActionResult RedirectToDefaultCulture()
        {
            return RedirectToAction("Index", new { culture = "ru" });
        }

        [Route("{culture}/")]
        public IActionResult Index(string culture = "ru")
        {
            if (!SupportedCultures.Contains(culture))
            {
                return RedirectToAction("Index", new { culture = "ru" });
            }

            ViewData["Culture"] = culture;
            return View($"~/Views/{culture}/Index.cshtml");
        }

        [Route("{culture}/Privacy")]
        public IActionResult Privacy(string culture = "ru")
        {
            if (!SupportedCultures.Contains(culture))
            {
                return RedirectToAction("Privacy", new { culture = "ru" });
            }

            ViewData["Culture"] = culture;
            return View($"~/Views/{culture}/Privacy.cshtml");
        }

        [Route("{culture}/Contract")]
        public IActionResult Contract(string culture = "ru")
        {
            if (!SupportedCultures.Contains(culture))
            {
                return RedirectToAction("Contract", new { culture = "ru" });
            }

            ViewData["Culture"] = culture;
            return View($"~/Views/{culture}/Contract.cshtml");
        }

        [Route("Error")]
        public IActionResult Error()
        {
            var exceptionHandlerPathFeature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();
            if (exceptionHandlerPathFeature != null)
            {
                _logger.LogError(exceptionHandlerPathFeature.Error, "Unhandled exception caught in Error page.");
                ViewBag.ErrorMessage = exceptionHandlerPathFeature.Error.Message;
            }

            return View("~/Views/Shared/Error.cshtml");
        }
    }
}
