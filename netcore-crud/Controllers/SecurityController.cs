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

        //private (CommonViewModel Common, SecurityModel Security) BuildModel(long? rowStamp)
        //{
        //    return Commons.LoadModule<SecurityModel>(
        //        _configuration,
        //        PubTableName,
        //        PubPrimary,
        //        rowStamp);
        //}



        private (CommonViewModel Common, SecurityModel Security) BuildModel(String PubPrimaryValue)
        {
            var model = Commons.LoadModule<SecurityModel>(
                _configuration,
                PubTableName,
                PubPrimary,
                PubPrimaryValue);

            //Load Detail Table
            if (!string.IsNullOrWhiteSpace(PubPrimaryValue))
            {
                model.Common.UserAuthority = Commons.LoadModuleDetail<UserAuthorityModel>(
                    _configuration,
                    "UserAuthority",
                    "GroupName",
                    model.Model.GroupName);

                model.Common.MenuChoice = Commons.LoadModuleDetail<MenuChoiceModel>(
                    _configuration,
                    "MenuChoice",
                    "GroupName",
                    model.Model.GroupName);

            }

            return model;
        }

        [HttpGet("{PubPrimaryValue?}")]
        public IActionResult Index(string PubPrimaryValue)
        {
            return View(BuildModel(PubPrimaryValue));
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


        [HttpPost("save")]
        public IActionResult Save(
        [Bind(Prefix = "Item1")] CommonViewModel common,
        [Bind(Prefix = "Item2")] SecurityModel security)
        {
            try
            {
                Commons.SaveModule(_configuration, PubTableName, PubPrimary, security, common.Mode);

                //Commons.InsertModuleDetail<UserAuthorityModel>(
                //    _configuration,
                //    "UserAuthority",
                //    "GroupName",
                //    security.GroupName,
                //    common.UserAuthority);

                //Commons.InsertModuleDetail<MenuChoiceModel>(
                //    _configuration,
                //    "MenuChoice",
                //    "GroupName",
                //    security.GroupName,
                //    common.MenuChoice);

                var model = BuildModel(security.GroupName);
                model.Common.Message = Commons.ShowMessage("success", "Record saved successfully.");

                return View("Index", model);
            }
            catch (Exception ex) {
                var model = BuildModel(security.GroupName);
                model.Common.Message = Commons.ShowMessage("danger", ex.Message);

                return View("Index", model);
            }
        }


        [HttpPost("select")]
        public IActionResult Select(String PubPrimaryValue)
        {
            //var model = Commons.LoadModule<SecurityModel>(
            //    _configuration,
            //    "GroupName",
            //    rowStamp);

            return RedirectToAction(nameof(Index), new { PubPrimaryValue });
        }



        [HttpPost("searchviewlist")]
        public IActionResult SearchViewList([FromForm(Name = "Item1.txtSearch")] string txtSearch)
        {
            var model = Commons.LoadModule<SecurityModel>(
                _configuration,
                PubTableName,
                PubPrimary,
                null,
                txtSearch);

            model.Common.txtSearch = txtSearch;
            ViewBag.ShowModal = true;
            return View("Index", model);
        }






    }
}
