namespace netcore_crud.Models.Security
{
    public class UserAuthorityModel
    {
        public bool Reads { get; set; }
        public bool Inserts { get; set; }
        public bool Deletes { get; set; }
        public bool Edits { get; set; }
        public string? GroupName { get; set; }
        public string? Application { get; set; }
        public bool? Active { get; set; }
        public decimal RowStamp { get; set; }
    }
}
