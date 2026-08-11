using Dapper;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using netcore_crud.Models.Security;
using netcore_crud.Models.Shared;
using System.Data;
using System.Reflection;

namespace netcore_crud.App_Code
{
    // Shared helper methods para sa messages at generic database operations ng modules.
    public static class Commons
    {
        // Gumagawa ng Bootstrap alert message na puwedeng ipakita sa view.
        public static HtmlString ShowMessage(string alertType, string message)
        {
            // Piliin ang icon depende sa alert type (success, warning, danger, o info).
            string icon = alertType.ToLower() switch
            {
                "success" => "bi bi-check-circle-fill",
                "warning" => "bi bi-exclamation-triangle-fill",
                "danger" => "bi bi-x-circle-fill",
                "info" => "bi bi-info-circle-fill",
                _ => "bi bi-info-circle-fill"
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

            // Ibalik ang formatted HTML para ma-render ng Razor bilang alert.
            return new HtmlString(html);
        }




        // Naglo-load ng listahan at, kung meron, ang selected record ng isang module.
        public static ModuleViewModel<T> LoadModule<T>(
            IConfiguration configuration,
            string TableName,
            string PubPrimary,
            string? PubPrimaryValue,
            string search = "")//Search
            where T : new()
        {
            // Ihanda ang default data para sa view bago kumuha sa database.
            var common = new CommonViewModel();
            var model = new T();

            using SqlConnection myconn = new SqlConnection(configuration.GetConnectionString("NetCoreConnection"));

            // Kunin ang latest 30 records o i-filter gamit ang search input.
            string sql;

            if (string.IsNullOrWhiteSpace(search))
            {
                sql = $"SELECT TOP 30 * FROM {TableName} ORDER BY RowStamp DESC";
            }
            else
            {
                sql = $@"  SELECT TOP 30 * FROM {TableName} WHERE {PubPrimary} LIKE @Search OR Description LIKE @Search ORDER BY RowStamp DESC";
            }

            // SqlDataAdapter ang nagfi-fill ng listahan sa DataTable para sa view.
            DataTable dt = new();

            using SqlDataAdapter da = new SqlDataAdapter();
            da.SelectCommand = new SqlCommand(sql, myconn);

            if (!string.IsNullOrWhiteSpace(search))
            {
                // I-parameterize ang search text para safe ang value sa SQL query.
                da.SelectCommand.Parameters.AddWithValue("@Search", $"%{search}%");
            }

            da.Fill(dt);

            // I-save ang result list sa common view model.
            common.ViewList = dt;

            // Kapag may primary value, kunin din ang buong selected record.
            if (!string.IsNullOrWhiteSpace(PubPrimaryValue))
            {
                string sqlRecord = $"SELECT * FROM {TableName} WHERE {PubPrimary} = @PubPrimary";

                using SqlCommand cmd = new SqlCommand(sqlRecord, myconn);
                cmd.Parameters.AddWithValue("@PubPrimary", PubPrimaryValue);

                myconn.Open();

                using SqlDataReader dr = cmd.ExecuteReader();

                // I-map ang database columns sa kaparehong property names ng model.
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

                    // Ang selected record ay puwede nang i-edit sa view.
                    common.Mode = "EDIT";
                    // Itago ang original identifier para ma-validate ito sa pag-save.
                    common.OriginalPubPrimary = PubPrimaryValue;
                }
            }

            return new ModuleViewModel<T>
            {
                Common = common,
                Record = model
            };
        }

