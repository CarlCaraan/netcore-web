using Microsoft.AspNetCore.Mvc;

namespace netcore_crud.Controllers
{
    // Mga page na makikita lang ng naka-login na user.
    public class DashboardController : BaseController
    {
        // Landing page para sa Administrator pagkatapos mag-login.
        public IActionResult LoginSuccess()
        {
            return View();
        }

        // Main dashboard ng normal user.
        public IActionResult Index()
        {
            ViewBag.Message = HttpContext.Session.GetString("PermissionMessage");

            HttpContext.Session.Remove("PermissionMessage");

            return View();
        }

        // Privacy page ng application.
        public IActionResult Privacy()
        {
            return View();
        }

    }
}
