using System.Data;

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

        // Common Flags
        public bool IsNew => Mode == "NEW";
        public bool IsEdit => Mode == "EDIT";

        // Common Messages
        public string Message { get; set; } = string.Empty;
        public bool HasError { get; set; }

        // Search / Paging (para sa List/Search modules)
        public string txtSearch { get; set; } = string.Empty;
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public int TotalRecords { get; set; }

        //Data Table
        public int Rowstamp { get; set; } = 0;
        public DataTable ViewList { get; set; } = new DataTable();

        public bool IsReadOnly { get; set; } = true;


        // Detail Tables
        public List<UserAuthorityModel> UserAuthority { get; set; } = new();
        public List<MenuChoiceModel> MenuChoice { get; set; } = new();
    }
}
