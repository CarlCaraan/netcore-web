using Dapper;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.Data.SqlClient;
using netcore_crud.App_Code;
using netcore_crud.Authorization;
using netcore_crud.Caching;
using netcore_crud.Models.Security;
using netcore_crud.Models.Shared;
using System.Data;

namespace netcore_crud.Controllers
{

    // Module para mag-manage ng security groups at kanilang permissions.
    [Route("sys-security")]
    public class SecurityController : BaseController
    {
        public const string ModuleName = "SECURITY";
        public string ModuleAbbr = "SEC";
        // Configuration ng main table at mga fields na ginagamit ng module.
        string PubTableName = "GroupName";
        string PubPrimary = "GroupName";
        string PubSecondary = "Description";
        string PubThird = "CanChangePW";

        private readonly IConfiguration _configuration;
        private readonly CacheManager _cacheManager;
        private readonly IValidator<SecurityModel> _validator;

        public SecurityController(IConfiguration configuration, IValidator<SecurityModel> validator, CacheManager cacheManager)
        {
            _configuration = configuration;
            _validator = validator;
            _cacheManager = cacheManager;
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

                model.Common.LoginNames = Commons.LoadModuleDetail<LoginNamesModel>(
                    _configuration,
                    "LoginNames",
                    "GroupName",
                    model.Record.GroupName);

            }
            else {
                model.Common.IsDisable = true;
            }

            return model;
        }

        [Permission(ModuleName, "READ")]
        [HttpGet("{PubPrimaryValue?}")]
        // Ipakita ang module; optional ang group name para sa selected record.
        public IActionResult Index(string? PubPrimaryValue)
        {
            return View(BuildModel(PubPrimaryValue));
        }


        [Permission(ModuleName, "INSERT")]
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

