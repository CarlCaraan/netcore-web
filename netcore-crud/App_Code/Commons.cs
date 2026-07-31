using Dapper;
using Microsoft.AspNetCore.Html;
using Microsoft.Data.SqlClient;
using netcore_crud.Models;
using System.Data;
using System.Reflection;

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




        // ========= LOAD MODULE MAIN TABLE =========
        public static (CommonViewModel Common, T Model) LoadModule<T>(
            IConfiguration configuration,
            string TableName,
            string PubPrimary,
            long? rowStamp = null,
            string search = "")
            where T : new()
        {
            var common = new CommonViewModel();
            var model = new T();

            using SqlConnection myconn = new SqlConnection(configuration.GetConnectionString("NetCoreConnection"));

            // Load View List
            string sql;

            if (string.IsNullOrWhiteSpace(search))
            {
                sql = $"SELECT TOP 30 * FROM {TableName} ORDER BY RowStamp DESC";
            }
            else
            {
                sql = $@"  SELECT TOP 30 * FROM {TableName} WHERE {PubPrimary} LIKE @Search OR Description LIKE @Search ORDER BY RowStamp DESC";
            }

            DataTable dt = new();

            using SqlDataAdapter da = new SqlDataAdapter();
            da.SelectCommand = new SqlCommand(sql, myconn);

            if (!string.IsNullOrWhiteSpace(search))
            {
                da.SelectCommand.Parameters.AddWithValue("@Search", $"%{search}%");
            }

            da.Fill(dt);

            common.ViewList = dt;

            // Load Selected Record
            if (rowStamp.HasValue)
            {
                string sqlRecord = $"SELECT * FROM {TableName} WHERE RowStamp = @RowStamp";

                using SqlCommand cmd = new SqlCommand(sqlRecord, myconn);
                cmd.Parameters.AddWithValue("@RowStamp", rowStamp.Value);

                myconn.Open();

                using SqlDataReader dr = cmd.ExecuteReader();

                if (dr.Read())
                {
                    foreach (PropertyInfo prop in typeof(T).GetProperties())
                    {
                        if (!HasColumn(dr, prop.Name) || dr[prop.Name] == DBNull.Value)
                            continue;

                        var value = Convert.ChangeType(
                            dr[prop.Name],
                            Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);

                        prop.SetValue(model, value);
                    }

                    common.Mode = "EDIT";
                }
            }

            return (common, model);
        }

        private static bool HasColumn(SqlDataReader reader, string columnName)
        {
            for (int i = 0; i < reader.FieldCount; i++)
            {
                if (reader.GetName(i).Equals(columnName, StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }





        public static List<T> LoadModuleDetail<T>(
        IConfiguration configuration,
        string tableName,
        string foreignKey,
        object foreignKeyValue)
        {
            using SqlConnection myconn = new SqlConnection(configuration.GetConnectionString("NetCoreConnection"));

            string sql = $"SELECT * FROM {tableName} WHERE {foreignKey} = @{foreignKey} ORDER BY RowStamp";

            return myconn.Query<T>(sql, new DynamicParameters(new Dictionary<string, object>
        {
        { foreignKey, foreignKeyValue }
        })).ToList();
        }





    }
}
