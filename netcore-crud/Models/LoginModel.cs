namespace netcore_crud.Models
{
    public class LoginModel
    {
        public string GroupName { get; set; } = string.Empty;

        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public bool Hold { get; set; }
    }
}
