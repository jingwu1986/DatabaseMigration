using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;

namespace DatabaseMigration.Core
{
    public abstract class DbInterpreter
    {
        #region Field & Property
        public virtual string UnicodeInsertChar { get; } = "N";
        public const string RowNumberColumnName = "ROWNUMBER";
        public abstract string CommandParameterChar { get; }
        public abstract char QuotationLeftChar { get; }
        public abstract char QuotationRightChar { get; }
        public abstract DatabaseType DatabaseType { get; }
        public abstract bool SupportBulkCopy { get; }
        public GenerateScriptOption Option { get; set; } = new GenerateScriptOption();
        public ConnectionInfo ConnectionInfo { get; set; }

        public delegate void DataReadHandler(Table table, List<TableColumn> columns, List<Dictionary<string, object>> data, DataTable dataTable);
        public event DataReadHandler OnDataRead;
        private IObserver<FeedbackInfo> m_Observer;
        #endregion

        #region Constructor     

        public DbInterpreter(ConnectionInfo connectionInfo, GenerateScriptOption option)
        {
            this.ConnectionInfo = connectionInfo;
            this.Option = option;
        }
        #endregion

        #region Common Method

        public abstract int BulkCopy(
            DbConnection connection,
            DataTable dataTable,
            string destinationTableName = null,
            int? bulkCopyTimeout = null,
            int? batchSize = null);

        public abstract Task<int> BulkCopyAsync(
            DbConnection connection,
            DataTable dataTable,
            string destinationTableName = null,
            int? bulkCopyTimeout = null,
            int? batchSize = null);

        public string GetObjectDisplayName(DatabaseObject obj, bool useQuotedString = false)
        {
            if (this.GetType().Name == nameof(SqlServerInterpreter))
            {
                return $"{GetString(obj.Owner, useQuotedString)}.{GetString(obj.Name, useQuotedString)}";
            }
            return $"{GetString(obj.Name, useQuotedString)}";
        }

        private string GetString(string str, bool useQuotedString = false)
        {
            return useQuotedString ? GetQuotedString(str) : str;
        }

        public abstract DbConnector GetDbConnector();
        protected string GetQuotedObjectName(DatabaseObject obj)
        {
            return this.GetObjectDisplayName(obj, true);
        }
        protected string GetQuotedColumnNames(IEnumerable<TableColumn> columns)
        {
            return string.Join(",", columns.OrderBy(item => item.Order).Select(item => GetQuotedString(item.ColumnName)));
        }

        protected string GetQuotedString(string str)
        {
            return $"{QuotationLeftChar}{str}{QuotationRightChar}";
        }

        protected DbDataReader GetDataReader(DbConnection dbConnection, string sql)
        {
            DbCommander dbCommander = new DbCommander(dbConnection, System.Data.CommandType.Text, sql);
            DbDataReader dataReader = dbCommander.ExecteReader();
            return dataReader;
        }
        protected object GetScalar(DbConnection dbConnection, string sql)
        {
            DbCommander dbCommander = new DbCommander(dbConnection, System.Data.CommandType.Text, sql);
            object obj = dbCommander.ExecuteScalar();
            return obj;
        }

        protected DataTable GetDataTable(DbConnection dbConnection, string sql)
        {
            var reader = dbConnection.ExecuteReader(sql);
            var dt = new DataTable();
            dt.Load(reader);
            return dt;
        }
        protected async Task<DataTable> GetDataTableAsync(DbConnection dbConnection, string sql)
        {
            var reader = await dbConnection.ExecuteReaderAsync(sql);
            var dt = new DataTable();
            dt.Load(reader);
            return dt;
        }

        public int ExecuteNonQuery(string sql, Dictionary<string, object> paramaters = null)
        {
            return this.ExecuteNonQuery(this.GetDbConnector().CreateConnection(), sql, paramaters);
        }

        public Task<int> ExecuteNonQueryAsync(string sql, Dictionary<string, object> paramaters = null)
        {
            return this.InteralExecuteNonQuery(this.GetDbConnector().CreateConnection(), sql, paramaters, true);
        }

        public int ExecuteNonQuery(DbConnection dbConnection, string sql, Dictionary<string, object> paramaters = null, bool disposeConnection = true)
        {
            return this.InteralExecuteNonQuery(dbConnection, sql, paramaters, disposeConnection, false).Result;
        }

