using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseMigration.Core
{
    public class OracleInterpreter : DbInterpreter
    {
        #region Field & Property
        public const string SEMICOLON_FUNC = "CHR(59)";
        public const string CONNECT_CHAR = "||";
        public override string CommandParameterChar { get { return ":"; } }
        public override char QuotationLeftChar { get { return '"'; } }
        public override char QuotationRightChar { get { return '"'; } }
        public override DatabaseType DatabaseType { get { return DatabaseType.Oracle; } }
        public override bool SupportBulkCopy { get { return false; } }
        #endregion

        #region Common Method
        public override async Task<int> BulkCopyAsync(
            DbConnection connection,
            DataTable dataTable,
            string destinationTableName = null,
            int? bulkCopyTimeout = null,
            int? batchSize = null)
        {
            throw new NotImplementedException();
        }
        public override int BulkCopy(
            DbConnection connection,
            DataTable dataTable,
            string destinationTableName = null,
            int? bulkCopyTimeout = null,
            int? batchSize = null)
        {
            throw new NotImplementedException();
        }

        public OracleInterpreter(ConnectionInfo connectionInfo, GenerateScriptOption options) : base(connectionInfo, options) { }

        public override DbConnector GetDbConnector()
        {
            return new DbConnector(new OracleProvider(), new OracleConnectionBuilder(), this.ConnectionInfo);
        }

        protected override IEnumerable<DbParameter> BuildCommandParameters(Dictionary<string, object> paramaters)
        {
            foreach (KeyValuePair<string, object> kp in paramaters)
            {
                yield return new OracleParameter(kp.Key, kp.Value);
            }
        }
        #endregion

        #region Database
        public override List<Database> GetDatabases()
        {
            DbConnector dbConnector = this.GetDbConnector();

            string sql = $@"SELECT TABLESPACE_NAME AS ""Name"" FROM USER_TABLESPACES";

            return base.GetDatabases(dbConnector, sql);
        }
        #endregion

        #region User Defined Type
        public override List<UserDefinedType> GetUserDefinedTypes(params string[] typeNames)
        {
            return new List<UserDefinedType>();
        }

        public override async Task<List<UserDefinedType>> GetUserDefinedTypesAsync(params string[] typeNames)
        {
            return await Task.Run(() => GetUserDefinedTypes(typeNames));
        }
        #endregion

        #region Table

        private string GetSqlForGetTables(params string[] tableNames)
        {
            string sql = $@"SELECT T.OWNER AS ""Owner"", T.TABLE_NAME AS ""Name"", C.COMMENTS AS ""Comment"",
                          1 AS ""IdentitySeed"", 1 AS ""IdentityIncrement""
                          FROM ALL_TABLES T
                          LEFT JOIN USER_TAB_COMMENTS C ON T.TABLE_NAME= C.TABLE_NAME
                          WHERE UPPER(OWNER)=UPPER('{ConnectionInfo.UserId}')";

            if (tableNames != null && tableNames.Count() > 0)
            {
                string strTableNames = StringHelper.GetSingleQuotedString(tableNames);
                sql += $" AND T.TABLE_NAME IN ({ strTableNames })";
            }

            sql += " ORDER BY T.TABLE_NAME";

            return sql;
        }
        public override List<Table> GetTables(params string[] tableNames)
        {
            DbConnector dbConnector = this.GetDbConnector();

            string sql = GetSqlForGetTables(tableNames);

            return base.GetTables(dbConnector, sql);
        }

        public override async Task<List<Table>> GetTablesAsync(params string[] tableNames)
        {
            DbConnector dbConnector = this.GetDbConnector();

            string sql = GetSqlForGetTables(tableNames);

            return await base.GetTablesAsync(dbConnector, sql);
        }
        #endregion

        #region Table Column
        public override List<TableColumn> GetTableColumns(params string[] tableNames)
        {
            DbConnector dbConnector = this.GetDbConnector();

            string sql = $@"SELECT OWNER AS ""Owner"", C.TABLE_NAME AS ""TableName"",C.COLUMN_NAME AS ""ColumnName"",DATA_TYPE AS ""DataType"",CASE NULLABLE WHEN 'Y' THEN 1 ELSE 0 END AS ""IsNullable"", DATA_LENGTH AS ""MaxLength"",
                 DATA_PRECISION AS ""Precision"",DATA_SCALE AS ""Scale"", COLUMN_ID AS ""Order"", DATA_DEFAULT AS ""DefaultValue"", 0 AS ""IsIdentity"", CC.COMMENTS AS ""Comment"" , '' AS ""TypeOwner""
                 FROM ALL_TAB_COLUMNS C
                 LEFT JOIN USER_COL_COMMENTS CC ON C.TABLE_NAME=CC.TABLE_NAME AND C.COLUMN_NAME=CC.COLUMN_NAME
                 WHERE UPPER(OWNER)=UPPER('{ConnectionInfo.UserId}')";

            if (tableNames != null && tableNames.Count() > 0)
            {
                string strTableNames = StringHelper.GetSingleQuotedString(tableNames);
                sql += $" AND C.TABLE_NAME IN ({ strTableNames })";
            }

            return base.GetTableColumns(dbConnector, sql);
        }
        #endregion

        #region Table Primary Key
        public override List<TablePrimaryKey> GetTablePrimaryKeys(params string[] tableNames)
        {
            DbConnector dbConnector = this.GetDbConnector();

            string sql = $@"SELECT UC.OWNER AS ""Owner"", UC.TABLE_NAME AS ""TableName"",UC.CONSTRAINT_NAME AS ""KeyName"",UCC.COLUMN_NAME AS ""ColumnName"", UCC.POSITION AS ""Order"", 0 AS ""IsDesc""
                        FROM USER_CONSTRAINTS UC
                        JOIN USER_CONS_COLUMNS UCC ON UC.OWNER=UCC.OWNER AND UC.TABLE_NAME=UCC.TABLE_NAME AND UC.CONSTRAINT_NAME=UCC.CONSTRAINT_NAME  
                        WHERE UC.CONSTRAINT_TYPE='P' AND UPPER(UC.OWNER)=UPPER('{ConnectionInfo.UserId}')";

            if (tableNames != null && tableNames.Count() > 0)
            {
                string strTableNames = StringHelper.GetSingleQuotedString(tableNames);
                sql += $" AND UC.TABLE_NAME IN ({ strTableNames })";
            }

            return base.GetTablePrimaryKeys(dbConnector, sql);
        }
        #endregion

        #region Table Foreign Key
        public override List<TableForeignKey> GetTableForeignKeys(params string[] tableNames)
        {
            DbConnector dbConnector = this.GetDbConnector();

            string sql = $@"SELECT UC.OWNER AS ""Owner"", UC.TABLE_NAME AS ""TableName"", UC.CONSTRAINT_NAME AS ""KeyName"", UCC.column_name AS ""ColumnName"",
                        RUCC.TABLE_NAME AS ""ReferencedTableName"",RUCC.COLUMN_NAME AS ""ReferencedColumnName"",
                        0 AS ""UpdateCascade"", CASE UC.DELETE_RULE WHEN 'CASCADE' THEN 1 ELSE 0 END AS ""DeleteCascade"" 
                        FROM USER_CONSTRAINTS UC                       
                        JOIN USER_CONS_COLUMNS UCC ON UC.OWNER=UCC.OWNER AND UC.TABLE_NAME=UCC.TABLE_NAME AND UC.CONSTRAINT_NAME=UCC.CONSTRAINT_NAME                       
                        JOIN USER_CONS_COLUMNS RUCC ON UC.OWNER=RUCC.OWNER AND UC.R_CONSTRAINT_NAME=RUCC.CONSTRAINT_NAME AND UCC.POSITION=RUCC.POSITION
                        WHERE UC.CONSTRAINT_TYPE='R' AND UPPER(UC.OWNER)=UPPER('{ConnectionInfo.UserId}')";

            if (tableNames != null && tableNames.Count() > 0)
            {
                string strTableNames = StringHelper.GetSingleQuotedString(tableNames);
                sql += $" AND UC.TABLE_NAME IN ({ strTableNames })";
            }

            return base.GetTableForeignKeys(dbConnector, sql);
        }
        #endregion

        #region Table Index
        public override List<TableIndex> GetTableIndexes(params string[] tableNames)
        {
            DbConnector dbConnector = this.GetDbConnector();

            string sql = $@"SELECT UC.owner AS ""Owner"", ui.table_name AS ""TableName"", ui.index_name AS ""IndexName"", uic.column_name AS ""ColumnName"", uic.column_position AS ""Order"",
                CASE uic.descend WHEN 'ASC' THEN 0 ELSE 1 END AS ""IsDesc"", CASE ui.uniqueness WHEN 'UNIQUE' THEN 1 ELSE 0 END AS ""IsUnique""
                FROM user_indexes ui
                JOIN user_ind_columns uic ON ui.index_name = uic.index_name AND ui.table_name = uic.table_name
                LEFT JOIN user_constraints uc ON ui.table_name = uc.table_name AND ui.table_owner = uc.owner AND ui.index_name = uc.constraint_name AND uc.constraint_type = 'P'
                WHERE uc.constraint_name IS NULL AND UPPER(UC.owner)=UPPER('{ConnectionInfo.UserId}')";

            if (tableNames != null && tableNames.Count() > 0)
            {
                string strTableNames = StringHelper.GetSingleQuotedString(tableNames);
                sql += $" AND UC.TABLE_NAME IN ({ strTableNames })";
            }

            return base.GetTableIndexes(dbConnector, sql);
        }
        #endregion

        #region View        
        public override List<View> GetViews(params string[] viewNames)
        {
            return this.InternalGetViews(false, viewNames).Result;
        }

        public override Task<List<View>> GetViewsAsync(params string[] viewNames)
        {
            return this.InternalGetViews(true, viewNames);
        }

        private async Task<List<View>> InternalGetViews(bool async = false, params string[] viewNames)
        {
            string sql = $@"SELECT V.OWNER AS ""Owner"", V.VIEW_NAME AS ""Name"",TEXT_VC AS ""Definition"" 
                        FROM ALL_VIEWS V
                        WHERE UPPER(OWNER) = UPPER('{ConnectionInfo.UserId}')";

            if (viewNames != null && viewNames.Any())
            {
                string strViewNames = StringHelper.GetSingleQuotedString(viewNames);
                sql += $" AND V.VIEW_NAME IN ({ strViewNames })";
            }

            sql += " ORDER BY VIEW_NAME";

            DbConnector dbConnector = this.GetDbConnector();

            return async ? await base.GetViewsAsync(dbConnector, sql) : base.GetViews(dbConnector, sql);
        }
        #endregion

        #region Generate Schema Script 

        public override string GenerateSchemaScripts(SchemaInfo schemaInfo)
        {
            StringBuilder sb = new StringBuilder();

            #region Create Table
            foreach (Table table in schemaInfo.Tables)
            {
                this.FeedbackInfo(OperationState.Begin, "table", table.Name);

                string tableName = table.Name;
                string quotedTableName = this.GetQuotedObjectName(table);

                IEnumerable<TableColumn> tableColumns = schemaInfo.Columns.Where(item => item.TableName == tableName).OrderBy(item => item.Order);

                IEnumerable<TablePrimaryKey> primaryKeys = schemaInfo.TablePrimaryKeys.Where(item => item.TableName == tableName);

                #region Create Table

                sb.Append(
$@"
CREATE TABLE {quotedTableName}(
{string.Join("," + Environment.NewLine, tableColumns.Select(item => this.TranslateColumn(table, item))).TrimEnd(',')}
)
TABLESPACE
{this.ConnectionInfo.Database};");
                #endregion

                sb.AppendLine();

                #region Comment
                if (!string.IsNullOrEmpty(table.Comment))
                {
                    sb.AppendLine($"COMMENT ON TABLE {this.ConnectionInfo.UserId}.{GetQuotedString(tableName)} IS '{ValueHelper.TransferSingleQuotation(table.Comment)}';");
                }

                foreach (TableColumn column in tableColumns.Where(item => !string.IsNullOrEmpty(item.Comment)))
                {
                    sb.AppendLine($"COMMENT ON COLUMN {this.ConnectionInfo.UserId}.{GetQuotedString(tableName)}.{GetQuotedString(column.ColumnName)} IS '{ValueHelper.TransferSingleQuotation(column.Comment)}';");
                }
                #endregion

                #region Primary Key
                if (Option.GenerateKey && primaryKeys.Count() > 0)
                {
                    string primaryKey =
$@"
ALTER TABLE {quotedTableName} ADD CONSTRAINT {primaryKeys.FirstOrDefault().KeyName} PRIMARY KEY 
(
{string.Join(Environment.NewLine, primaryKeys.Select(item => $"{ GetQuotedString(item.ColumnName)},")).TrimEnd(',')}
)
USING INDEX 
TABLESPACE
{this.ConnectionInfo.Database}
;";
                    sb.Append(primaryKey);
                }
                #endregion

                #region Foreign Key
                if (Option.GenerateKey)
                {
                    IEnumerable<TableForeignKey> foreignKeys = schemaInfo.TableForeignKeys.Where(item => item.TableName == tableName);
                    if (foreignKeys.Count() > 0)
                    {
                        sb.AppendLine();
                        ILookup<string, TableForeignKey> foreignKeyLookup = foreignKeys.ToLookup(item => item.KeyName);

                        IEnumerable<string> keyNames = foreignKeyLookup.Select(item => item.Key);

                        foreach (string keyName in keyNames)
                        {
                            TableForeignKey tableForeignKey = foreignKeyLookup[keyName].First();

                            string columnNames = string.Join(",", foreignKeyLookup[keyName].Select(item => $"{GetQuotedString(item.ColumnName)}"));
                            string referenceColumnName = string.Join(",", foreignKeyLookup[keyName].Select(item => $"{GetQuotedString(item.ReferencedColumnName)}"));

                            sb.Append(
$@"
ALTER TABLE {quotedTableName} ADD CONSTRAINT { GetQuotedString(keyName)} FOREIGN KEY ({columnNames})
REFERENCES { GetQuotedString(tableForeignKey.ReferencedTableName)}({referenceColumnName})
");

                            if (tableForeignKey.DeleteCascade)
                            {
                                sb.AppendLine("ON DELETE CASCADE");
                            }

                            sb.Append(";");
                        }
                    }
                }
                #endregion

                #region Index
                if (Option.GenerateIndex)
                {
                    IEnumerable<TableIndex> indices = schemaInfo.TableIndices.Where(item => item.TableName == tableName).OrderBy(item => item.Order);
                    if (indices.Count() > 0)
                    {
                        sb.AppendLine();

                        List<string> indexColumns = new List<string>();

                        ILookup<string, TableIndex> indexLookup = indices.ToLookup(item => item.IndexName);
                        IEnumerable<string> indexNames = indexLookup.Select(item => item.Key);
                        foreach (string indexName in indexNames)
                        {
                            TableIndex tableIndex = indexLookup[indexName].First();

                            string columnNames = string.Join(",", indexLookup[indexName].Select(item => $"{GetQuotedString(item.ColumnName)}"));

                            if (indexColumns.Contains(columnNames))
                            {
                                continue;
                            }

                            sb.AppendLine($"CREATE {(tableIndex.IsUnique ? "UNIQUE" : "")} INDEX { GetQuotedString(tableIndex.IndexName)} ON { GetQuotedString(tableName)} ({columnNames});");

                            if (!indexColumns.Contains(columnNames))
                            {
                                indexColumns.Add(columnNames);
                            }
                        }
                    }
                }
                #endregion

                //#region Default Value
                //if (options.GenerateDefaultValue)
                //{
                //    IEnumerable<TableColumn> defaultValueColumns = columns.Where(item => item.TableName == tableName && !string.IsNullOrEmpty(item.DefaultValue));
                //    foreach (TableColumn column in defaultValueColumns)
                //    {
                //        sb.AppendLine($"ALTER TABLE \"{tableName}\" MODIFY \"{column.ColumnName}\" DEFAULT {column.DefaultValue};");
                //    }
                //}
                //#endregion

                this.FeedbackInfo(OperationState.End, "table", table.Name);
            }
            #endregion

            #region View
            foreach (View view in schemaInfo.Views)
            {
                this.FeedbackInfo(OperationState.Begin, "view", view.Name);

                string viewName = view.Name;
                string quotedTableName = this.GetQuotedObjectName(view);

                sb.AppendLine();
                sb.Append(view.Definition);
                sb.Append(";");

                this.FeedbackInfo(OperationState.End, "view", view.Name);
            }
            #endregion

            if (Option.ScriptOutputMode.HasFlag(GenerateScriptOutputMode.WriteToFile))
            {
                this.AppendScriptsToFile(sb.ToString(), GenerateScriptMode.Schema, true);
            }

            return sb.ToString();
        }

        public override string TranslateColumn(Table table, TableColumn column)
        {
            bool isChar = column.DataType.ToLower().IndexOf("char") >= 0;
            string dataType = column.DataType;
            if (column.DataType.IndexOf("(") < 0)
            {
                if (isChar)
                {
                    long? dataLength = column.MaxLength;
                    if (dataLength > 0 && dataType.StartsWith("n"))
                    {
                        dataLength = dataLength / 2;
                    }

                    dataType = $"{dataType}({dataLength.ToString()})";
                }
                else if (!this.IsNoLengthDataType(dataType))
                {
                    dataType = $"{dataType}";
                    if (!(column.Precision == 0 && column.Scale == 0))
                    {
                        long precision = column.Precision.HasValue ? column.Precision.Value : column.MaxLength.Value;
                        int scale = column.Scale.HasValue ? column.Scale.Value : 0;

                        if (dataType == "raw")
                        {
                            dataType = $"{dataType}({precision})";
                        }
                        else
                        {
                            dataType = $"{dataType}({precision},{scale})";
                        }
                    }
                    else if (column.MaxLength > 0)
                    {
                        dataType += $"({column.MaxLength})";
                    }
                }
            }

            return $@"{ GetQuotedString(column.ColumnName)} {dataType} {(string.IsNullOrEmpty(column.DefaultValue) ? "" : " DEFAULT " + this.GetColumnDefaultValue(column))} {(column.IsRequired ? "NOT NULL" : "NULL")}";
        }

        private bool IsNoLengthDataType(string dataType)
        {
            string[] flags = { "date", "time", "int", "text", "clob", "blob", "binary_double" };

            return flags.Any(item => dataType.ToLower().Contains(item));
        }

        #endregion

        #region Generate Data Script
        public override long GetTableRecordCount(DbConnection connection, Table table)
        {
            string sql = $@"SELECT COUNT(1) FROM {this.ConnectionInfo.UserId}.{ GetQuotedString(table.Name)}";

            return base.GetTableRecordCount(connection, sql);
        }
        public override async Task<long> GetTableRecordCountAsync(DbConnection connection, Table table)
        {
            string sql = $@"SELECT COUNT(1) FROM {this.ConnectionInfo.UserId}.{ GetQuotedString(table.Name)}";

            return await base.GetTableRecordCountAsync(connection, sql);
        }
        public override string GenerateDataScripts(SchemaInfo schemaInfo)
        {
            return base.GenerateDataScripts(schemaInfo);
        }
        public override async Task<string> GenerateDataScriptsAsync(SchemaInfo schemaInfo)
        {
            return await base.GenerateDataScriptsAsync(schemaInfo);
        }
        protected override string GetBatchInsertPrefix()
        {
            return "INSERT ALL INTO";
        }

        protected override string GetBatchInsertItemBefore(string tableName, bool isFirstRow)
        {
            return isFirstRow ? "" : $"INTO {tableName} VALUES";
        }

        protected override string GetBatchInsertItemEnd(bool isAllEnd)
        {
            return (isAllEnd ? $"{Environment.NewLine}SELECT 1 FROM DUAL;" : "");
        }

        protected override string GetPagedSql(string tableName, string columnNames, string primaryKeyColumns, string whereClause, long pageNumber, int pageSize)
        {
            var startEndRowNumber = PaginationHelper.GetStartEndRowNumber(pageNumber, pageSize);

            string pagedSql = $@"with PagedRecords as
								(
									SELECT {columnNames}, ROW_NUMBER() OVER (ORDER BY (SELECT 0 FROM DUAL)) AS ROWNUMBER
									FROM {tableName}
                                    {whereClause}
								)
								SELECT *
								FROM PagedRecords
								WHERE ROWNUMBER BETWEEN {startEndRowNumber.StartRowNumber} AND {startEndRowNumber.EndRowNumber}";

            return pagedSql;
        }

        protected override string GetUnicodeInsertChar()
        {
            return "";
        }

        protected override bool NeedInsertParameter(object value)
        {
            if (value != null && value.GetType() == typeof(string))
            {
                string str = value.ToString();
                if (str.Length > 4000 || (str.Contains(SEMICOLON_FUNC) && str.Length > 2000))
                {
                    return true;
                }
            }
            return false;
        }
        #endregion
    }
}
