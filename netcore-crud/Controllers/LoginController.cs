using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using netcore_crud.App_Code;
using netcore_crud.Models;
using netcore_crud.Models.Shared;
using System.Data;
using System.Diagnostics;


namespace netcore_crud.Controllers
{
    // Humahawak sa login, logout, at pag-log ng user activity.
    public class LoginController : Controller
    {
        private readonly IConfiguration _configuration;

        private readonly ILogger<LoginController> _logger;

        public LoginController(ILogger<LoginController> logger, IConfiguration configuration)
        {
            // Kunin ang logger at database settings na ginagamit ng controller.
            _logger = logger;
            _configuration = configuration;
        }

        [HttpGet]
        // Ipakita ang login form.
        public IActionResult Index()
        {
            string? userName = HttpContext.Session.GetString("UserName");
            if (!string.IsNullOrWhiteSpace(userName))
            {
                return Redirect("/dashboard/LoginSuccess");
            }

            return View();
        }


        [HttpPost]
        // I-validate ang login credentials at gumawa ng session kapag valid.
        public IActionResult Index(LoginModel model)
        {
            // Kunin ang database connection na naka-configure sa appsettings.
            string connStr = _configuration.GetConnectionString("NetCoreConnection");

            if (string.IsNullOrEmpty(connStr))
            {
                throw new Exception("Connection string is NULL!");
            }
            var errors = ModelState
                .Where(x => x.Value.Errors.Count > 0)
                .Select(x => new
                {
                    Field = x.Key,
                    Errors = x.Value.Errors.Select(e => e.ErrorMessage).ToList()
                })
                .ToList();
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            using SqlConnection myconn = new SqlConnection(connStr);

            // Hanapin ang user at i-compare ang decrypted password sa input password.
            string sql = @"SELECT GroupName, ISNULL(Hold,0) AS Hold FROM LoginNames WHERE UserName = @UserName AND CONVERT(VARCHAR(100), DECRYPTBYPASSPHRASE('PHILSOFT', Password)) = @Password";

            var user = myconn.QueryFirstOrDefault<LoginModel>(
                sql,
                new
                {
                    UserName = model.Username.Trim(),
                    Password = model.Password
                });

            if (user != null)
            {
                // Hindi puwedeng mag-login ang account na naka-hold.
                if ((bool)user.Hold == false)
                {
                    // I-save ang user details sa session para magamit sa protected pages.
                    HttpContext.Session.SetString("UserName", model.Username.Trim());
                    HttpContext.Session.SetString("GroupName", user.GroupName);

                    // Mag-record ng successful login sa USERLOGS table.
                    InsertLoginLogs(model.Username.Trim());

                    // Iba ang landing page ng Administrator.
                    if (model.Username.Trim().ToUpper() == "ADMINISTRATOR")
                    {
                        //return Redirect("/login-success");
                        return RedirectToAction("LoginSuccess", "Dashboard");
                    }

                    return RedirectToAction("LoginSuccess", "Dashboard");
                    //return RedirectToAction("Index", "Dashboard");
                }

                // Ipakita ang dahilan kung bakit hindi pinapayagang mag-login.
                ViewBag.Message = Commons.ShowMessage("warning", "Username is on hold!");
                return View(model);
            }

            // Walang nahanap na matching username at password.
            ViewBag.Message = Commons.ShowMessage("danger", "Invalid Username or Password!");

            return View(model);
        }

        // Nagse-save ng login date at time para sa audit trail.
        private void InsertLoginLogs(string Username)
        {
            string connStr = _configuration.GetConnectionString("NetCoreConnection");

            using SqlConnection myconn = new SqlConnection(connStr);

            string sql = @"
                INSERT INTO USERLOGS ( UserName, TimeLog, DateLog, TypeLog) VALUES (@UserName, @LogTime, @LogDate, 'LOGIN')";

            myconn.Execute(sql,
                new
                {
                    UserName = Username,
                    LogTime = DateTime.Now.ToShortTimeString(),
                    LogDate = DateTime.Now.ToShortDateString()
                });
        }

        // I-log ang logout, linisin ang session, saka bumalik sa login page.
        public IActionResult Logout()
        {
            string? userName = HttpContext.Session.GetString("UserName");

            if (!string.IsNullOrEmpty(userName))
            {
                // May username lang na dapat i-log bago i-clear ang session.
                InsertLogoutLogs(userName);
            }

            HttpContext.Session.Clear();

            return RedirectToAction("Index", "Login");
        }

        // Nagse-save ng logout date at time para sa audit trail.
        private void InsertLogoutLogs(string userName)
        {
            string connStr = _configuration.GetConnectionString("NetCoreConnection")!;

            using SqlConnection myconn = new SqlConnection(connStr);

            string sql = @"INSERT INTO USERLOGS (UserName, TimeLog, DateLog, TypeLog) VALUES (@UserName, @TimeLog, @DateLog, 'LOGOUT')";

            myconn.Execute(sql,
                new
                {
                    UserName = userName,
                    TimeLog = DateTime.Now.ToShortTimeString(),
                    DateLog = DateTime.Now.ToShortDateString()
                });
        }



        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        // Default error page; walang cache para laging fresh ang error details.
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

    }
}
