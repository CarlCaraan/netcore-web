using System.ComponentModel.DataAnnotations;

namespace netcore_crud.Models.CardFile
{
    public class CardFileModel
    {
        public string? Code { get; set; }
        public string? Type { get; set; }
        public string? Category { get; set; }
        public string? Name { get; set; }
        public string? Designation { get; set; }
        public string? Department { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? Province { get; set; }
        public string? ZipCode { get; set; }
        public string? Country { get; set; }
        public string? TINNumber { get; set; }
        public string? ContactPerson { get; set; }
        public string? Phone1a { get; set; }
        public string? Phone1b { get; set; }
        public string? Phone1c { get; set; }
        public string? Phone2a { get; set; }
        public string? Phone2b { get; set; }
        public string? Phone2c { get; set; }
        public string? Mobile1a { get; set; }
        public string? Mobile1b { get; set; }
        public string? Mobile2a { get; set; }
        public string? Mobile2b { get; set; }
        public string? Fax { get; set; }
        public string? Email { get; set; }
        public string? Remarks { get; set; }
        public decimal? ARTerms { get; set; }
        public decimal? APTerms { get; set; }
        public string? PhotoFile0 { get; set; }
        public string? LinkAccount { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public decimal RowStamp { get; set; }
        public bool? IsVatable { get; set; }
        public string? BRANCH_CODE { get; set; }
        public string? RDO_CODE { get; set; }
        public string? EntityType { get; set; }
        public string? ATC { get; set; }
        public string? Nature { get; set; }
        public double? CreditableTax { get; set; }
        public bool Status { get; set; }
        public double? CreditLimit { get; set; }
        public string? APAccountNumber { get; set; }
        public string? APSLNumber { get; set; }
        public string? Division { get; set; }
        public string? EdCon { get; set; }
        public string? Currency { get; set; }
        public string? FactorRateCode { get; set; }
        public string? APSLTitle { get; set; }
        public string? APAccountTitle { get; set; }
        public double? Discount { get; set; }
        public bool? PettyCash { get; set; }
        public bool? POTransaction { get; set; }
        public string? Address2 { get; set; }
        public string? BusinessStyle { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? MiddleName { get; set; }
        public string? Suffix { get; set; }
        public string? Name2307 { get; set; }
    }
}