namespace netcore_crud.Models.Report
{
    public class CardMasterFileReportModel
    {
        public string Source { get; set; } = string.Empty;
        public string? EmpNoCode { get; set; }
        public string? CardType { get; set; }
        public string? GivenName { get; set; }
        public string? ContactPerson { get; set; }
        public string ContactInfo { get; set; } = string.Empty;
        public string? Address { get; set; }
        public string? TINNumber { get; set; }
        public decimal? ARTerms { get; set; }
        public decimal? APTerms { get; set; }
        public string? Remarks { get; set; }
        public decimal Linenumber { get; set; }
        public string ProjectAssigned { get; set; } = string.Empty;
        public string UnitPurchased { get; set; } = string.Empty;
        public string? ZipCode { get; set; }
        public string? BRANCH_CODE { get; set; }
        public string? RDO_CODE { get; set; }
        public double? Discount { get; set; }
        public string? Address2 { get; set; }
        public string? BusinessStyle { get; set; }
        public bool? POTransaction { get; set; }
        public bool? PettyCash { get; set; }
        public string? GivenName2307 { get; set; }
    }
}
