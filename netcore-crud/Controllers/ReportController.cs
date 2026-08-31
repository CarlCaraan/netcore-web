using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using netcore_crud.Models.Report;
using netcore_crud.Reports;

namespace netcore_crud.Controllers
{
    public class ReportController : Controller
    {
        private readonly IConfiguration _configuration;

        public ReportController(IConfiguration configuration)
        {
            _configuration = configuration;
        }


        public IActionResult CardFileRpt(string? cardType)
        {
            if (string.IsNullOrWhiteSpace(cardType))
                return RedirectToAction("Index", "CardFile");

            using var myconn = new SqlConnection(_configuration.GetConnectionString("NetCoreConnection"));

            var data = myconn.Query<CardMasterFileReportModel>("SELECT * FROM CardMasterFile WHERE CardType LIKE '%' + @CardType + '%'", new { CardType = cardType }).ToList();

            var report = new CardFileReport(data);

            return View("ReportViewer", report);
        }
    }
}
