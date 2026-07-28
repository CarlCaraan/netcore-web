using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Dapper;
using System.Data;
using netcore_crud.Models;
using netcore_crud.App_Code;


namespace netcore_crud.Controllers
{
    public class LoginController : Controller
    {
        private readonly IConfiguration _configuration;

        private readonly ILogger<LoginController> _logger;

        public LoginController(ILogger<LoginController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }


        [HttpPost]
        public IActionResult Index(string Username, string Password)
        {
            string connStr = _configuration.GetConnectionString("NetCoreConnection");

            if (string.IsNullOrEmpty(connStr))
            {
                throw new Exception("Connection string is NULL!");
            }

            using SqlConnection myconn = new SqlConnection(connStr);

            string sql = @"SELECT GroupName, ISNULL(Hold,0) AS Hold FROM LoginNames WHERE UserName = @UserName AND CONVERT(VARCHAR(100), DECRYPTBYPASSPHRASE('PHILSOFT', Password)) = @Password";

            var user = myconn.QueryFirstOrDefault<LoginResult>(
                sql,
                new
                {
                    UserName = Username.Trim(),
                    Password
                });

            if (user != null)
            {
                if ((bool)user.Hold == false)
                {
                    HttpContext.Session.SetString("UserName", Username);
                    HttpContext.Session.SetString("GroupName", user.GroupName);

                    InsertLoginLogs(Username);

                    if (Username.ToUpper() == "ADMINISTRATOR")
                    {
                        //return Redirect("/login-success");
                        return RedirectToAction("LoginSuccess", "Dashboard");
                    }

                    return RedirectToAction("Dashboard", "Dashboard");
                }

                ViewBag.Message = Commons.ShowMessage("warning", "Username is on hold!");
                return View();
            }

            ViewBag.Message = Commons.ShowMessage("danger", "Invalid Username or Password!");

            return View();
        }

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

        public IActionResult Logout()
        {
            string? userName = HttpContext.Session.GetString("UserName");

            if (!string.IsNullOrEmpty(userName))
            {
                InsertLogoutLogs(userName);
            }

            HttpContext.Session.Clear();

            return RedirectToAction("Index", "Login");
        }

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
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

    }
}
