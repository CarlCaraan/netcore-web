using Dapper;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using netcore_crud.App_Code;
using netcore_crud.Authorization;
using netcore_crud.Caching;
using netcore_crud.Models.CardFile;
using netcore_crud.Models.Shared;
using System.Data;
using static System.Runtime.CompilerServices.RuntimeHelpers;

namespace netcore_crud.Controllers
{

    // Module para mag-manage ng security groups at kanilang permissions.
    [Route("sys-card-file")]
    public class CardFileController : BaseController
    {
        public const string ModuleName = "Card File";
        public string ModuleAbbr = "CRDF";
        // Configuration ng main table at mga fields na ginagamit ng module.
        string PubTableName = "CardFile";
        string PubPrimary = "Code";
        string PubSecondary = "Type";
        string PubThird = "Category";

        private readonly IConfiguration _configuration;
        private readonly CacheManager _cacheManager;
        private readonly IValidator<CardFileModel> _validator;

        public CardFileController(IConfiguration configuration, IValidator<CardFileModel> validator, CacheManager cacheManager)
        {
            _configuration = configuration;
            _validator = validator;
            _cacheManager = cacheManager;
        }



        // Binubuo ang main record at detail permissions para sa napiling group.
        private ModuleViewModel<CardFileModel> BuildModel(string? pubPrimaryValue)
        {
            var model = Commons.LoadModule<CardFileModel>(
                _configuration,
                PubTableName,
                PubPrimary,
                pubPrimaryValue);

            //Load Dropdowns


            // Kapag may napiling group, isama ang related authority at menu records.
            if (!string.IsNullOrWhiteSpace(pubPrimaryValue))
            {
                // Walang Detail
            }
            else
            {
                model.Common.IsDisable = false;
            }

            using var myconn = new SqlConnection(_configuration.GetConnectionString("NetCoreConnection"));
            model.Common.CardFileType = myconn.Query<SelectListItem>("SELECT CardFileType AS Value, CardFileType AS Text FROM CardFileTypeSetup ORDER BY CardFileType").ToList();
            model.Common.CardFileCategory = myconn.Query<SelectListItem>("SELECT CardFileCategory AS Value, CardFileCategory AS Text FROM CardFileCategorySetup ORDER BY CardFileCategory").ToList();

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

            //if ()
            //{
            //    model.Record.Code = "AUTO GENERATED";
            //}
            //else
            //{
            //    txtCode.Text = "";
            //    txtCode.ReadOnly = false;
            //    txtCode.BackColor = System.Drawing.ColorTranslator.FromHtml("#FFFFFF");
            //    ExecuteJS("showModalCurrency();");
            //    //txtCode.Focus();
            //}
            ViewBag.ShowCardTypeModal = true;

            return View("Index", model);
        }

        public IActionResult SelectCardType(string cardType)
        {
            var model = BuildModel(null);
            if (cardType != null)
            {
                model.Common.Mode = "NEW";

                // I-set ang current user at timestamps para sa bagong record.
                model.Record.CreatedBy = HttpContext.Session.GetString("UserName") ?? string.Empty;
                model.Record.CreatedDate = DateTime.Now;
                model.Record.ModifiedBy = HttpContext.Session.GetString("UserName") ?? string.Empty;
                model.Record.ModifiedDate = DateTime.Now;

                model.Record.Code = "AUTO GENERATED";
                model.Common.IsReadOnly = true;

                model.Record.Type = cardType;
                model.Common.IsDisable = true;

                if (model.Record.Code == "AUTO GENERATED")
                {
                    using var myconn = new SqlConnection(_configuration.GetConnectionString("NetCoreConnection"));

                    string xCodeAbbr = myconn.QueryFirstOrDefault<string>("SELECT CardFileTypeAbbr FROM CardFileTypeSetup WHERE CardFileType = @CardFileType", new { CardFileType = cardType }) ?? string.Empty;

                    if (!string.IsNullOrEmpty(xCodeAbbr))
                    {
                        int nextCode = myconn.QueryFirstOrDefault<int>("SELECT ISNULL(MAX(CASE WHEN Code LIKE @Prefix THEN TRY_CONVERT(NUMERIC, REPLACE(Code, @ReplacePrefix, '')) END), 0) + 1 FROM CardFile WHERE Code LIKE @Prefix", new { Prefix = xCodeAbbr + "-%", ReplacePrefix = xCodeAbbr + "-" });

                        model.Record.Code = $"{xCodeAbbr}-{nextCode:D5}";
                    }
                }
            }
            return View("Index", model);
        }


        [HttpPost("save")]
        // I-save ang bagong group o update sa existing group.
        public async Task<IActionResult> Save(ModuleViewModel<CardFileModel> model)
        {
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

                // Save Detail
                // Walang Detail

                // I-reload ang saved data bago magpakita ng success message.
                var viewModel = BuildModel(model.Record.Code);
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
            //var model = Commons.LoadModule<CardFileModel>(
            //    _configuration,
            //    "GroupName",
            //    rowStamp);

            return RedirectToAction(nameof(Index), new { PubPrimaryValue });
        }



        [HttpPost("searchviewlist")]
        // I-filter ang listahan ng groups gamit ang search text mula sa modal/list view.
        public IActionResult SearchViewList(ModuleViewModel<CardFileModel> model)
        {
            var viewModel = Commons.LoadModule<CardFileModel>(
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
        public IActionResult Delete(ModuleViewModel<CardFileModel> model)
        {
            try
            {
                // DeleteModule ang may database transaction para sa main record at related detail records.
                Commons.DeleteModule(
                    _configuration,
                    PubTableName,
                    PubPrimary,
                    model.Record);


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
        public IActionResult SearchModule(ModuleViewModel<CardFileModel> model)
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




    }
}