        public Task<int> ExecuteNonQueryAsync(DbConnection dbConnection, string sql, Dictionary<string, object> paramaters = null, bool disposeConnection = true)
        {
            return this.InteralExecuteNonQuery(dbConnection, sql, paramaters, disposeConnection, true);
        }

        public async Task<int> InteralExecuteNonQuery(DbConnection dbConnection, string sql, Dictionary<string, object> paramaters = null, bool disposeConnection = true, bool async = false)
        {
#if disposeConnection
            using (dbConnection)
#endif
            {
                DbCommander dbCommander = new DbCommander(dbConnection, System.Data.CommandType.Text, sql);

                if (paramaters != null)
                {
                    var cmdParams = this.BuildCommandParameters(paramaters);
                    if (cmdParams != null)
                    {
                        foreach(var parameter in cmdParams)
                        {
                            dbCommander.DbCommand.Parameters.Add(parameter);
                        }                       
                    }
                }

                int result = async ? await dbCommander.ExecuteNonQueryAsync() : dbCommander.ExecuteNonQuery();

                return result;
            }
        }

        protected abstract IEnumerable<DbParameter> BuildCommandParameters(Dictionary<string, object> paramaters);
        #endregion

        #region Observer
        public IDisposable Subscribe(IObserver<FeedbackInfo> observer)
        {
            this.m_Observer = observer;
            return null;
        }

        public void Feedback(FeedbackInfoType infoType, string message)
        {
            if (this.m_Observer != null)
            {
                this.m_Observer.OnNext(new FeedbackInfo() { Owner = this.GetType().Name, InfoType = infoType, Message = message });
            }
        }

        public void FeedbackInfo(string message)
        {
            this.Feedback(FeedbackInfoType.Info, message);
        }
        public void FeedbackError(string message)
        {
            this.Feedback(FeedbackInfoType.Error, message);
        }

        public void FeedbackInfo(OperationState state, string objectType, string objectName)
        {
            string message = $"{state.ToString()} to generate script for {objectType} \"{objectName}\".";
            this.Feedback(FeedbackInfoType.Info, message);
        }
        #endregion

        #region Database
        public abstract List<Database> GetDatabases();
        public List<Database> GetDatabases(DbConnector dbConnector, string sql)
        {
            List<Database> databases = new List<Database>();
            using (DbConnection dbConnection = dbConnector.CreateConnection())
            {
                DbDataReader dataReader = this.GetDataReader(dbConnection, sql);
                if (dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        Database database = new Database()
                        {
                            Name = dataReader["Name"]?.ToString(),
                        };

                        databases.Add(database);
                    }
                }
            }
            return databases;
        }
        #endregion

        #region User Defined Type
        public abstract List<UserDefinedType> GetUserDefinedTypes(params string[] typeNames);
        public abstract Task<List<UserDefinedType>> GetUserDefinedTypesAsync(params string[] typeNames);
        protected List<UserDefinedType> GetUserDefinedTypes(DbConnector dbConnector, string sql)
        {
            return InternalGetUserDefinedTypes(dbConnector, sql, false).Result;
        }

        protected Task<List<UserDefinedType>> GetUserDefinedTypesAsync(DbConnector dbConnector, string sql)
        {
            return InternalGetUserDefinedTypes(dbConnector, sql, true);
        }

        private async Task<List<UserDefinedType>> InternalGetUserDefinedTypes(DbConnector dbConnector, string sql, bool async = false)
        {
            List<UserDefinedType> userDefinedTypes;
            using (DbConnection dbConnection = dbConnector.CreateConnection())
            {
                userDefinedTypes = (async ? (await dbConnection.QueryAsync<UserDefinedType>(sql)) : dbConnection.Query<UserDefinedType>(sql)).ToList();
            }

            this.FeedbackInfo($"Get {userDefinedTypes.Count} user defined types.");

            return userDefinedTypes;
        }
        #endregion

        #region Table
        public abstract List<Table> GetTables(params string[] tableNames);
        public abstract Task<List<Table>> GetTablesAsync(params string[] tableNames);
        protected List<Table> GetTables(DbConnector dbConnector, string sql)
        {
            return this.InteralGetTables(dbConnector, sql, false).Result;
        }

        protected Task<List<Table>> GetTablesAsync(DbConnector dbConnector, string sql)
        {
            return this.InteralGetTables(dbConnector, sql, true);
        }

