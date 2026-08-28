namespace netcore_crud.Models.Dashboard
{
    public class DashboardModel
    {
        public List<DashboardUserModel> Users { get; set; } = new();
        public List<DashboardCardFileModel> CardFiles { get; set; } = new();
        public string? Message { get; set; }
    }
}
