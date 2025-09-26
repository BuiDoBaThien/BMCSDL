using Microsoft.AspNetCore.Mvc;

namespace WorkScheduleApp.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            var user = HttpContext.Session.GetString("username");
            ViewBag.User = user ?? "Khách";
            return View();
        }
    }
}
