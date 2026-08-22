using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using netcore_crud.App_Code;
using netcore_crud.Models.Security;
using netcore_crud.Models.Shared;
using netcore_crud.Validators.Security;

namespace netcore_crud.Controllers
{
    public class ManageUsersController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly ManageUsersValidator _validator;

        public ManageUsersController(IConfiguration configuration, ManageUsersValidator validator)
        {
            _configuration = configuration;
            _validator = validator;
        }

        private ModuleViewModel<LoginNamesModel> BuildModel(string? groupName)
        {

        var model = Commons.LoadModule<LoginNamesModel>(
                    _configuration,
                    "LoginNames",
                    "GroupName",
                    null); // NULL para walang maselect na record na imamap sa fields

            if (!string.IsNullOrWhiteSpace(groupName))
            {
                model.Common.LoginNames = Commons.LoadModuleDetail<LoginNamesModel>(
                    _configuration,
                    "LoginNames",
                    "GroupName",
                    groupName);
            }

            return model;
        }

        [HttpGet("sys-security/ManageUsers/{groupName}")]
        public IActionResult Index(string groupName)
        {
            HttpContext.Session.SetString("ManageUsersGroupName", groupName);

            return View(BuildModel(groupName));
        }


        [HttpPost("sys-security/ManageUsers/{groupName}/save")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save(string groupName, ModuleViewModel<LoginNamesModel> model)
        {
            var validationResult = await _validator.ValidateAsync(model.Record);

            if (!validationResult.IsValid)
            {
                foreach (var error in validationResult.Errors)
                {
                    ModelState.AddModelError($"Record.{error.PropertyName}", error.ErrorMessage);
                }

                var viewModel = BuildModel(groupName);
                viewModel.Record = model.Record;

                return View("Index", viewModel);
            }

            using SqlConnection myconn = new SqlConnection(_configuration.GetConnectionString("NetCoreConnection"));
            await myconn.OpenAsync();

            string checkQuery = "SELECT COUNT(1) FROM LoginNames WHERE Username = @Username";

            bool isRecordExist = await myconn.ExecuteScalarAsync<int>(checkQuery, new { Username = model.Record.Username }) > 0;

            if (isRecordExist)
            {
                ModelState.AddModelError("Record.Username", "Username already exists!");

                var viewModel = BuildModel(groupName);
                viewModel.Record = model.Record;

                return View("Index", viewModel);
            }

            string query = "INSERT INTO LoginNames (Username, Password, GroupName, Hold, Email, CreatedBy, CreatedDate) VALUES (@Username, ENCRYPTBYPASSPHRASE('PHILSOFT', @Password), @GroupName, 0, @Email, @CreatedBy, GETDATE())";

            await myconn.ExecuteAsync(query, new
            {
                Username = model.Record.Username,
                Password = model.Record.Password,
                GroupName = groupName,
                Email = model.Record.Email,
                CreatedBy = HttpContext.Session.GetString("UserName")
            });

            var savedModel = BuildModel(groupName);
            savedModel.Common.Message = Commons.ShowMessage("success", "Record saved successfully.");

            return View("Index", savedModel);
        }

        [HttpGet("sys-security/ManageUsers/{groupName}/save")]
        public IActionResult Save(string groupName)
        {
            return RedirectToAction("Index", "ManageUsers", new { groupName });
        }


        [HttpPost("sys-security/ManageUsers/{groupName}/delete")]
        public IActionResult Delete(string username, string groupName)
        {
            try
            {
                using SqlConnection myconn = new SqlConnection(_configuration.GetConnectionString("NetCoreConnection"));

                string query = "DELETE FROM LoginNames WHERE Username = @Username AND GroupName <> 'Administrator'";

                myconn.Execute(query, new { Username = username });

                var savedModel = BuildModel(groupName);
                savedModel.Common.Message = Commons.ShowMessage("success", "User deleted successfully.");

                return View("Index", savedModel);
            }
            catch (Exception ex)
            {
                var model = BuildModel(groupName);
                model.Common.Message = Commons.ShowMessage("danger", ex.Message);

                return View("Index", model);
            }
        }

        [HttpGet("sys-security/ManageUsers/{groupName}/delete")]
        public IActionResult Delete(string groupName)
        {
            return RedirectToAction("Index", "ManageUsers", new { groupName });
        }





    }
}
