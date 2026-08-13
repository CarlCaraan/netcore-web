using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.Data.SqlClient;
using netcore_crud.App_Code;
using netcore_crud.Models.Security;
using netcore_crud.Models.Shared;
using System.Data;
using System.Security.AccessControl;

namespace netcore_crud.Controllers
{

    // Module para mag-manage ng security groups at kanilang permissions.
    [Route("sys-security")]
    public class SecurityController : BaseController
    {
        public string ModuleName = "SECURITY";
        public string ModuleAbbr = "SEC";
        // Configuration ng main table at mga fields na ginagamit ng module.
        string PubTableName = "GroupName";
        string PubPrimary = "GroupName";
        string PubSecondary = "Description";
        string PubThird = "CanChangePW";

        private readonly IConfiguration _configuration;

        public SecurityController(IConfiguration configuration)
        {
            // Database configuration na ipapasa sa shared module helpers.
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



        // Binubuo ang main record at detail permissions para sa napiling group.
        private ModuleViewModel<SecurityModel> BuildModel(string? pubPrimaryValue)
        {
            var model = Commons.LoadModule<SecurityModel>(
                _configuration,
                PubTableName,
                PubPrimary,
                pubPrimaryValue);

            // Kapag may napiling group, isama ang related authority at menu records.
            if (!string.IsNullOrWhiteSpace(pubPrimaryValue))
            {
                model.Common.UserAuthority = Commons.LoadModuleDetail<UserAuthorityModel>(
                    _configuration,
                    "UserAuthority",
                    "GroupName",
                    model.Record.GroupName);

                model.Common.MenuChoice = Commons.LoadModuleDetail<MenuChoiceModel>(
                    _configuration,
                    "MenuChoice",
                    "GroupName",
                    model.Record.GroupName);

            }

            return model;
        }

        [HttpGet("{PubPrimaryValue?}")]
        // Ipakita ang module; optional ang group name para sa selected record.
        public IActionResult Index(string? PubPrimaryValue)
        {
            return View(BuildModel(PubPrimaryValue));
        }



        [HttpPost("new")]
        // Maghanda ng blank record na puwedeng i-save bilang bagong security group.
        public IActionResult New()
        {
            var model = BuildModel(null);

            model.Common.Mode = "NEW";
            model.Common.IsReadOnly = false;

            // I-set ang current user at timestamps para sa bagong record.
            model.Record.CreatedBy = HttpContext.Session.GetString("UserName") ?? string.Empty;
            model.Record.CreatedDate = DateTime.Now;
            model.Record.ModifiedBy = HttpContext.Session.GetString("UserName") ?? string.Empty;
            model.Record.ModifiedDate = DateTime.Now;

            // Default values ng fields sa bagong group.
            model.Record.GroupName = "";
            model.Record.Description = "";
            model.Record.CanChangePW = false;

            return View("Index", model);
        }


        [HttpPost("save")]
        // I-save ang bagong group o update sa existing group.
        public IActionResult Save(ModuleViewModel<SecurityModel> model)
        {
            try
            {
                // SaveModule ang may duplicate primary-value validation at database transaction.
                Commons.SaveModule(
                    _configuration,
                    PubTableName,
                    PubPrimary,
                    model.Record,
                    model.Common.Mode,
                    model.Common.OriginalPubPrimary);

                Commons.SaveModuleDetail(
                    _configuration,
                    "UserAuthority",
                    "GroupName",
                    model.Record.GroupName,
                    model.Common.Mode,
                    model.Common.UserAuthority);


                //Commons.InsertModuleDetail<MenuChoiceModel>(
                //    _configuration,
                //    "MenuChoice",
                //    "GroupName",
                //    model.Record.GroupName,
                //    model.Common.MenuChoice);


                // I-reload ang saved data bago magpakita ng success message.
                var viewModel = BuildModel(model.Record.GroupName);
                viewModel.Common.Message = Commons.ShowMessage("success", "Record saved successfully.");

                ModelState.Remove("Common.Mode");
                ModelState.Remove("Common.OriginalPubPrimary");


                return View("Index", viewModel);
            }
            catch (Exception ex)
            {
                // Ibalik ang form kasama ang validation o database error.
                model.Common.Message = Commons.ShowMessage("danger", ex.Message);

                return View("Index", model);
            }
        }


        [HttpPost("select")]
        // I-redirect sa selected group para ma-load ang complete record details.
        public IActionResult Select(String PubPrimaryValue)
        {
            //var model = Commons.LoadModule<SecurityModel>(
            //    _configuration,
            //    "GroupName",
            //    rowStamp);

            return RedirectToAction(nameof(Index), new { PubPrimaryValue });
        }



        [HttpPost("searchviewlist")]
        // I-filter ang listahan ng groups gamit ang search text mula sa modal/list view.
        public IActionResult SearchViewList(ModuleViewModel<SecurityModel> model)
        {
            var viewModel = Commons.LoadModule<SecurityModel>(
                _configuration,
                PubTableName,
                PubPrimary,
                null,
                model.Common.txtSearch);

            // Ibalik ang modal at panatilihin ang search keyword sa screen.
            viewModel.Common.txtSearch = model.Common.txtSearch;
            ViewBag.ShowModal = true;
            return View("Index", viewModel);
        }


        [HttpPost("delete")]
        // I-delete ang selected group at ang mga related detail records.
        public IActionResult Delete(ModuleViewModel<SecurityModel> model)
        {
            try
            {
                // DeleteModule ang may database transaction para sa main record at related detail records.
                Commons.DeleteModule(
                    _configuration,
                    PubTableName,
                    PubPrimary,
                    model.Record,
                    ("UserAuthority", PubPrimary, model.Record),
                    ("MenuChoice", PubPrimary, model.Record));


                // I-reload ang data pagkatapos ma-delete bago magpakita ng success message.
                var viewModel = BuildModel(null);
                viewModel.Common.Message = Commons.ShowMessage("success", "Record deleted successfully.");

                ModelState.Clear();

                return View("Index", viewModel);
            }
            catch (Exception ex)
            {
                // Ibalik ang form kasama ang validation o database error.
                model.Common.Message = Commons.ShowMessage("danger", ex.Message);

                return View("Index", model);
            }
        }




    }
}
