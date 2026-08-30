using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Caching.Memory;
using netcore_crud.Authorization;
using netcore_crud.Caching;
using netcore_crud.Models;
using netcore_crud.Models.Dashboard;
using System.Data;
using System.Diagnostics;

namespace netcore_crud.Controllers
{
    // Mga page na makikita lang ng naka-login na user.
    public class DashboardController : BaseController
    {
        public const string ModuleName = "DASHBOARD";
        private readonly IConfiguration _configuration;
        private readonly IMemoryCache _cache;
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(IConfiguration configuration, IMemoryCache cache, ILogger<DashboardController> logger)
        {
            _configuration = configuration;
            _cache = cache;
            _logger = logger;
        }




        // Landing page para sa Administrator pagkatapos mag-login.
        public IActionResult LoginSuccess()
        {
            ViewBag.Message = HttpContext.Session.GetString("PermissionMessage");
            HttpContext.Session.Remove("PermissionMessage");

            return View();
        }

        // Main dashboard ng normal user.
        [Permission(ModuleName, "READ")]
        public IActionResult Index()
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            if (_cache.TryGetValue(CacheKeys.Dashboard, out DashboardModel? cachedModel))
            {
                stopwatch.Stop();

                _logger.LogInformation(  "Dashboard cache HIT: {ElapsedMs:F2} ms", stopwatch.Elapsed.TotalMilliseconds);
                return View(cachedModel);
            }

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

            _cache.Set(
                CacheKeys.Dashboard,
                model,
                new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
                });

            stopwatch.Stop();

            _logger.LogInformation("Dashboard cache MISS: {ElapsedMs:F2} ms", stopwatch.Elapsed.TotalMilliseconds);

            return View(model);
        }

        // Privacy page ng application.
        public IActionResult Privacy()
        {
            return View();
        }

    }
}