        private async Task<List<Table>> InteralGetTables(DbConnector dbConnector, string sql, bool async = false)
        {
            List<Table> tables = new List<Table>();
            using (DbConnection dbConnection = dbConnector.CreateConnection())
            {
                var list = async ? (await dbConnection.QueryAsync<Table>(sql)) : dbConnection.Query<Table>(sql);
                if (list != null && list.Any())
                {
                    int i = 1;
                    foreach (var table in list)
                    {
                        table.Order = i++;
                        tables.Add(table);
                    }
                }
            }

            this.FeedbackInfo($"Get {tables.Count} tables.");

            return tables;
        }
        #endregion      

        #region Table Column
        public abstract List<TableColumn> GetTableColumns(params string[] tableNames);

        protected List<TableColumn> GetTableColumns(DbConnector dbConnector, string sql)
        {
            List<TableColumn> columns;
            using (DbConnection dbConnection = dbConnector.CreateConnection())
            {
                columns = dbConnection.Query<TableColumn>(sql).ToList();
            }
            return columns;
        }
        #endregion

        #region Table Primary Key
        public abstract List<TablePrimaryKey> GetTablePrimaryKeys(params string[] tableNames);

        protected List<TablePrimaryKey> GetTablePrimaryKeys(DbConnector dbConnector, string sql)
        {
            List<TablePrimaryKey> tablePrimaryKeys;
            using (DbConnection dbConnection = dbConnector.CreateConnection())
            {
                tablePrimaryKeys = dbConnection.Query<TablePrimaryKey>(sql).ToList();
            }

            return tablePrimaryKeys;
        }
        #endregion

        #region Table Foreign Key
        public abstract List<TableForeignKey> GetTableForeignKeys(params string[] tableNames);

        protected List<TableForeignKey> GetTableForeignKeys(DbConnector dbConnector, string sql)
        {
            List<TableForeignKey> tableForeignKeys;
            using (DbConnection dbConnection = dbConnector.CreateConnection())
            {
                tableForeignKeys = dbConnection.Query<TableForeignKey>(sql).ToList();
            }

            return tableForeignKeys;
        }
        #endregion

        #region Table Index
        public abstract List<TableIndex> GetTableIndexes(params string[] tableNames);

        protected List<TableIndex> GetTableIndexes(DbConnector dbConnector, string sql)
        {
            List<TableIndex> tableIndexes;
            using (DbConnection dbConnection = dbConnector.CreateConnection())
            {
                tableIndexes = dbConnection.Query<TableIndex>(sql).ToList();
            }

            return tableIndexes;
        }
        #endregion

        #region Identity
        public virtual void SetIdentityEnabled(DbConnection dbConnection, TableColumn column, bool enabled) { }
        #endregion

        #region View
        public abstract List<View> GetViews(params string[] viewNames);
        public abstract Task<List<View>> GetViewsAsync(params string[] viewNames);
        protected List<View> GetViews(DbConnector dbConnector, string sql)
        {
            return this.InteralGetViews(dbConnector, sql, false).Result;
        }

        protected Task<List<View>> GetViewsAsync(DbConnector dbConnector, string sql)
        {
            return this.InteralGetViews(dbConnector, sql, true);
        }

        private async Task<List<View>> InteralGetViews(DbConnector dbConnector, string sql, bool async = false)
        {
            List<View> views = new List<View>();
            using (DbConnection dbConnection = dbConnector.CreateConnection())
            {
                var list = async ? (await dbConnection.QueryAsync<View>(sql)) : dbConnection.Query<View>(sql);
                if (list != null && list.Any())
                {
                    int i = 1;
                    foreach (var view in list)
                    {
                        view.Order = i++;
                        views.Add(view);
                    }
                }
            }

            this.FeedbackInfo($"Get {views.Count} views.");

            return views;
        }
        #endregion      

        #region Generate Scripts

        public virtual SchemaInfo GetSchemaInfo(SelectionInfo selectionInfo, bool getAllIfNotSpecified = true)
        {
            return this.InternalGetSchemalInfo(selectionInfo, getAllIfNotSpecified, false).Result;
        }

        public virtual Task<SchemaInfo> GetSchemaInfoAsync(SelectionInfo selectionInfo, bool getAllIfNotSpecified = true)
        {
            return this.InternalGetSchemalInfo(selectionInfo, getAllIfNotSpecified, true);
        }