        // Tinitingnan kung may matching column bago mag-set ng model property.
        private static bool HasColumn(SqlDataReader reader, string columnName)
        {
            for (int i = 0; i < reader.FieldCount; i++)
            {
                if (reader.GetName(i).Equals(columnName, StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }




        // Kinukuha ang detail rows na naka-link sa parent record gamit ang foreign key.
        public static List<T> LoadModuleDetail<T>(
            IConfiguration configuration,
            string tableName,
            string foreignKey,
            object foreignKeyValue)
        {
            using SqlConnection myconn = new SqlConnection(
                configuration.GetConnectionString("NetCoreConnection"));

            string sql = $"SELECT * FROM {tableName} WHERE {foreignKey} = @{foreignKey} ORDER BY RowStamp";

            // I-map ng Dapper ang bawat row papunta sa generic detail model.
            return myconn.Query<T>(
                sql,
                new DynamicParameters(new Dictionary<string, object>
                {
                    { foreignKey, foreignKeyValue }
                })
            ).ToList();
        }


        /// <summary> DAPPER method para mag-save ng record sa database gamit ang generic model.
        /// Nagse-save ng record: INSERT para sa NEW at UPDATE para sa EDIT.
        /// </summary>
        public static void SaveModule<T>(
            IConfiguration configuration,
            string tableName,
            string pubPrimary,
            T model,
            string mode,
            string? originalPubPrimary = null)
        {
            // Gumamit ng isang connection at transaction para isang buong process ang save.
            using SqlConnection myconn = new SqlConnection(configuration.GetConnectionString("NetCoreConnection"));
            myconn.Open();

            using SqlTransaction tran = myconn.BeginTransaction();

            try
            {
                // Kunin lahat ng properties ng model para makagawa ng dynamic SQL.
                PropertyInfo[] props = typeof(T).GetProperties();

                if (mode.Equals("NEW", StringComparison.OrdinalIgnoreCase))
                {
                    // Hanapin ang configured primary key sa model.
                    PropertyInfo? primaryProperty = props.FirstOrDefault(x =>
                        x.Name.Equals(pubPrimary, StringComparison.OrdinalIgnoreCase));

                    if (primaryProperty == null)
                        throw new Exception($"The primary column '{pubPrimary}' was not found in the model.");

                    object? primaryValue = primaryProperty.GetValue(model);

                    // Required ang primary value bago mag-save.
                    if (primaryValue == null || string.IsNullOrWhiteSpace(primaryValue.ToString()))
                        throw new Exception($"{pubPrimary} is required.");

                    // I-check muna kung may existing record na may parehong primary value.
                    string checkSql = $@"
                    SELECT COUNT_BIG(1)
                    FROM {tableName} WITH (UPDLOCK, HOLDLOCK)
                    WHERE {pubPrimary} = @PrimaryValue";

                    long duplicate = myconn.ExecuteScalar<long>(
                        checkSql,
                        new { PrimaryValue = primaryValue },
                        tran);

                    if (duplicate > 0)
                        throw new Exception($"{pubPrimary} '{primaryValue}' already exists.");

                    // Dynamic na buuin ang listahan ng columns at parameters.
                    List<string> columns = new();
                    List<string> parameters = new();

                    foreach (PropertyInfo prop in props)
                    {
                        // Skip ang auto-generated RowStamp.
                        if (prop.Name.Equals("RowStamp", StringComparison.OrdinalIgnoreCase))
                            continue;

                        columns.Add(prop.Name);
                        parameters.Add("@" + prop.Name);
                    }

                    // Dynamic INSERT statement.
                    string insertSql = $@"
                    INSERT INTO {tableName}
                    (
                        {string.Join(",", columns)}
                    )
                    VALUES
                    (
                        {string.Join(",", parameters)}
                    )";

                    // Automatic na imi-map ni Dapper ang model properties sa SQL parameters.
                    myconn.Execute(insertSql, model, tran);
                }
                else if (mode.Equals("EDIT", StringComparison.OrdinalIgnoreCase))
                {
                    // Hanapin ang primary key property.
                    PropertyInfo? primaryProperty = props.FirstOrDefault(x =>
                        x.Name.Equals(pubPrimary, StringComparison.OrdinalIgnoreCase));

                    if (primaryProperty == null)
                        throw new Exception($"The primary column '{pubPrimary}' was not found in the model.");

                    object? primaryValue = primaryProperty.GetValue(model);

                    // Hindi pinapayagang baguhin ang primary key.
                    if (!string.Equals(primaryValue?.ToString(), originalPubPrimary, StringComparison.Ordinal))
                        throw new Exception($"{pubPrimary} cannot be changed.");

                    // Dynamic na buuin ang SET clause.
                    List<string> setClause = new();

                    foreach (PropertyInfo prop in props)
                    {
                        // Skip ang RowStamp.
                        if (prop.Name.Equals("RowStamp", StringComparison.OrdinalIgnoreCase))
                            continue;

                        // Skip din ang primary key dahil hindi ito dapat ma-update.
                        if (prop.Name.Equals(pubPrimary, StringComparison.OrdinalIgnoreCase))
                            continue;

                        setClause.Add($"{prop.Name}=@{prop.Name}");
                    }

                    // Dynamic UPDATE statement.
                    string updateSql = $@"
                    UPDATE {tableName}
                    SET {string.Join(",", setClause)}
                    WHERE {pubPrimary} = @{pubPrimary}";

                    // Automatic parameter mapping ni Dapper.
                    myconn.Execute(updateSql, model, tran);
                }
                else
                {
                    throw new Exception("Invalid save mode.");
                }

                // I-commit ang transaction kapag walang error.
                tran.Commit();
            }
            catch
            {
                // I-rollback ang buong transaction kapag may error.
                tran.Rollback();
                throw;
            }
        }

        /// <summary> ADO.NET method para mag-save ng record sa database gamit ang generic model.
        /// Nagse-save ng record: INSERT para sa NEW at UPDATE para sa EDIT.
        /// </summary>
        //public static void SaveModule<T>(
        //    IConfiguration configuration,
        //    string tableName,
        //    string pubPrimary,
        //    T model,
        //    string mode,
        //    string? originalPubPrimary = null)
        //{
        //    // Isang connection at transaction para isang buo ang save process.
        //    using SqlConnection myconn = new SqlConnection(configuration.GetConnectionString("NetCoreConnection"));
        //    myconn.Open();
        //    using SqlTransaction tran = myconn.BeginTransaction();
        //    try
        //    {
        //        // Isang command lang ang gagamitin sa loob ng active transaction.
        //        using SqlCommand cmd = new SqlCommand();
        //        cmd.Connection = myconn;
        //        cmd.Transaction = tran;

        //        // Kunin ang model properties para dynamic na mabuo ang SQL fields.
        //        PropertyInfo[] props = typeof(T).GetProperties();

        //        if (mode.Equals("NEW", StringComparison.OrdinalIgnoreCase))
        //        {
        //            // Siguraduhing ang configured primary field ay nasa model.
        //            PropertyInfo? primaryProperty = props.FirstOrDefault(prop =>
        //                prop.Name.Equals(pubPrimary, StringComparison.OrdinalIgnoreCase));

        //            if (primaryProperty == null)
        //                throw new Exception($"The primary column '{pubPrimary}' was not found in the model.");

        //            object primaryValue = primaryProperty.GetValue(model) ?? DBNull.Value;

        //            // Required ang primary value bago makapag-save ng bagong record.
        //            if (primaryValue == DBNull.Value || string.IsNullOrWhiteSpace(primaryValue.ToString()))
        //                throw new Exception($"{pubPrimary} is required.");

        //            // I-check ang duplicate primary value bago mag-INSERT.
        //            cmd.CommandText = $@"
        //                SELECT COUNT_BIG(1)
        //                FROM {tableName} WITH (UPDLOCK, HOLDLOCK)
        //                WHERE {pubPrimary} = @DuplicatePubPrimary";
        //            cmd.Parameters.AddWithValue("@DuplicatePubPrimary", primaryValue);

        //            if (Convert.ToInt64(cmd.ExecuteScalar()) > 0)
        //                throw new Exception($"{pubPrimary} '{primaryValue}' already exists.");

        //            cmd.Parameters.Clear();

        //            // Buuin ang INSERT columns at parameters mula sa model properties.
        //            List<string> columns = new();
        //            List<string> parameters = new();

        //            foreach (PropertyInfo prop in props)
        //            {
        //                // Hindi sinasama ang auto-generated RowStamp column.
        //                if (prop.Name.Equals("RowStamp", StringComparison.OrdinalIgnoreCase))
        //                    continue;

        //                columns.Add(prop.Name);
        //                parameters.Add("@" + prop.Name);

        //                object value = prop.GetValue(model) ?? DBNull.Value;
        //                cmd.Parameters.AddWithValue("@" + prop.Name, value);
        //            }
        //            cmd.CommandText = $@"
        //        INSERT INTO {tableName}
        //        (
        //            {string.Join(",", columns)}
        //        )
        //        VALUES
        //        (
        //            {string.Join(",", parameters)}
        //        )";
        //        }
        //        else if (mode.Equals("EDIT", StringComparison.OrdinalIgnoreCase))
        //        {
        //            // Kunin ang current primary value mula sa submitted model.
        //            PropertyInfo? primaryProperty = props.FirstOrDefault(prop =>
        //                prop.Name.Equals(pubPrimary, StringComparison.OrdinalIgnoreCase));

        //            if (primaryProperty == null)
        //                throw new Exception($"The primary column '{pubPrimary}' was not found in the model.");

        //            object primaryValue = primaryProperty.GetValue(model) ?? DBNull.Value;

        //            // Ang original value ang target ng edit; bawal itong palitan dahil unique identifier ito.
        //            if (string.IsNullOrWhiteSpace(originalPubPrimary) ||
        //                !string.Equals(Convert.ToString(primaryValue), originalPubPrimary, StringComparison.Ordinal))
        //            {
        //                throw new Exception($"{pubPrimary} cannot be changed.");
        //            }

        //            // Buuin ang fields na puwedeng i-update; excluded ang primary at RowStamp.
        //            List<string> setClause = new();

        //            foreach (PropertyInfo prop in props)
        //            {
        //                object value = prop.GetValue(model) ?? DBNull.Value;

        //                cmd.Parameters.AddWithValue("@" + prop.Name, value);

        //                if (prop.Name.Equals("RowStamp", StringComparison.OrdinalIgnoreCase))
        //                    continue;

        //                if (prop.Name.Equals(pubPrimary, StringComparison.OrdinalIgnoreCase)) // Hindi dapat mabago ang primary column.
        //                    continue;
        //                setClause.Add($"{prop.Name}=@{prop.Name}");
        //            }

        //            cmd.CommandText = $@"
        //        UPDATE {tableName}
        //        SET {string.Join(",", setClause)}
        //        WHERE {pubPrimary}=@{pubPrimary}";
        //        }
        //        else
        //        {
        //            throw new Exception("Invalid save mode.");
        //        }

        //        // I-execute ang INSERT/UPDATE, saka i-commit kapag successful.
        //        cmd.ExecuteNonQuery();
        //        tran.Commit();
        //    }
        //    catch
        //    {
        //        // I-rollback lahat kapag may validation o database error.
        //        tran.Rollback();
        //        throw;
        //    }
        //}

        [HttpPost]
        public static void SaveModuleDetail(
            IConfiguration configuration,
            string tableName,
            string foreignKeyColumn,
            object foreignKeyValue,
            string mode,
            List<UserAuthorityModel>? details = null)
        {
            using SqlConnection myconn = new(configuration.GetConnectionString("NetCoreConnection"));
            myconn.Open();

            using SqlTransaction transaction = myconn.BeginTransaction();

            try
            {
                if (mode.Equals("NEW", StringComparison.OrdinalIgnoreCase))
                {
                    using SqlCommand mycmd = myconn.CreateCommand();
                    mycmd.Transaction = transaction;
                    mycmd.CommandText = $"INSERT INTO [{tableName}] ([Reads], [Inserts], [Edits], [Deletes], [{foreignKeyColumn}], [Application], [Active]) SELECT 1, 1, 1, 1, @ForeignKeyValue, [Application], 1 FROM [{tableName}] WHERE [{foreignKeyColumn}] = 'ADMINISTRATOR'";
                    mycmd.Parameters.AddWithValue("@ForeignKeyValue", foreignKeyValue ?? DBNull.Value);
                    mycmd.ExecuteNonQuery();
                }
                else if (mode.Equals("EDIT", StringComparison.OrdinalIgnoreCase))
                {
                    using SqlCommand mycmd = myconn.CreateCommand();
                    mycmd.Transaction = transaction;
                    mycmd.CommandText = $"DELETE FROM [{tableName}] WHERE [{foreignKeyColumn}] = @ForeignKeyValue";
                    mycmd.Parameters.AddWithValue("@ForeignKeyValue", foreignKeyValue ?? DBNull.Value);
                    mycmd.ExecuteNonQuery();

                    if (details != null)
                    {
                        foreach (UserAuthorityModel item in details)
                        {
                            using SqlCommand insertCmd = myconn.CreateCommand();
                            insertCmd.Transaction = transaction;
                            insertCmd.CommandText = $"INSERT INTO [{tableName}] ([Reads], [Inserts], [Edits], [Deletes], [{foreignKeyColumn}], [Application], [Active]) VALUES (@Reads, @Inserts, @Edits, @Deletes, @ForeignKeyValue, @Application, @Active)";
                            insertCmd.Parameters.AddWithValue("@Reads", item.Reads);
                            insertCmd.Parameters.AddWithValue("@Inserts", item.Inserts);
                            insertCmd.Parameters.AddWithValue("@Edits", item.Edits);
                            insertCmd.Parameters.AddWithValue("@Deletes", item.Deletes);
                            insertCmd.Parameters.AddWithValue("@ForeignKeyValue", foreignKeyValue ?? DBNull.Value);
                            insertCmd.Parameters.AddWithValue("@Application", item.Application ?? "");
                            insertCmd.Parameters.AddWithValue("@Active", item.Active);
                            insertCmd.ExecuteNonQuery();
                        }
                    }
                }

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }





    }
}
