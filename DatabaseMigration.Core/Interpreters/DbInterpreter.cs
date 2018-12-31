using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseMigration.Core
{
    public abstract class DbInterpreter
    {
        #region Fields
        public const string UnicodeInsertChar = "N";
        public const string RowNumberColumnName = "ROWNUMBER";
        public abstract string CommandParameterChar { get; }
        public abstract char QuotationLeftChar { get; }
        public abstract char QuotationRightChar { get; }
        public abstract DatabaseType DatabaseType { get; }
        public GenerateScriptOption Option { get; set; } = new GenerateScriptOption();
        public ConnectionInfo ConnectionInfo { get; set; }

        public delegate void DataReadHandler(Table table, List<TableColumn> columns, List<Dictionary<string, object>> data);
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
        public string GetDisplayTableName(Table table, bool useQuotedString = false)
        {
            if (this.GetType().Name == nameof(SqlServerInterpreter))
            {
                return $"{GetString(table.Owner, useQuotedString)}.{GetString(table.Name, useQuotedString)}";
            }
            return $"{GetString(table.Name, useQuotedString)}";
        }

        private string GetString(string str, bool useQuotedString = false)
        {
            return useQuotedString ? GetQuotedString(str) : str;
        }

        public abstract DbConnector GetDbConnector();
        protected string GetQuotedTableName(Table table)
        {
            return this.GetDisplayTableName(table, true);
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
            DbCommander dbCommander = new DbCommander(dbConnection, System.Data.CommandType.Text, sql);
            DataTable table = dbCommander.ExecteDataTable();
            return table;
        }

        public void ExecuteNonQuery(string sql, Dictionary<string, object> paramaters = null)
        {
            this.ExecuteNonQuery(this.GetDbConnector().CreateConnection(), sql, paramaters);
        }

        public void ExecuteNonQuery(DbConnection dbConnection, string sql, Dictionary<string, object> paramaters = null, bool disposeConnection = true)
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
                        dbCommander.DbCommand.Parameters.AddRange(cmdParams.ToArray());                        
                    }
                }

                dbCommander.ExecuteNonQuery();
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

        #region Table
        public abstract List<Table> GetTables(params string[] tableNames);
        protected List<Table> GetTables(DbConnector dbConnector, string sql)
        {
            List<Table> tables = new List<Table>();
            using (DbConnection dbConnection = dbConnector.CreateConnection())
            {
                DbDataReader dataReader = this.GetDataReader(dbConnection, sql);
                if (dataReader.HasRows)
                {
                    int i = 1;
                    while (dataReader.Read())
                    {
                        Table table = new Table()
                        {
                            Owner = dataReader["Owner"]?.ToString(),
                            Name = dataReader["Name"]?.ToString(),
                            Comment = dataReader["Comment"]?.ToString(),
                            IdentitySeed = StringHelper.Convert2Int(dataReader["IdentitySeed"]?.ToString()),
                            IdentityIncrement = StringHelper.Convert2Int(dataReader["IdentityIncrement"]?.ToString()),
                            Order = i++
                        };

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
            List<TableColumn> columns = new List<TableColumn>();
            using (DbConnection dbConnection = dbConnector.CreateConnection())
            {
                DbDataReader dataReader = this.GetDataReader(dbConnection, sql);
                if (dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        TableColumn column = new TableColumn()
                        {
                            Owner = dataReader["Owner"]?.ToString(),
                            TableName = dataReader["TableName"]?.ToString(),
                            ColumnName = dataReader["ColumnName"]?.ToString(),
                            DataType = dataReader["DataType"]?.ToString(),
                            Comment = dataReader["Comment"]?.ToString(),
                            IsRequired = !StringHelper.Convert2Bool(dataReader["IsNullable"]?.ToString()),
                            IsIdentity = StringHelper.Convert2Bool(dataReader["IsIdentity"]?.ToString()),
                            MaxLength = StringHelper.Convert2Long(dataReader["MaxLength"]?.ToString()),
                            Precision = StringHelper.Convert2Int(dataReader["Precision"]?.ToString()),
                            Scale = StringHelper.Convert2Int(dataReader["Scale"]?.ToString()),
                            Order = StringHelper.Convert2Int(dataReader["Order"]?.ToString()) ?? 0,
                            DefaultValue = dataReader["DefaultValue"]?.ToString()
                        };

                        columns.Add(column);
                    }
                }
            }

            return columns;
        }
        #endregion

        #region Table Primary Key
        public abstract List<TablePrimaryKey> GetTablePrimaryKeys(params string[] tableNames);

        protected List<TablePrimaryKey> GetTablePrimaryKeys(DbConnector dbConnector, string sql)
        {
            List<TablePrimaryKey> tablePrimaryKeys = new List<TablePrimaryKey>();
            using (DbConnection dbConnection = dbConnector.CreateConnection())
            {
                DbDataReader dataReader = this.GetDataReader(dbConnection, sql);
                if (dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        TablePrimaryKey tablePrimaryKey = new TablePrimaryKey()
                        {
                            Owner = dataReader["Owner"]?.ToString(),
                            TableName = dataReader["TableName"]?.ToString(),
                            KeyName = dataReader["KeyName"]?.ToString(),
                            ColumnName = dataReader["ColumnName"]?.ToString(),
                            Order = StringHelper.Convert2Int(dataReader["Order"]?.ToString()) ?? 0,
                            IsDesc = StringHelper.Convert2Bool(dataReader["IsDesc"]?.ToString())
                        };

                        tablePrimaryKeys.Add(tablePrimaryKey);
                    }
                }
            }

            return tablePrimaryKeys;
        }
        #endregion

        #region Table Foreign Key
        public abstract List<TableForeignKey> GetTableForeignKeys(params string[] tableNames);

        protected List<TableForeignKey> GetTableForeignKeys(DbConnector dbConnector, string sql)
        {
            List<TableForeignKey> tableForeignKeys = new List<TableForeignKey>();
            using (DbConnection dbConnection = dbConnector.CreateConnection())
            {
                DbDataReader dataReader = this.GetDataReader(dbConnection, sql);
                if (dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        TableForeignKey tableForeignKey = new TableForeignKey()
                        {
                            Owner = dataReader["Owner"]?.ToString(),
                            TableName = dataReader["TableName"]?.ToString(),
                            KeyName = dataReader["KeyName"]?.ToString(),
                            ColumnName = dataReader["ColumnName"]?.ToString(),
                            ReferencedTableName = dataReader["ReferencedTableName"]?.ToString(),
                            ReferencedColumnName = dataReader["ReferencedColumnName"]?.ToString(),
                            UpdateCascade = StringHelper.Convert2Bool(dataReader["UpdateCascade"]?.ToString()),
                            DeleteCascade = StringHelper.Convert2Bool(dataReader["DeleteCascade"]?.ToString())
                        };

                        tableForeignKeys.Add(tableForeignKey);
                    }
                }
            }

            return tableForeignKeys;
        }
        #endregion

        #region Table Index
        public abstract List<TableIndex> GetTableIndexes(params string[] tableNames);

        protected List<TableIndex> GetTableIndexes(DbConnector dbConnector, string sql)
        {
            List<TableIndex> tableIndexes = new List<TableIndex>();
            using (DbConnection dbConnection = dbConnector.CreateConnection())
            {
                DbDataReader dataReader = this.GetDataReader(dbConnection, sql);
                if (dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        TableIndex tableIndex = new TableIndex()
                        {
                            Owner = dataReader["Owner"]?.ToString(),
                            TableName = dataReader["TableName"]?.ToString(),
                            IndexName = dataReader["IndexName"]?.ToString(),
                            ColumnName = dataReader["ColumnName"]?.ToString(),
                            Order = StringHelper.Convert2Int(dataReader["Order"]?.ToString()) ?? 0,
                            IsUnique = StringHelper.Convert2Bool(dataReader["IsUnique"]?.ToString()),
                            IsDesc = StringHelper.Convert2Bool(dataReader["IsDesc"]?.ToString())
                        };

                        tableIndexes.Add(tableIndex);
                    }
                }
            }

            return tableIndexes;
        }
        #endregion

        #region Identity
        public virtual void SetIdentityEnabled(DbConnection dbConnection, TableColumn column, bool enabled) { }   
        #endregion

        #region Generate Scripts

        public virtual SchemaInfo GetSchemaInfo(string[] tableNames)
        {
            List<Table> tables = this.GetTables(tableNames);
            List<TableColumn> columns = this.GetTableColumns(tableNames);
            List<TablePrimaryKey> tablePrimaryKeys = new List<TablePrimaryKey>();
            List<TableForeignKey> tableForeignKeys = new List<TableForeignKey>();
            List<TableIndex> tableIndices = new List<TableIndex>();

            if (Option.GenerateKey)
            {
                tablePrimaryKeys = this.GetTablePrimaryKeys(tableNames);
                tableForeignKeys = this.GetTableForeignKeys(tableNames);
            }

            if (Option.GenerateIndex)
            {
                tableIndices = tableIndices = this.GetTableIndexes(tableNames);
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

            return new SchemaInfo() { Tables = tables, Columns = columns, TablePrimaryKeys = tablePrimaryKeys, TableForeignKeys = tableForeignKeys, TableIndices = tableIndices };
        }
        public abstract string GenerateSchemaScripts(SchemaInfo schemaInfo);
        public abstract string TranslateColumn(Table table, TableColumn column);

        public abstract long GetTableRecordCount(DbConnection connection, Table table);
        protected long GetTableRecordCount(DbConnection connection, string sql)
        {
            object obj = this.GetScalar(connection, sql);
            if (obj != null)
            {
                return Convert.ToInt64(obj);
            }
            return 0;
        }

        public virtual string GenerateDataScripts(SchemaInfo schemaInfo)
        {
            StringBuilder sb = new StringBuilder();

            if (Option.ScriptOutputMode == GenerateScriptOutputMode.WriteToFile)
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
                    
                    long total = this.GetTableRecordCount(connection, table);

                    if (Option.DataGenerateThreshold.HasValue && total > Option.DataGenerateThreshold.Value)
                    {
                        continue;
                    }

                    int pageSize = Option.DataBatchSize;

                    this.FeedbackInfo($"{strTableCount}Begin to read data from table {table.Name}, total rows:{total}.");

                    Dictionary<long, List<Dictionary<string, object>>> dictPagedData = new Dictionary<long, List<Dictionary<string, object>>>();
                    if (isSelfReference)
                    {
                        string parentColumnName = schemaInfo.TableForeignKeys.FirstOrDefault(item => item.Owner == table.Owner && item.TableName == tableName && item.ReferencedTableName == tableName).ColumnName;

                        string strWhere = $" WHERE {GetQuotedString(parentColumnName)} IS NULL";
                        dictPagedData = this.GetSortedPageDatas(connection, table, primaryKeyColumns, parentColumnName, columns, Option, strWhere);
                    }
                    else
                    {
                        dictPagedData = this.GetPagedDatas(connection, table, columns, primaryKeyColumns, total, pageSize);
                    }

                    this.FeedbackInfo($"{strTableCount}End read data from table {table.Name}.");

                    this.AppendDataScripts(Option, sb, table, columns, dictPagedData);

                    i++;                   
                }
            }

            return sb.ToString();
        }

        private Dictionary<long, List<Dictionary<string, object>>> GetSortedPageDatas(DbConnection connection, Table table, string primaryKeyColumns, string parentColumnName, List<TableColumn> columns, GenerateScriptOption option, string whereClause = "")
        {
            string columnNames = this.GetQuotedColumnNames(columns);
            string quotedTableName = this.GetQuotedTableName(table);

            int pageSize = option.DataBatchSize;

            long total = Convert.ToInt64(this.GetScalar(connection, $"SELECT COUNT(1) FROM {quotedTableName} {whereClause}"));

            Dictionary<long, List<Dictionary<string, object>>> dictPagedData = this.GetPagedDatas(connection, table, columns, primaryKeyColumns, total, pageSize, whereClause);

            List<object> parentValues = dictPagedData.Values.SelectMany(item => item.Select(t => t[primaryKeyColumns.Trim(new char[] { QuotationLeftChar, QuotationRightChar })])).ToList();

            if (parentValues != null && parentValues.Count > 0)
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

        private Dictionary<long, List<Dictionary<string, object>>> GetPagedDatas(DbConnection connection, Table table, List<TableColumn> columns, string primaryKeyColumns, long total, int pageSize, string whereClause = "")
        {
            string quotedTableName = this.GetQuotedTableName(table);
            string columnNames = this.GetQuotedColumnNames(columns);

            Dictionary<long, List<Dictionary<string, object>>> dictPagedData = new Dictionary<long, List<Dictionary<string, object>>>();

            long pageCount = PaginationHelper.GetPageCount(total, pageSize);

            for (long pageNumber = 1; pageNumber <= pageCount; pageNumber++)
            {
                string pagedSql = this.GetPagedSql(quotedTableName, columnNames, primaryKeyColumns, whereClause, pageNumber, pageSize);

                DbDataReader dataReader = this.GetDataReader(connection, pagedSql);
                List<Dictionary<string, object>> rows = new List<Dictionary<string, object>>();

                while (dataReader.Read())
                {
                    Dictionary<string, object> dicField = new Dictionary<string, object>();
                    for (int i = 0; i < dataReader.FieldCount; i++)
                    {
                        string columnName = dataReader.GetName(i);
                        if (columnName != RowNumberColumnName)
                        {
                            TableColumn column = columns.FirstOrDefault(item => item.ColumnName == columnName);
                            object value = dataReader[i];

                            if (this.IsBytes(value) && this.Option.TreatBytesAsNullForData)
                            {
                                value = null;
                            }
                            
                            object newValue = this.GetInsertValue(column, value);                           

                            dicField.Add(columnName, newValue);
                        }
                    }
                    rows.Add(dicField);
                }
                dataReader.Close();

                dictPagedData.Add(pageNumber, rows);

                if (this.OnDataRead != null)
                {
                    this.FeedbackInfo($"Transfer data from table {table.Name}, rows:{rows.Count}.");
                    this.OnDataRead(table, columns, rows);
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

                string tableName = this.GetQuotedTableName(table);
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
                foreach (Dictionary<string, object> row in kp.Value)
                {
                    rowCount++;

                    Dictionary<string, object> insertParameters;

                    string valuesWithoutParameter = "";
                    string values = this.GetInsertValues(columns, excludeColumnNames, rowCount - 1, row, out insertParameters, out valuesWithoutParameter);

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

        private string GetInsertValues(List<TableColumn> columns, List<string> excludeColumnNames, int rowIndex, Dictionary<string, object> row, out Dictionary<string, object> parameters, out string valuesWithoutParameter)
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
                        string parameterName = $"P{rowIndex}_{column.ColumnName}";
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

            valuesWithoutParameter = $"({string.Join(",", values.Select(item => parameterPlaceholders.Contains(item)? "NULL": item))})";

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
                    if(((Byte[])value).Length == 16) //GUID
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
        #endregion
    }
}