        private async Task<SchemaInfo> InternalGetSchemalInfo(SelectionInfo selectionInfo, bool getAllIfNotSpecified = true, bool async = false)
        {
            List<UserDefinedType> userDefinedTypes = new List<UserDefinedType>();

            string[] userDefinedTypeNames = selectionInfo.UserDefinedTypeNames;
            string[] tableNames = selectionInfo.TableNames;
            string[] viewNames = selectionInfo.ViewNames;

            List<Table> tables = new List<Table>();
            List<TableColumn> columns = new List<TableColumn>();
            List<View> views = new List<View>();

            if ((userDefinedTypeNames != null && userDefinedTypeNames.Length > 0) || getAllIfNotSpecified)
            {
                userDefinedTypes = async ? await this.GetUserDefinedTypesAsync(userDefinedTypeNames) : this.GetUserDefinedTypes(userDefinedTypeNames);
            }

            List<TablePrimaryKey> tablePrimaryKeys = new List<TablePrimaryKey>();
            List<TableForeignKey> tableForeignKeys = new List<TableForeignKey>();
            List<TableIndex> tableIndices = new List<TableIndex>();

            if ((tableNames != null && tableNames.Length > 0) || getAllIfNotSpecified)
            {
                tables = async ? await this.GetTablesAsync(tableNames) : this.GetTables(tableNames);
                columns = this.GetTableColumns(tableNames);

                if (Option.GenerateKey)
                {
                    tablePrimaryKeys = this.GetTablePrimaryKeys(tableNames);
                    tableForeignKeys = this.GetTableForeignKeys(tableNames);
                }

                if (Option.GenerateIndex)
                {
                    tableIndices = this.GetTableIndexes(tableNames);
                }
            }

            if (Option.SortTablesByKeyReference && Option.GenerateKey)
            {
                List<string> sortedTableNames = TableReferenceHelper.ResortTableNames(tableNames, tableForeignKeys);

                int i = 1;
                foreach (string tableName in sortedTableNames)
                {
                    Table table = tables.FirstOrDefault(item => item.Name == tableName);
                    if (table != null)
                    {
                        table.Order = i++;
                    }
                }

                tables = tables.OrderBy(item => item.Order).ToList();
            }

            if ((viewNames != null && viewNames.Length > 0) || getAllIfNotSpecified)
            {
                views = async ? await this.GetViewsAsync(viewNames) : this.GetViews(viewNames);
                views = ViewHelper.ResortViews(views);
            }

            return new SchemaInfo
            {
                UserDefinedTypes = userDefinedTypes,
                Tables = tables,
                Columns = columns,
                TablePrimaryKeys = tablePrimaryKeys,
                TableForeignKeys = tableForeignKeys,
                TableIndices = tableIndices,
                Views = views
            };
        }

        public abstract string GenerateSchemaScripts(SchemaInfo schemaInfo);
        public abstract string TranslateColumn(Table table, TableColumn column);

        public abstract long GetTableRecordCount(DbConnection connection, Table table);
        public abstract Task<long> GetTableRecordCountAsync(DbConnection connection, Table table);
        protected long GetTableRecordCount(DbConnection connection, string sql)
        {
            return connection.ExecuteScalar<long>(sql);
        }

        protected async Task<long> GetTableRecordCountAsync(DbConnection connection, string sql)
        {
            return await connection.ExecuteScalarAsync<long>(sql);
        }

        public virtual string GenerateDataScripts(SchemaInfo schemaInfo)
        {
            return this.InternalGenerateDataScripts(schemaInfo, false).Result;
        }
        public virtual Task<string> GenerateDataScriptsAsync(SchemaInfo schemaInfo)
        {
            return this.InternalGenerateDataScripts(schemaInfo, true);
        }

