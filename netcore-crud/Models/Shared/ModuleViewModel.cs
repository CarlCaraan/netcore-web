namespace netcore_crud.Models.Shared
{
    // Reusable form model for modules that have common page state and one main record.
    public class ModuleViewModel<T> where T : new()
    {
        public CommonViewModel Common { get; set; } = new();
        public T Record { get; set; } = new();
    }
}
