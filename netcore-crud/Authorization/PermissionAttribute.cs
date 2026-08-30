namespace netcore_crud.Authorization
{
    using System;

    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
    public class PermissionAttribute : Attribute
    {
        public string Application { get; }
        public string Action { get; }

        public PermissionAttribute(string application, string action)
        {
            Application = application.ToUpper();
            Action = action;
        }
    }
}
