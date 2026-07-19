using Microsoft.AspNetCore.Html;

namespace netcore_crud.App_Code
{
    public static class Commons
    {
        public static HtmlString ShowMessage(string alertType, string message)
        {
            string icon = alertType.ToLower() switch
            {
                "success" => "fa-solid fa-circle-check",
                "warning" => "fa-solid fa-triangle-exclamation",
                "danger" => "fa-solid fa-circle-xmark",
                "info" => "fa-solid fa-circle-info",
                _ => "fa-solid fa-circle-info"
            };

            string html = $@"
<div class='alert alert-{alertType} alert-dismissible fade show' role='alert'>
    <i class='{icon} me-2'></i>
    {message}
    <button type='button'
            class='btn-close'
            data-bs-dismiss='alert'
            aria-label='Close'>
    </button>
</div>";

            return new HtmlString(html);
        }
    }
}