        private async Task<string> InternalGenerateDataScripts(SchemaInfo schemaInfo, bool async = false)
        {
            StringBuilder sb = new StringBuilder();

            if (Option.ScriptOutputMode.HasFlag(GenerateScriptOutputMode.WriteToFile))
            {
                this.AppendScriptsToFile("", GenerateScriptMode.Data, true);
            }

            int i = 0;
            int pickupIndex = -1;
            if (schemaInfo.PickupTable != null)
            {
                foreach (Table table in schemaInfo.Tables)
                {
                    if (table.Owner == schemaInfo.PickupTable.Owner && table.Name == schemaInfo.PickupTable.Name)
                    {
                        pickupIndex = i;
                        break;
                    }
                    i++;
                }
            }

            i = 0;
            using (DbConnection connection = this.GetDbConnector().CreateConnection())
            {
                int tableCount = schemaInfo.Tables.Count - (pickupIndex == -1 ? 0 : pickupIndex + 1);
                int count = 0;
                foreach (Table table in schemaInfo.Tables)
                {
                    if (i < pickupIndex)
                    {
                        i++;
                        continue;
                    }

                    count++;
                    string strTableCount = $"({count}/{tableCount})";
                    string tableName = table.Name;
                    List<TableColumn> columns = schemaInfo.Columns.Where(item => item.Owner == table.Owner && item.TableName == tableName).OrderBy(item => item.Order).ToList();

                    bool isSelfReference = TableReferenceHelper.IsSelfReference(tableName, schemaInfo.TableForeignKeys);

                    List<TablePrimaryKey> primaryKeys = schemaInfo.TablePrimaryKeys.Where(item => item.Owner == table.Owner && item.TableName == tableName).ToList();
                    string primaryKeyColumns = string.Join(",", primaryKeys.OrderBy(item => item.Order).Select(item => GetQuotedString(item.ColumnName)));

                    long total = await this.GetTableRecordCountAsync(connection, table);

                    if (Option.DataGenerateThreshold.HasValue && total > Option.DataGenerateThreshold.Value)
                    {
                        continue;
                    }

                    int pageSize = Option.DataBatchSize;

                    this.FeedbackInfo($"{strTableCount}Begin to read data from table {table.Name}, total rows:{total}.");

                    Dictionary<long, List<Dictionary<string, object>>> dictPagedData;
                    if (isSelfReference)
                    {
                        string parentColumnName = schemaInfo.TableForeignKeys.FirstOrDefault(item =>
                            item.Owner == table.Owner
                            && item.TableName == tableName
                            && item.ReferencedTableName == tableName)?.ColumnName;

                        string strWhere = $" WHERE {GetQuotedString(parentColumnName)} IS NULL";
                        dictPagedData = this.GetSortedPageDatas(connection, table, primaryKeyColumns, parentColumnName, columns, Option, strWhere);
                    }
                    else
                    {
                        dictPagedData = async ? await this.GetPagedDataListAsync(connection, table, columns, primaryKeyColumns, total, pageSize)
                            : this.GetPagedDataList(connection, table, columns, primaryKeyColumns, total, pageSize);
                    }

                    this.FeedbackInfo($"{strTableCount}End read data from table {table.Name}.");

                    this.AppendDataScripts(Option, sb, table, columns, dictPagedData);

                    i++;
                }
            }

            var dataScripts = string.Empty;
            try
            {
                dataScripts = sb.ToString();
            }
            catch (OutOfMemoryException ex)
            {
                this.FeedbackError("Exception occurs:" + ex.Message);
            }
            finally
            {
                sb.Clear();
            }
            return dataScripts;
        }
        private Dictionary<long, List<Dictionary<string, object>>> GetSortedPageDatas(DbConnection connection, Table table, string primaryKeyColumns, string parentColumnName, List<TableColumn> columns, GenerateScriptOption option, string whereClause = "")
        {
            string quotedTableName = this.GetQuotedObjectName(table);

            int pageSize = option.DataBatchSize;

            long total = Convert.ToInt64(this.GetScalar(connection, $"SELECT COUNT(1) FROM {quotedTableName} {whereClause}"));

            var dictPagedData = this.GetPagedDataList(connection, table, columns, primaryKeyColumns, total, pageSize, whereClause);

            List<object> parentValues = dictPagedData.Values.SelectMany(item => item.Select(t => t[primaryKeyColumns.Trim(QuotationLeftChar, QuotationRightChar)])).ToList();

            if (parentValues.Count > 0)
            {
                TableColumn parentColumn = columns.FirstOrDefault(item => item.Owner == table.Owner && item.ColumnName == parentColumnName);

                long parentValuesPageCount = PaginationHelper.GetPageCount(parentValues.Count, option.InQueryItemLimitCount);

                for (long parentValuePageNumber = 1; parentValuePageNumber <= parentValuesPageCount; parentValuePageNumber++)
                {
                    IEnumerable<object> pagedParentValues = parentValues.Skip((int)(parentValuePageNumber - 1) * pageSize).Take(option.InQueryItemLimitCount);
                    whereClause = $" WHERE {GetQuotedString(parentColumnName)} IN ({string.Join(",", pagedParentValues.Select(item => SelectDataScriptValue(parentColumn, item, true)))})";
                    total = Convert.ToInt64(this.GetScalar(connection, $"SELECT COUNT(1) FROM {quotedTableName} {whereClause}"));

                    if (total > 0)
                    {
                        Dictionary<long, List<Dictionary<string, object>>> dictChildPagedData = this.GetSortedPageDatas(connection, table, primaryKeyColumns, parentColumnName, columns, option, whereClause);

                        foreach (var kp in dictChildPagedData)
                        {
                            long pageNumber = dictPagedData.Keys.Max(item => item);
                            dictPagedData.Add(pageNumber + 1, kp.Value);
                        }
                    }
                }
            }

            return dictPagedData;
        }