            return View("Index", model);
        }


        [HttpPost("save")]
        // I-save ang bagong group o update sa existing group.
        public async Task<IActionResult> Save(ModuleViewModel<SecurityModel> model)
        {
            //if (!ModelState.IsValid)
            //{
            //    foreach (var error in ModelState)
            //    {
            //        foreach (var message in error.Value.Errors)
            //        {
            //            Console.WriteLine($"{error.Key}: {message.ErrorMessage}");
            //        }
            //    }
            //}

            if (string.IsNullOrWhiteSpace(model.Common.Mode))
            {
                model.Common.Message = Commons.ShowMessage("danger", "Select or click New first before saving record");
                return View("Index", model);
            }

            //FluentValidator
            var validationResult = await _validator.ValidateAsync(model.Record);
            if (!validationResult.IsValid)
            {
                foreach (var error in validationResult.Errors)
                {
                    ModelState.AddModelError($"Record.{error.PropertyName}", error.ErrorMessage);
                }

                model = BuildModel(null);

                return View("Index", model);
            }


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

                SaveModuleDetail(
                    _configuration,
                    "UserAuthority",
                    "GroupName",
                    model.Record.GroupName,
                    model.Common.Mode,
                    model.Common.UserAuthority);

                SaveModuleDetail(
                    _configuration,
                    "MenuChoice",
                    "GroupName",
                    model.Record.GroupName,
                    model.Common.Mode,
                    model.Common.MenuChoice);


                // I-reload ang saved data bago magpakita ng success message.
                var viewModel = BuildModel(model.Record.GroupName);
                viewModel.Common.Message = Commons.ShowMessage("success", "Record saved successfully.");

                ModelState.Remove("Common.Mode");
                ModelState.Remove("Common.OriginalPubPrimary");

                _cacheManager.Clear(CacheKeys.Dashboard);

                return View("Index", viewModel);
            }
            catch (Exception ex)
            {
                // Ibalik ang form kasama ang validation o database error.
                model.Common.Message = Commons.ShowMessage("danger", ex.Message);

                return View("Index", model);
            }
        }

        [Permission(ModuleName, "EDIT")]
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

        [Permission(ModuleName, "DELETE")]
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

                _cacheManager.Clear(CacheKeys.Dashboard);

                return View("Index", viewModel);
            }
            catch (Exception ex)
            {
                // Ibalik ang form kasama ang validation o database error.
                model.Common.Message = Commons.ShowMessage("danger", ex.Message);

                return View("Index", model);
            }
        }

        [Permission(ModuleName, "EDIT")]
        [HttpPost("searchmodule")]
        // Maghanap ng pinakamalapit na record gamit ang mga field na may laman sa model.
        public IActionResult SearchModule(ModuleViewModel<SecurityModel> model)
        {
            // Gamitin ang reusable SearchModule mula sa Commons para hanapin ang matching record.
            // Ibabalik nito ang value ng primary key ng nahanap na record.
            string? pubPrimaryValue = Commons.SearchModule(
                _configuration,
                PubTableName,
                PubPrimary,
                model.Record);

            // I-redirect sa Index gamit ang nahanap na primary key.
            // Si Index/BuildModel na ang bahalang mag-load ng buong record at related details.
            return RedirectToAction(nameof(Index), new { PubPrimaryValue = pubPrimaryValue });
        }



        public static void SaveModuleDetail<T>(
            IConfiguration configuration,
            string tableName,
            string foreignKeyColumn,
            object foreignKeyValue,
            string mode,
            List<T>? details = null)
        {
            using SqlConnection myconn = new(configuration.GetConnectionString("NetCoreConnection"));
            myconn.Open();

            using SqlTransaction transaction = myconn.BeginTransaction();

            try
            {
                switch (tableName)
                {
                    case "UserAuthority":
                        if (mode.Equals("NEW", StringComparison.OrdinalIgnoreCase))
                        {
                            string query = $"INSERT INTO [{tableName}] ([Reads], [Inserts], [Edits], [Deletes], [{foreignKeyColumn}], [Application], [Active]) SELECT 1, 1, 1, 1, @ForeignKeyValue, [Application], 1 FROM [{tableName}] WHERE [{foreignKeyColumn}] = 'ADMINISTRATOR'";

                            myconn.Execute(query, new { ForeignKeyValue = foreignKeyValue }, transaction);
                        }
                        else if (mode.Equals("EDIT", StringComparison.OrdinalIgnoreCase))
                        {
                            string deleteQuery = $"DELETE FROM [{tableName}] WHERE [{foreignKeyColumn}] = @ForeignKeyValue";

                            myconn.Execute(deleteQuery, new { ForeignKeyValue = foreignKeyValue }, transaction);

                            if (details != null && details.Count > 0)
                            {
                                string insertQuery = $"INSERT INTO [{tableName}] ([Reads], [Inserts], [Edits], [Deletes], [{foreignKeyColumn}], [Application], [Active]) VALUES (@Reads, @Inserts, @Edits, @Deletes, @ForeignKeyValue, @Application, @Active)";

                                foreach (T detail in details)
                                {
                                    UserAuthorityModel item = (UserAuthorityModel)(object)detail;

                                    myconn.Execute(
                                        insertQuery,
                                        new
                                        {
                                            item.Reads,
                                            item.Inserts,
                                            item.Edits,
                                            item.Deletes,
                                            ForeignKeyValue = foreignKeyValue,
                                            Application = item.Application ?? "",
                                            item.Active
                                        },
                                        transaction);
                                }
                            }
                        }
                        break;

                    case "MenuChoice":
                        if (mode.Equals("NEW", StringComparison.OrdinalIgnoreCase))
                        {
                            string query = $"INSERT INTO [{tableName}] ([{foreignKeyColumn}], [Application], [Menu], [Access], [Active]) SELECT @ForeignKeyValue, [Application], [Menu], [Access], 1 FROM [{tableName}] WHERE [{foreignKeyColumn}] = 'ADMINISTRATOR'";

                            myconn.Execute(query, new { ForeignKeyValue = foreignKeyValue }, transaction);
                        }
                        else if (mode.Equals("EDIT", StringComparison.OrdinalIgnoreCase))
                        {
                            string deleteQuery = $"DELETE FROM [{tableName}] WHERE [{foreignKeyColumn}] = @ForeignKeyValue";

                            myconn.Execute(deleteQuery, new { ForeignKeyValue = foreignKeyValue }, transaction);

                            if (details != null && details.Count > 0)
                            {
                                string insertQuery = $"INSERT INTO [{tableName}] ([{foreignKeyColumn}], [Application], [Menu], [Access]) VALUES (@ForeignKeyValue, @Application, @Menu, @Access)";

                                foreach (T detail in details)
                                {
                                    MenuChoiceModel item = (MenuChoiceModel)(object)detail;

                                    myconn.Execute(
                                        insertQuery,
                                        new
                                        {
                                            ForeignKeyValue = foreignKeyValue,
                                            Application = item.Application ?? "",
                                            Menu = item.Menu ?? "",
                                            item.Access
                                        },
                                        transaction);
                                }
                            }
                        }
                        break;

                    default:
                        throw new ArgumentException($"Unsupported detail table: {tableName}");
                }

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }



    }
}
