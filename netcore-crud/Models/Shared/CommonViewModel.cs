using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using netcore_crud.Models.CardFile;
using netcore_crud.Models.Security;
using System.Data;

namespace netcore_crud.Models.Shared
{
    public class CommonViewModel
    {
        // Record State
        public string Mode { get; set; } = "";
        // Original unique identifier; ginagamit para malaman kung may binagong PubPrimary sa edit.
        public string OriginalPubPrimary { get; set; } = string.Empty;
        public string Status => Mode switch
        {
            "NEW" => "NEW RECORD",
            "EDIT" => "CURRENTLY EDITING RECORD",
            _ => "READY"
        };

        // Common Flags
        public bool IsNew => Mode == "NEW";
        public bool IsEdit => Mode == "EDIT";
        // Computed property ito para automatic magbago kapag nagbago ang Mode.
        public bool IsAuditReadOnly => IsNew || IsEdit;

        // Common Messages
        public HtmlString Message { get; set; }
        public bool HasError { get; set; }

        // Search / Paging
        public string txtSearch { get; set; } = string.Empty;
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public int TotalRecords { get; set; }

        public int Rowstamp { get; set; }
        public DataTable ViewList { get; set; } = new();
        // Default na editable ang fields kapag unang binuksan ang module.
        public bool IsReadOnly { get; set; } = false;
        public bool IsDisable { get; set; } = true;

        // SECURITY MODUULE DETAIL TABLES
        public List<UserAuthorityModel> UserAuthority { get; set; } = new();
        public List<MenuChoiceModel> MenuChoice { get; set; } = new();
        public List<LoginNamesModel> LoginNames { get; set; } = new();

        // CARD FILE DROPDOWNS
        public List<SelectListItem> CardFileType { get; set; } = new();
        public List<SelectListItem> CardFileCategory { get; set; } = new();
    }
}
