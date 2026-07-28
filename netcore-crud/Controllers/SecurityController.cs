using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.Data.SqlClient;
using netcore_crud.App_Code;
using netcore_crud.Models;
using System.Data;
using System.Security.AccessControl;

namespace netcore_crud.Controllers
{


    [Route("sys-security")]
    public class SecurityController : BaseController
    {
        public string ModuleName = "SECURITY";
        public string ModuleAbbr = "SEC";
        string PubTableName = "GroupName";
        string PubPrimary = "GroupName";
        string PubSecondary = "Description";
        string PubThird = "CanChangePW";

        private readonly IConfiguration _configuration;

        public SecurityController(IConfiguration configuration)
        {
            _configuration = configuration;
        }


        //private DataTable LoadViewList()
        //{
        //    using SqlConnection myconn = new SqlConnection(_configuration.GetConnectionString("NetCoreConnection"));

        //    string sql = $@"SELECT TOP 30 {PubPrimary}, {PubSecondary}, {PubThird} FROM {PubTableName} ORDER BY RowStamp DESC";

        //    DataTable dt = new();

        //    using SqlDataAdapter da = new SqlDataAdapter(sql, myconn);
        //    da.Fill(dt);

        //    return dt;
        //}

        //[HttpGet("")]
        //public IActionResult Index()
        //{
        //    var common = new CommonViewModel
        //    {
        //        ViewList = LoadViewList()
        //    };

        //    var security = new SecurityModel();
        //    return View((common, security));
        //}

        [HttpGet("{rowStamp?}")]
        public IActionResult Index(long? rowStamp)
        {
            var model = Commons.LoadModule<SecurityModel>(
                _configuration,
                "GroupName",
                rowStamp);

            return View(model);
        }



        [HttpPost("new")]
        public IActionResult New()
        {
            var common = new CommonViewModel
            {
                Mode = "NEW",
                //ViewList = LoadViewList(),
                IsReadOnly = false
            };

            var security = new SecurityModel
            {
                CreatedBy = HttpContext.Session.GetString("UserName"),
                CreatedDate = DateTime.Now,
                ModifiedBy = HttpContext.Session.GetString("UserName"),
                ModifiedDate = DateTime.Now,

                GroupName = "",
                Description = "",
                CanChangePW = false
            };


            return View("Index", (common, security));
        }



        [HttpPost("select")]
        public IActionResult Select(long rowStamp)
        {
            //var model = Commons.LoadModule<SecurityModel>(
            //    _configuration,
            //    "GroupName",
            //    rowStamp);

            return RedirectToAction(nameof(Index), new { rowStamp });
        }




    }
}
