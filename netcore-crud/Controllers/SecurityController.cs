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

        private (CommonViewModel Common, SecurityModel Security) BuildModel(long? rowStamp)
        {
            return Commons.LoadModule<SecurityModel>(
                _configuration,
                "GroupName",
                rowStamp);
        }

        [HttpGet("{rowStamp?}")]
        public IActionResult Index(long? rowStamp)
        {
            return View(BuildModel(rowStamp));
        }



        [HttpPost("new")]
        public IActionResult New()
        {
            var model = BuildModel(null);

            model.Common.Mode = "NEW";
            model.Common.IsReadOnly = false;

            model.Security.CreatedBy = HttpContext.Session.GetString("UserName");
            model.Security.CreatedDate = DateTime.Now;
            model.Security.ModifiedBy = HttpContext.Session.GetString("UserName");
            model.Security.ModifiedDate = DateTime.Now;

            model.Security.GroupName = "";
            model.Security.Description = "";
            model.Security.CanChangePW = false;

            return View("Index", model);
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
