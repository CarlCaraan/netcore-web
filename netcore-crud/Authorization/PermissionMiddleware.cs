namespace netcore_crud.Authorization
{
    using Dapper;
    using Microsoft.Data.SqlClient;

    public class PermissionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IConfiguration _configuration;

        public PermissionMiddleware(
            RequestDelegate next,
            IConfiguration configuration)
        {
            _next = next;
            _configuration = configuration;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Kunin ang endpoint na napili ng ASP.NET Core routing.
            var endpoint = context.GetEndpoint();

            // Hanapin kung may PermissionAttribute ang action.
            var permission = endpoint?
                .Metadata
                .GetMetadata<PermissionAttribute>();

            // Walang [Permission] = walang permission check.
            if (permission == null)
            {
                await _next(context);
                return;
            }

            // Kunin ang current logged-in user.
            string? userName = context.Session.GetString("UserName");

            if (string.IsNullOrWhiteSpace(userName))
            {
                //context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                context.Response.Redirect("/");
                return;
            }

            bool allowed = await HasPermission(
                userName,
                permission.Application,
                permission.Action);

            if (!allowed)
            {
                context.Session.SetString(
                    "PermissionMessage",
                    $"You do not have permission to perform '{permission.Action}' on '{permission.Application}'."
                );

                context.Response.Redirect("/dashboard/loginsuccess");
                return;
            }

            await _next(context);
        }

        private async Task<bool> HasPermission(
            string userName,
            string application,
            string action)
        {
            using SqlConnection myconn = new SqlConnection(
                _configuration.GetConnectionString("NetCoreConnection"));

            string column = action.ToUpperInvariant() switch
            {
                "READ" => "Reads",
                "INSERT" => "Inserts",
                "EDIT" => "Edits",
                "DELETE" => "Deletes",
                _ => throw new Exception($"Invalid permission action: {action}")
            };

            string sql = $"SELECT COUNT_BIG(1) FROM UserAuthority WHERE GroupName = (SELECT GroupName FROM LoginNames WHERE Username = @UserName) AND Application = @Application AND {column} = 1";

            long count = await myconn.ExecuteScalarAsync<long>(
                sql,
                new
                {
                    UserName = userName,
                    Application = application
                });

            return count > 0;
        }
    }
}
