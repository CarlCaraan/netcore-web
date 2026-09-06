using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using netcore_crud.Models.Report;
using netcore_crud.Reports;

namespace netcore_crud.Controllers
{
    public class ReportController : BaseController
    {
        private readonly IConfiguration _configuration;

        public ReportController(IConfiguration configuration)
        {
            _configuration = configuration;
        }


        public IActionResult CardFileRpt(string? cardType)
        {
            if (string.IsNullOrWhiteSpace(cardType))
                cardType ="";

            using var myconn = new SqlConnection(_configuration.GetConnectionString("NetCoreConnection"));

            var data = myconn.Query<CardMasterFileReportModel>("SELECT * FROM CardMasterFile WHERE CardType LIKE '%' + @CardType + '%'", new { CardType = cardType }).ToList();

            var report = new CardFileReport(data);

            cardType = string.IsNullOrWhiteSpace(cardType) ? "ALL" : cardType;
            report.Parameters["CardType"].Value = cardType;
            report.RequestParameters = false;

            return View("ReportViewer", report);
        }



        ////MORE REPORTS HERE
        //public IActionResult TestRpt(string? cardType)
        //{
        //}
        


    }
}
