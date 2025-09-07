using Microsoft.AspNetCore.Mvc;

namespace YourNamespace.Controllers
{
    public class LogsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
