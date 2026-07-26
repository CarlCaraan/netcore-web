using Microsoft.AspNetCore.Mvc;

namespace netcore_crud.Controllers
{
    public class DashboardController : BaseController
    {
        public IActionResult LoginSuccess()
        {
            return View();
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

    }
}
