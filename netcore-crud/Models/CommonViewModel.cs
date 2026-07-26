namespace netcore_crud.Models
{
    public class CommonViewModel
    {
        // Record State
        public string Mode { get; set; } = "VIEW";
        public string Status
        {
            get
            {
                return Mode switch
                {
                    "NEW" => "NEW RECORD",
                    "EDIT" => "CURRENTLY EDITING RECORD",
                    _ => "READY"
                };
            }
        }

        // Audit Information
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime? CreatedDate { get; set; }

        public string ModifiedBy { get; set; } = string.Empty;
        public DateTime? ModifiedDate { get; set; }

        // Common Flags
        public bool IsNew => Mode == "NEW";
        public bool IsEdit => Mode == "EDIT";

        // Common Messages
        public string Message { get; set; } = string.Empty;
        public bool HasError { get; set; }

        // Search / Paging (para sa List/Search modules)
        public string SearchText { get; set; } = string.Empty;
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public int TotalRecords { get; set; }
    }
}
