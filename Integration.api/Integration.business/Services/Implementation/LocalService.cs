using Integration.business.Services.Interfaces;
using Integration.data.Data;
using Integration.data.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using MySql.Data.MySqlClient;
using System.Text;

namespace Integration.business.Services.Implementation
{
    public class LocalService : ILocalService
    {
        private readonly AppDbContext _appDbContext;
        private readonly IDataBaseService _dataBaseService;

        public LocalService(AppDbContext appDbContext,IDataBaseService dataBaseService)
        {
            _appDbContext = appDbContext;
            _dataBaseService = dataBaseService;
        }
        #region HelpersFromSqlToSql
        public async Task<ApiResponse<int>> SyncLocalToPublic(int moduleId)
        {
            var module = await _appDbContext.modules
           .Include(c => c.ToDb)
           .Include(c => c.FromDb)
           .Include(c => c.columnFroms)
           .FirstOrDefaultAsync(c => c.Id == moduleId);
            if (module is null)
                return new ApiResponse<int>(false, "Module Not Found");
            var references = await _appDbContext.References
                  .Where(c => c.ModuleId == moduleId)
                  .ToListAsync();

            var ReferencesIds = await GetReferenceASyncSqlToSql(references, module);



            var columnFrom = module.columnFroms;
            //var queryFrom = $"SELECT {string.Join(',', columnFrom.Select(c => c.Name))} FROM {module.TableFrom?.Name} WHERE {string.Join(" OR ", module.conditionFroms?.Select(c => c.Operation) ?? new List<string>())}";
            //var queryFrom = $"SELECT {string.Join(',', columnFrom.Select(c => c.ColumnFromName))} FROM {module.TableFromName} where {module.FromInsertFlagName}=1 or {module.FromUpdateFlagName}=1";
            var queryFrom = $"SELECT {string.Join(',', columnFrom.Select(c => c.ColumnFromName))} FROM {module.TableFromName}";
            var updateQueries = new StringBuilder();
            var allValues = new List<Dictionary<string, string>>();
            var AllIdsAndLocalIdsOnCloud = await FetchIdsAsync(module.ToPrimaryKeyName, module.LocalIdName, module.TableToName, module);
            var SelectedFrom = module.FromDb;
            if (SelectedFrom is null)
                return new ApiResponse<int>(false, "No FromDataBase Selected");
            using (var connection = _dataBaseService.GetConnection(SelectedFrom))
            {
                await connection.OpenAsync();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = queryFrom;

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var data = "";
                            var id = -1;
                            var rowValues = new Dictionary<string, string>(); // لتخزين القيم في كل صف

                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                var n = reader.GetName(i);
                                var d = columnFrom.FirstOrDefault(c => c.ColumnFromName == n);
                                var Name = reader.GetName(i);

                                // معالجة القيمة بحسب نوعها
                                dynamic value;
                                if (reader.IsDBNull(i))
                                {
                                    value = "NULL";
                                }
                                else
                                {
                                    // إذا كان النوع DateTime، قم بتنسيق القيمة
                                    if (reader.GetFieldType(i) == typeof(DateTime))
                                    {
                                        DateTime dateTimeValue = reader.GetDateTime(i);
                                        value = $"{dateTimeValue:yyyy-MM-dd HH:mm:ss}"; // تنسيق التاريخ
                                    }
                                    else
                                    {
                                        value = reader.GetValue(i).ToString().Trim();

                                    }
                                    if (d.isReference)
                                    {
                                        if (ReferencesIds.ContainsKey(d.TableReferenceName))
                                        {
                                            if (ReferencesIds[d.TableReferenceName].TryGetValue(value, out object newid))
                                            {
                                                var NewValued = newid as dynamic;
                                                if (NewValued != null)
                                                {
                                                    if (NewValued.Id is int idValue)
                                                    {
                                                        value = idValue.ToString();
                                                    }
                                                    else
                                                    {
                                                        value = NewValued.Id.ToString();
                                                    }
                                                }
                                            }

                                        }
                                    }
                                }

                                if (Name == module.ToInsertFlagName)
                                {
                                    value = "0";
                                }  
                                if (Name == module.ToUpdateFlagName)
                                {
                                    value = "0";
                                }


                                rowValues[Name] = value;

                                if (Name == module.fromPrimaryKeyName)
                                {
                                    int key = Convert.ToInt32(reader.GetValue(i).ToString());

                                    if (AllIdsAndLocalIdsOnCloud.TryGetValue(key.ToString(), out var newId))
                                    {
                                        id = Convert.ToInt32(newId);
                                    }
                                    else
                                    {
                                        id = -1;
                                    }
                                }



                                if (d != null && !string.IsNullOrEmpty(d.ColumnToName) && id != -1)
                                {
                                    data += $"{d.ColumnToName} = '{value}',";
                                }
                            }

                            allValues.Add(rowValues);

                            if (id == -1)
                            {
                                var insertValues = string.Join(", ", columnFrom
                                    .Where(c => !string.IsNullOrEmpty(c.ColumnToName))
                                    .Select(c => $"'{rowValues[c.ColumnFromName]}'"));

                                var insertQuery = $"INSERT INTO {module.TableToName} ({string.Join(",", columnFrom.Where(c => !string.IsNullOrEmpty(c.ColumnToName)).Select(c => c.ColumnToName))}) VALUES ({insertValues})";

                                insertQuery = insertQuery.Replace("AM", string.Empty);
                                insertQuery = insertQuery.Replace("PM", string.Empty);
                                updateQueries.Append(insertQuery);
                                updateQueries.Append(";");
                            }

                            // تصحيح موضع الـ else block
                            if (!string.IsNullOrEmpty(data))
                            {
                                var updateQuery = $"UPDATE {module.TableToName} SET {data.TrimEnd(',')} WHERE {module.ToPrimaryKeyName}={id}";
                                updateQueries.Append(updateQuery);
                                updateQueries.Append(";");
                            }
                        }

                    }

