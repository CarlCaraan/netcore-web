namespace netcore_crud.Models.Security
{
    public class LoginNamesModel
    {
        public string? Username { get; set; }
        public string? Groupname { get; set; }
        public string? Createdby { get; set; }
        public DateTime? Createddate { get; set; }
        public bool Hold { get; set; }
        public decimal RowStamp { get; set; }
        public string? Password { get; set; }
        public string? Email { get; set; }
    }
}
