namespace netcore_crud.Models
{
    public class SecurityModel
    {
        // Audit Information
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime? CreatedDate { get; set; }

        public string ModifiedBy { get; set; } = string.Empty;
        public DateTime? ModifiedDate { get; set; }


        public string GroupName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool CanChangePW { get; set; } = false;

    }
}
