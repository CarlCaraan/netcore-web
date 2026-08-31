using DevExpress.XtraReports.UI;
using netcore_crud.Models.Report;
namespace netcore_crud.Reports;

public partial class CardFileReport : DevExpress.XtraReports.UI.XtraReport {
    public CardFileReport()
    {
        InitializeComponent();
    }

    public CardFileReport(IEnumerable<CardMasterFileReportModel> data)
    {
        InitializeComponent();
        DataSource = data;
    }
}