namespace netcore_crud.Models
{
    public class ModuleModel
    {
        public int ID { get; set; }

        public string Module { get; set; } = "";

        public string Url { get; set; } = "";

        public string CssClass { get; set; } = "";

        public string Category { get; set; } = "";

        public bool Active { get; set; }

        public int SortOrder { get; set; }
    }
}
