using Microsoft.AspNetCore.Mvc;
using netcore_crud.Models;

namespace netcore_crud.Controllers
{
    [Route("sys-security")]
    public class SecurityController : BaseController
    {
        [HttpGet("")]
        public IActionResult Index()
        {
            return View(new CommonViewModel());
        }


        [HttpPost]
        public IActionResult New()
        {
            var model = new CommonViewModel
            {
                Mode = "NEW",
                CreatedBy = HttpContext.Session.GetString("UserName"),
                CreatedDate = DateTime.Now,
                ModifiedBy = HttpContext.Session.GetString("UserName"),
                ModifiedDate = DateTime.Now
            };

            return View("Index", model);
        }








    }
}