                    var updateFlagQuery = $"UPDATE {module.TableFromName} SET {module.FromInsertFlagName}=0,{module.FromUpdateFlagName}=0";
                    command.CommandText = updateFlagQuery;
                    await command.ExecuteNonQueryAsync();
                }
            }

          
            if (updateQueries.Length <=0)
            {
                return new ApiResponse<int>(false, "No Rows To Sync");
            }

         
            var RowsEfected = await UpdateRowsAsyncSqlToSql(updateQueries, module);

            var SyncIdBetweenTables = await FetchIdsAsync(module.ToPrimaryKeyName, module.LocalIdName, module.TableToName, module);

            var str = new StringBuilder();
            foreach (var item in SyncIdBetweenTables)
            {
                if (item.Key!=null &&!string.IsNullOrEmpty(item.Key)&&!string.IsNullOrWhiteSpace(item.Key))
                {
                    str.Append($"Update {module.TableFromName} set {module.CloudIdName}={item.Value} where {module.fromPrimaryKeyName}={item.Key};");
                }
            }
            var UpdateQuery = str.ToString();
            using (var connection = _dataBaseService.GetConnection(SelectedFrom))
            {
                await connection.OpenAsync();
                using (var command = connection.CreateCommand())
                {

                    command.CommandText = UpdateQuery;
                    await command.ExecuteNonQueryAsync();
                }
            }
            return new ApiResponse<int>(true, "Sync Successfully", RowsEfected);
        }
    
        private async Task<int> UpdateRowsAsyncSqlToSql(StringBuilder updateQueries, Module module)
        {
            var DataBaseSelected = module.ToDb;
            if (DataBaseSelected is null)
                return 0;

            var Connectionstr = DataBaseSelected.ConnectionString;
            int rowsAffected = 0;

            // Get the appropriate connection type based on the database
            using (var connection = _dataBaseService.GetConnection(DataBaseSelected))
            {
                await connection.OpenAsync();

                using (var transaction = await connection.BeginTransactionAsync())
                {
                    try
                    {
                        // Create a command and execute it
                        using (var command = connection.CreateCommand())
                        {
                            command.Transaction = transaction;
                            command.CommandText = updateQueries.ToString();
                            rowsAffected = await command.ExecuteNonQueryAsync();
                        }

                        await transaction.CommitAsync();
                    }
                    catch
                    {
                        await transaction.RollbackAsync();
                        throw;
                    }
                }
            }

            return rowsAffected;
        }
        private async Task<Dictionary<dynamic, dynamic>> FetchIdsAsync(string CloudIdName, string CloudLocalIdName, string CloudTable, Module module)
        {
            var SelectedTO = module.ToDb;
            if (SelectedTO is null)
                return null;

            var dataDictionary = new Dictionary<dynamic, object>();

            // الحصول على الاتصال بناءً على نوع قاعدة البيانات
            using (var connection = _dataBaseService.GetConnection(SelectedTO))
            {
                await connection.OpenAsync();

                // إنشاء أمر قاعدة البيانات العام
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = $"SELECT {CloudIdName}, {CloudLocalIdName} FROM {CloudTable}";

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var id = reader[CloudLocalIdName].ToString(); // Assuming local_id is a string
                            var value = reader[CloudIdName].ToString(); // Assuming local_id is a string
                        

                            if(id!=null)
                                dataDictionary[id] = value;

                        }
                    }
                }
            }

            return dataDictionary; // Return the dictionary with local_id as the key
        }
        private async Task<Dictionary<string, Dictionary<string, object>>> GetReferenceASyncSqlToSql(List<TableReference> references, Module module)
        {
            var result = new Dictionary<string, Dictionary<string, object>>();

            foreach (var reference in references)
            {
                result.Add(reference.TableFromName, await FetchRefsAsync(reference.PrimaryName, reference.LocalName, reference.TableFromName, module));
            }
            return result;
        }
        private async Task<Dictionary<string, dynamic>> FetchRefsAsync(string cloudPrimaryName, string cloudLocalIdName, string tableName, Module module)
        {
            var SelectedTO = module.ToDb;
            if (SelectedTO == null)
            {
                Console.WriteLine("Database connection is null.");
                return null;
            }

            var dataDictionary = new Dictionary<string, dynamic>();

            try
            {
                // إنشاء اتصال عام
                using (var connection = _dataBaseService.GetConnection(SelectedTO))
                {
                    await connection.OpenAsync();
                    Console.WriteLine("Connection opened successfully.");

                    // إنشاء أمر عام
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = $"SELECT {cloudPrimaryName} as Id, {cloudLocalIdName} as LocalId, InsertedFlag FROM {tableName};";

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            // قراءة البيانات
                            while (await reader.ReadAsync())
                            {
                                var localId = reader["LocalId"]?.ToString();
                                var primaryId = reader["Id"]?.ToString();

                                if (localId != null)
                                {
                                    var obj = new
                                    {
                                        Id = primaryId,
                                        LocalId = localId
                                    };

                                    dataDictionary[localId] = obj;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
            }

            return dataDictionary; // Return the dictionary with local_id as the key
        }

        #endregion
    } 
}


