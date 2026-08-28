using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using netcore_crud.Models;
using netcore_crud.Models.Dashboard;
using System.Data;

namespace netcore_crud.Controllers
{
    // Mga page na makikita lang ng naka-login na user.
    public class DashboardController : BaseController
    {
        private readonly IConfiguration _configuration;

        public DashboardController(IConfiguration configuration)
        {
            _configuration = configuration;
        }




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

            using SqlConnection myconn = new SqlConnection(_configuration.GetConnectionString("NetCoreConnection"));

            //var users = myconn.Query<DashboardUserModel>("PROC_DASHBOARD", new { DashboardType = "User" }, commandType: System.Data.CommandType.StoredProcedure).ToList();
            //var cardFiles = myconn.Query<DashboardCardFileModel>("PROC_DASHBOARD", new { DashboardType = "CardFile" }, commandType: System.Data.CommandType.StoredProcedure).ToList();
            //ViewBag.Users = users;
            //ViewBag.CardFiles = cardFiles;

            var model = new DashboardModel
            {
                Users = myconn.Query<DashboardUserModel>("PROC_DASHBOARD", new { DashboardType = "User" }, commandType: CommandType.StoredProcedure).ToList(),
                CardFiles = myconn.Query<DashboardCardFileModel>("PROC_DASHBOARD", new { DashboardType = "CardFile" }, commandType: CommandType.StoredProcedure).ToList()
            };

            return View(model);
        }

        // Privacy page ng application.
        public IActionResult Privacy()
        {
            return View();
        }

    }
}
