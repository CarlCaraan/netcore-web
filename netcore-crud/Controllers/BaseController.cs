using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Net.Http.Headers;

namespace netcore_crud.Controllers
{
    // Base controller para required ang active login sa mga protected pages.
    public class BaseController : Controller
    {
        // Tumatawag bago ang bawat action para i-check ang session ng user.
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            // Iwasan na bumalik sa protected page gamit ang browser back button.
            Response.Headers[HeaderNames.CacheControl] = "no-cache, no-store, must-revalidate";
            Response.Headers[HeaderNames.Pragma] = "no-cache";
            Response.Headers[HeaderNames.Expires] = "0";

            var userName = HttpContext.Session.GetString("UserName");

            // Kapag walang session, ibalik agad sa login page.
            if (string.IsNullOrEmpty(userName))
            {
                context.Result = RedirectToAction("Index", "Login");
                return;
            }

            base.OnActionExecuting(context);
        }
    }
}