        private Dictionary<long, List<Dictionary<string, object>>> GetPagedDataList(DbConnection connection, Table table, List<TableColumn> columns, string primaryKeyColumns, long total, int pageSize, string whereClause = "")
        {
            return this.InteralGetPagedDataList(connection, table, columns, primaryKeyColumns, total, pageSize, whereClause, false).Result;
        }

        private Task<Dictionary<long, List<Dictionary<string, object>>>> GetPagedDataListAsync(DbConnection connection, Table table, List<TableColumn> columns, string primaryKeyColumns, long total, int pageSize, string whereClause = "")
        {
            return this.InteralGetPagedDataList(connection, table, columns, primaryKeyColumns, total, pageSize, whereClause, true);
        }

        private async Task<Dictionary<long, List<Dictionary<string, object>>>> InteralGetPagedDataList(DbConnection connection, Table table, List<TableColumn> columns, string primaryKeyColumns, long total, int pageSize, string whereClause = "", bool async = false)
        {
            string quotedTableName = this.GetQuotedObjectName(table);
            string columnNames = this.GetQuotedColumnNames(columns);

            var dictPagedData = new Dictionary<long, List<Dictionary<string, object>>>();

            long pageCount = PaginationHelper.GetPageCount(total, pageSize);

            for (long pageNumber = 1; pageNumber <= pageCount; pageNumber++)
            {
                string pagedSql = this.GetPagedSql(quotedTableName, columnNames, primaryKeyColumns, whereClause, pageNumber, pageSize);

                var dataTable = async ? await this.GetDataTableAsync(connection, pagedSql) : this.GetDataTable(connection, pagedSql);

                List<Dictionary<string, object>> rows = new List<Dictionary<string, object>>();
                foreach (DataRow row in dataTable.Rows)
                {
                    var dicField = new Dictionary<string, object>();
                    for (var i = 0; i < dataTable.Columns.Count; i++)
                    {
                        DataColumn column = dataTable.Columns[i];
                        string columnName = column.ColumnName;
                        if (columnName == RowNumberColumnName)
                        {
                            continue;
                        }

                        TableColumn tableColumn = columns.FirstOrDefault(item => item.ColumnName == columnName);

                        object value = row[i];

                        if (this.IsBytes(value) && this.Option.TreatBytesAsNullForData)
                        {
                            value = null;
                        }

                        object newValue = this.GetInsertValue(tableColumn, value);
                        dicField.Add(columnName, newValue);
                    }

                    rows.Add(dicField);
                }

                dictPagedData.Add(pageNumber, rows);

                if (this.OnDataRead != null)
                {
                    this.FeedbackInfo($"Transfer data from table {table.Name}, rows:{rows.Count}.");
                    this.OnDataRead(table, columns, rows, dataTable);
                }
            }

            return dictPagedData;
        }


        protected abstract string GetPagedSql(string tableName, string columnNames, string primaryKeyColumns, string whereClause, long pageNumber, int pageSize);

        protected virtual object GetInsertValue(TableColumn column, object value)
        {
            return value;
        }

        public virtual Dictionary<string, object> AppendDataScripts(GenerateScriptOption option, StringBuilder sb, Table table, List<TableColumn> columns, Dictionary<long, List<Dictionary<string, object>>> dictPagedData)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>();

