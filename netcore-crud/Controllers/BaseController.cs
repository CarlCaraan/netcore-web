using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Net.Http.Headers;

namespace netcore_crud.Controllers
{
    public class BaseController : Controller
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            Response.Headers[HeaderNames.CacheControl] = "no-cache, no-store, must-revalidate";
            Response.Headers[HeaderNames.Pragma] = "no-cache";
            Response.Headers[HeaderNames.Expires] = "0";

            var userName = HttpContext.Session.GetString("UserName");

            if (string.IsNullOrEmpty(userName))
            {
                context.Result = RedirectToAction("Index", "Login");
                return;
            }

            base.OnActionExecuting(context);
        }
    }
}
