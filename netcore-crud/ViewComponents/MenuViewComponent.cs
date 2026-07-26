using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using netcore_crud.Models;

namespace netcore_crud.ViewComponents
{

    public class MenuViewComponent : ViewComponent
    {
        private readonly IConfiguration _configuration;

        public MenuViewComponent(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IViewComponentResult Invoke(string category)
        {
            using var myconn = new SqlConnection(
                _configuration.GetConnectionString("NetCoreConnection"));

            string sql = @"SELECT Category,Module,Active,REPLACE(Url, '~/', '/') AS Url,CssClass,SortOrder FROM Modules WHERE Active = 1 AND Category = @Category ORDER BY SortOrder";

            var menus = myconn.Query<ModuleModel>(sql, new
            {
                Category = category
            }).ToList();

            return View(menus);
        }
    }
}