            bool appendString = option.ScriptOutputMode.HasFlag(GenerateScriptOutputMode.WriteToString);
            bool appendFile = option.ScriptOutputMode.HasFlag(GenerateScriptOutputMode.WriteToFile);

            List<string> excludeColumnNames = new List<string>();
            if (option.GenerateIdentity && !option.InsertIdentityValue)
            {
                excludeColumnNames = columns.Where(item => item.IsIdentity).Select(item => item.ColumnName).ToList();
            }

            foreach (var kp in dictPagedData)
            {
                StringBuilder sbFilePage = new StringBuilder(Environment.NewLine);

                string tableName = this.GetQuotedObjectName(table);
                string insert = $"{this.GetBatchInsertPrefix()} {tableName}({this.GetQuotedColumnNames(columns.Where(item => !excludeColumnNames.Contains(item.ColumnName)))})VALUES";

                if (appendString)
                {
                    sb.AppendLine(insert);
                }

                if (appendFile)
                {
                    sbFilePage.AppendLine(insert);
                }

                int rowCount = 0;
                foreach (var row in kp.Value)
                {
                    rowCount++;

                    string values = this.GetInsertValues(columns, excludeColumnNames, kp.Key, rowCount - 1, row, out var insertParameters, out var valuesWithoutParameter);

                    if (insertParameters != null)
                    {
                        foreach (var para in insertParameters)
                        {
                            parameters.Add(para.Key, para.Value);
                        }
                    }

                    values = $"{this.GetBatchInsertItemBefore(tableName, rowCount == 1)}{values}{this.GetBatchInsertItemEnd(rowCount == kp.Value.Count)}";

                    if (option.RemoveEmoji)
                    {
                        values = StringHelper.RemoveEmoji(values);
                    }

                    if (appendString)
                    {
                        sb.AppendLine(values);
                    }

                    if (appendFile)
                    {
                        sbFilePage.AppendLine(valuesWithoutParameter);
                    }
                }

                if (appendString)
                {
                    sb.AppendLine();
                }

                if (appendFile)
                {
                    sbFilePage.AppendLine();

                    this.AppendScriptsToFile(sbFilePage.ToString(), GenerateScriptMode.Data);
                }
            }

            return parameters;
        }

        protected virtual string GetBatchInsertPrefix()
        {
            return "INSERT INTO";
        }

        protected virtual string GetBatchInsertItemBefore(string tableName, bool isFirstRow)
        {
            return "";
        }

        protected virtual string GetBatchInsertItemEnd(bool isAllEnd)
        {
            return (isAllEnd ? ";" : ",");
        }

        public virtual void AppendScriptsToFile(string content, GenerateScriptMode generateScriptMode, bool clearAll = false)
        {
            string fileName = $"{this.ConnectionInfo.Database}_{this.GetType().Name.Replace("Interpreter", "")}_{DateTime.Today.ToString("yyyyMMdd")}_{generateScriptMode.ToString()}.sql";
            string filePath = Path.Combine(Option.ScriptOutputFolder, fileName);

            string directoryName = Path.GetDirectoryName(filePath);

            if (!Directory.Exists(directoryName))
            {
                Directory.CreateDirectory(directoryName);
            }

            if (!clearAll)
            {
                File.AppendAllText(filePath, content, Encoding.UTF8);
            }
            else
            {
                File.WriteAllText(filePath, content, Encoding.UTF8);
            }
        }

        private string GetInsertValues(List<TableColumn> columns, List<string> excludeColumnNames, long pageNumber, int rowIndex, Dictionary<string, object> row, out Dictionary<string, object> parameters, out string valuesWithoutParameter)
        {
            valuesWithoutParameter = "";
            parameters = new Dictionary<string, object>();

            List<object> values = new List<object>();
            List<string> parameterPlaceholders = new List<string>();
            foreach (TableColumn column in columns)
            {
                if (!excludeColumnNames.Contains(column.ColumnName))
                {
                    object value = this.SelectDataScriptValue(column, row[column.ColumnName]);

                    if (!this.Option.TreatBytesAsNullForScript && (this.BytesAsParameter(value) || this.NeedInsertParameter(value)))
                    {
                        string parameterName = $"P{pageNumber}_{rowIndex}_{column.ColumnName}";
                        parameters.Add(this.CommandParameterChar + parameterName, value);

                        string parameterPlaceholder = this.CommandParameterChar + parameterName;
                        values.Add(parameterPlaceholder);
                        parameterPlaceholders.Add(parameterPlaceholder);
                    }
                    else
                    {
                        values.Add(value);
                    }
                }
            }

            valuesWithoutParameter = $"({string.Join(",", values.Select(item => parameterPlaceholders.Contains(item) ? "NULL" : item))})";

            return $"({string.Join(",", values.Select(item => item))})";
        }

        private bool BytesAsParameter(object value)
        {
            return this.IsBytes(value);
        }

        public bool IsBytes(object value)
        {
            return value != null && value.GetType() == typeof(byte[]);
        }

        protected virtual bool NeedInsertParameter(object value)
        {
            return false;
        }

        private object SelectDataScriptValue(TableColumn column, object value, bool byteAsString = false)
        {
            if (value != null)
            {
                Type type = value.GetType();
                bool needQuotated = false;
                string strValue = "";

                if (type == typeof(DBNull))
                {
                    return "NULL";
                }
                else if (type == typeof(Byte[]))
                {
                    if (((Byte[])value).Length == 16) //GUID
                    {
                        if (this.GetType() == typeof(SqlServerInterpreter) && column.DataType.ToLower() == "uniqueidentifier")
                        {
                            needQuotated = true;
                            strValue = new Guid((byte[])value).ToString();
                        }
                        else if (this.GetType() == typeof(MySqlInterpreter) && column.DataType == "char" && column.MaxLength == 36)
                        {
                            needQuotated = true;
                            strValue = new Guid((byte[])value).ToString();
                        }
                        else if (byteAsString && this.GetType() == typeof(OracleInterpreter) && column.DataType.ToLower() == "raw" && column.MaxLength == 16)
                        {
                            needQuotated = true;
                            strValue = StringHelper.GuidToRaw(new Guid((byte[])value).ToString());
                        }
                        else
                        {
                            return value;
                        }
                    }
                    else
                    {
                        return value;
                    }
                }

                bool oracleSemicolon = false;

                switch (type.Name)
                {
                    case nameof(Guid):
                        needQuotated = true;
                        if (this.GetType() == typeof(OracleInterpreter) && column.DataType.ToLower() == "raw" && column.MaxLength == 16)
                        {
                            strValue = StringHelper.GuidToRaw(value.ToString());
                        }
                        else
                        {
                            strValue = value.ToString();
                        }
                        break;
                    case nameof(String):
                        needQuotated = true;
                        strValue = value.ToString();
                        if (this.GetType() == typeof(OracleInterpreter))
                        {
                            if (strValue.Contains(";"))
                            {
                                oracleSemicolon = true;
                            }
                        }
                        break;
                    case nameof(DateTime):
                        if (this.GetType() == typeof(OracleInterpreter))
                        {
                            strValue = $"TO_TIMESTAMP('{Convert.ToDateTime(value).ToString("yyyy-MM-dd HH:mm:ss.fff")}','yyyy-MM-dd hh24:mi:ss.ff')";
                        }
                        else
                        {
                            needQuotated = true;
                            strValue = value.ToString();
                        }
                        break;
                    case nameof(Boolean):
                        strValue = value.ToString() == "True" ? "1" : "0";
                        break;
                    default:
                        if (string.IsNullOrEmpty(strValue))
                        {
                            strValue = value.ToString();
                        }
                        break;
                }

                if (needQuotated)
                {
                    strValue = $"{this.GetUnicodeInsertChar()}'{ValueHelper.TransferSingleQuotation(strValue)}'";

                    if (oracleSemicolon)
                    {
                        strValue = strValue.Replace(";", $"'{OracleInterpreter.CONNECT_CHAR}{OracleInterpreter.SEMICOLON_FUNC}{OracleInterpreter.CONNECT_CHAR}'");
                    }

                    return strValue;
                }
                else
                {
                    return strValue;
                }
            }
            else
            {
                return null;
            }
        }

        protected virtual string GetUnicodeInsertChar()
        {
            return UnicodeInsertChar;
        }

        protected virtual string GetColumnDefaultValue(TableColumn column)
        {
            bool isChar = DataTypeHelper.IsCharType(column.DataType);
            if(isChar && !column.DefaultValue.StartsWith("'"))
            {
                return $"'{column.DefaultValue}'";
            }
            return column.DefaultValue;
        }
        #endregion
    }
}
