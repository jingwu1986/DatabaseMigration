﻿using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;
using Dapper;
using DatabaseInterpreter.Model;
using System.Globalization;
using DatabaseInterpreter.Utility;

namespace DatabaseInterpreter.Core
{
    public class MySqlInterpreter : DbInterpreter
    {
        #region Field & Property
        public override string UnicodeInsertChar => "";

        public override string CommandParameterChar { get { return "@"; } }
        public override char QuotationLeftChar { get { return '`'; } }
        public override char QuotationRightChar { get { return '`'; } }
        public override DatabaseType DatabaseType { get { return DatabaseType.MySql; } }
        public override bool SupportBulkCopy { get { return true; } }
        public override List<string> BuiltinDatabases => new List<string> { "sys", "mysql", "information_schema", "performance_schema" };

        public readonly string DbCharset = SettingManager.Setting.MySqlCharset;
        public readonly string DbCharsetCollation = SettingManager.Setting.MySqlCharsetCollation;
        private string loaderPath = "";
        #endregion

        #region Constructor
        public MySqlInterpreter(ConnectionInfo connectionInfo, DbInterpreterOption options) : base(connectionInfo, options)
        {
            this.dbConnector = this.GetDbConnector();
        }
        #endregion

        #region Common Method
        public override DbConnector GetDbConnector()
        {
            return new DbConnector(new MySqlProvider(), new MySqlConnectionBuilder(), this.ConnectionInfo);
        }       
        #endregion

        #region Schema Information
        #region Database
        public override Task<List<Database>> GetDatabasesAsync()
        {
            string notShowBuiltinDatabaseCondition = "";

            if (!this.ShowBuiltinDatabase)
            {
                string strBuiltinDatabase = this.BuiltinDatabases.Count > 0 ? string.Join(",", this.BuiltinDatabases.Select(item => $"'{item}'")) : "";
                notShowBuiltinDatabaseCondition = string.IsNullOrEmpty(strBuiltinDatabase) ? "" : $"WHERE SCHEMA_NAME NOT IN({strBuiltinDatabase})";
            }

            string sql = $"SELECT SCHEMA_NAME AS `Name` FROM INFORMATION_SCHEMA.`SCHEMATA` {notShowBuiltinDatabaseCondition}";

            return base.GetDbObjectsAsync<Database>(sql);
        }
        #endregion

        #region User Defined Type       

        public override Task<List<UserDefinedType>> GetUserDefinedTypesAsync(params string[] typeNames)
        {
            return base.GetDbObjectsAsync<UserDefinedType>("");
        }

        public override Task<List<UserDefinedType>> GetUserDefinedTypesAsync(DbConnection dbConnection, params string[] typeNames)
        {
            return base.GetDbObjectsAsync<UserDefinedType>(dbConnection, "");
        }
        #endregion

        #region Function  

        public override Task<List<Function>> GetFunctionsAsync(params string[] functionNames)
        {
            return base.GetDbObjectsAsync<Function>(this.GetSqlForRoutines("FUNCTION", functionNames));
        }

        public override Task<List<Function>> GetFunctionsAsync(DbConnection dbConnection, params string[] functionNames)
        {
            return base.GetDbObjectsAsync<Function>(dbConnection, this.GetSqlForRoutines("FUNCTION", functionNames));
        }

        private string GetSqlForRoutines(string type, params string[] objectNames)
        {
            bool isSimpleMode = this.IsObjectFectchSimpleMode();

            string sql = $@"SELECT ROUTINE_NAME AS `Name`, ROUTINE_SCHEMA AS `Owner`, 
                         {(isSimpleMode ? "''" : "ROUTINE_DEFINITION")} AS `Definition`
                        FROM INFORMATION_SCHEMA.`ROUTINES`
                        WHERE ROUTINE_TYPE = '{type}'  AND ROUTINE_SCHEMA = '{ConnectionInfo.Database}'
                        ";

            if (objectNames != null && objectNames.Any())
            {
                string strNames = StringHelper.GetSingleQuotedString(objectNames);
                sql += $" AND ROUTINE_NAME IN ({ strNames })";
            }

            sql += " ORDER BY ROUTINE_NAME";

            return sql;
        }
        #endregion

        #region Table
        public override Task<List<Table>> GetTablesAsync(params string[] tableNames)
        {
            return base.GetDbObjectsAsync<Table>(this.GetSqlForTables(tableNames));
        }

        public override Task<List<Table>> GetTablesAsync(DbConnection dbConnection, params string[] tableNames)
        {
            return base.GetDbObjectsAsync<Table>(dbConnection, this.GetSqlForTables(tableNames));
        }

        private string GetSqlForTables(params string[] tableNames)
        {
            bool isSimpleMode = this.IsObjectFectchSimpleMode();

            string sql = $@"SELECT TABLE_SCHEMA AS `Owner`, TABLE_NAME AS `Name` {(isSimpleMode ? "" : ", TABLE_COMMENT AS `Comment`, 1 AS `IdentitySeed`, 1 AS `IdentityIncrement`")}
                        FROM INFORMATION_SCHEMA.`TABLES`
                        WHERE TABLE_TYPE ='BASE TABLE' AND TABLE_SCHEMA ='{ConnectionInfo.Database}' 
                        ";

            if (tableNames != null && tableNames.Any())
            {
                string strTableNames = StringHelper.GetSingleQuotedString(tableNames);
                sql += $" AND TABLE_NAME IN ({ strTableNames })";
            }

            sql += " ORDER BY TABLE_NAME";

            return sql;
        }
        #endregion

        #region Table Column
        public override Task<List<TableColumn>> GetTableColumnsAsync(params string[] tableNames)
        {
            return base.GetDbObjectsAsync<TableColumn>(this.GetSqlForTableColumns(tableNames));
        }

        public override Task<List<TableColumn>> GetTableColumnsAsync(DbConnection dbConnection, params string[] tableNames)
        {
            return base.GetDbObjectsAsync<TableColumn>(dbConnection, this.GetSqlForTableColumns(tableNames));
        }

        private string GetSqlForTableColumns(params string[] tableNames)
        {
            string sql = $@"SELECT C.TABLE_SCHEMA AS `Owner`, C.TABLE_NAME AS `TableName`, COLUMN_NAME AS `Name`, COLUMN_TYPE AS `DataType`, 
                        CHARACTER_MAXIMUM_LENGTH AS `MaxLength`, CASE IS_NULLABLE WHEN 'YES' THEN 1 ELSE 0 END AS `IsNullable`,ORDINAL_POSITION AS `Order`,
                        NUMERIC_PRECISION AS `Precision`,NUMERIC_SCALE AS `Scale`, COLUMN_DEFAULT AS `DefaultValue`,COLUMN_COMMENT AS `Comment`,
                        CASE EXTRA WHEN 'auto_increment' THEN 1 ELSE 0 END AS `IsIdentity`,'' AS `TypeOwner`
                        FROM INFORMATION_SCHEMA.`COLUMNS` AS C
                        JOIN INFORMATION_SCHEMA.`TABLES` AS T ON T.`TABLE_NAME`= C.`TABLE_NAME` AND T.TABLE_TYPE='BASE TABLE' AND T.TABLE_SCHEMA=C.TABLE_SCHEMA
                        WHERE C.TABLE_SCHEMA ='{ConnectionInfo.Database}'";

            if (tableNames != null && tableNames.Count() > 0)
            {
                string strTableNames = StringHelper.GetSingleQuotedString(tableNames);
                sql += $" AND C.TABLE_NAME IN ({ strTableNames })";
            }

            return sql;
        }
        #endregion

        #region Table Primary Key
        public override Task<List<TablePrimaryKey>> GetTablePrimaryKeysAsync(params string[] tableNames)
        {
            return base.GetDbObjectsAsync<TablePrimaryKey>(this.GetSqlForTablePrimaryKeys(tableNames));
        }

        public override Task<List<TablePrimaryKey>> GetTablePrimaryKeysAsync(DbConnection dbConnection, params string[] tableNames)
        {
            return base.GetDbObjectsAsync<TablePrimaryKey>(dbConnection, this.GetSqlForTablePrimaryKeys(tableNames));
        }

        private string GetSqlForTablePrimaryKeys(params string[] tableNames)
        {
            string sql = $@"SELECT C.`CONSTRAINT_SCHEMA` AS `Owner`, K.TABLE_NAME AS `TableName`, K.CONSTRAINT_NAME AS `Name`, 
                            K.COLUMN_NAME AS `ColumnName`, K.`ORDINAL_POSITION` AS `Order`, 0 AS `IsDesc`
                        FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS AS C
                        JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE AS K ON C.TABLE_NAME = K.TABLE_NAME AND C.CONSTRAINT_CATALOG = K.CONSTRAINT_CATALOG AND C.CONSTRAINT_SCHEMA = K.CONSTRAINT_SCHEMA AND C.CONSTRAINT_NAME = K.CONSTRAINT_NAME
                        WHERE C.CONSTRAINT_TYPE = 'PRIMARY KEY'
                        AND C.`CONSTRAINT_SCHEMA` ='{ConnectionInfo.Database}'";

            if (tableNames != null && tableNames.Count() > 0)
            {
                string strTableNames = StringHelper.GetSingleQuotedString(tableNames);
                sql += $" AND C.TABLE_NAME IN ({ strTableNames })";
            }

            return sql;
        }
        #endregion

        #region Table Foreign Key
        public override Task<List<TableForeignKey>> GetTableForeignKeysAsync(params string[] tableNames)
        {
            return base.GetDbObjectsAsync<TableForeignKey>(this.GetSqlForTableForeignKeys(tableNames));
        }

        public override Task<List<TableForeignKey>> GetTableForeignKeysAsync(DbConnection dbConnection, params string[] tableNames)
        {
            return base.GetDbObjectsAsync<TableForeignKey>(dbConnection, this.GetSqlForTableForeignKeys(tableNames));
        }

        private string GetSqlForTableForeignKeys(params string[] tableNames)
        {
            string sql = $@"SELECT C.`CONSTRAINT_SCHEMA` AS `Owner`, K.TABLE_NAME AS `TableName`, K.CONSTRAINT_NAME AS `Name`, 
                        K.COLUMN_NAME AS `ColumnName`, K.`REFERENCED_TABLE_NAME` AS `ReferencedTableName`,K.`REFERENCED_COLUMN_NAME` AS `ReferencedColumnName`,
                        CASE RC.UPDATE_RULE WHEN 'CASCADE' THEN 1 ELSE 0 END AS `UpdateCascade`, 
                        CASE RC.`DELETE_RULE` WHEN 'CASCADE' THEN 1 ELSE 0 END AS `DeleteCascade`
                        FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS AS C
                        JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE AS K ON C.TABLE_NAME = K.TABLE_NAME AND C.CONSTRAINT_CATALOG = K.CONSTRAINT_CATALOG AND C.CONSTRAINT_SCHEMA = K.CONSTRAINT_SCHEMA AND C.CONSTRAINT_NAME = K.CONSTRAINT_NAME
                        JOIN INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS RC ON RC.CONSTRAINT_SCHEMA=C.CONSTRAINT_SCHEMA AND RC.CONSTRAINT_NAME=C.CONSTRAINT_NAME AND C.TABLE_NAME=RC.TABLE_NAME                        
                        WHERE C.CONSTRAINT_TYPE = 'FOREIGN KEY'
                        AND C.`CONSTRAINT_SCHEMA` ='{ConnectionInfo.Database}'";

            if (tableNames != null && tableNames.Count() > 0)
            {
                string strTableNames = StringHelper.GetSingleQuotedString(tableNames);
                sql += $" AND C.TABLE_NAME IN ({ strTableNames })";
            }

            return sql;
        }
        #endregion

        #region Table Index
        public override Task<List<TableIndex>> GetTableIndexesAsync(params string[] tableNames)
        {
            return base.GetDbObjectsAsync<TableIndex>(this.GetSqlForTableIndexes(tableNames));
        }

        public override Task<List<TableIndex>> GetTableIndexesAsync(DbConnection dbConnection, params string[] tableNames)
        {
            return base.GetDbObjectsAsync<TableIndex>(dbConnection, this.GetSqlForTableIndexes(tableNames));
        }

        private string GetSqlForTableIndexes(params string[] tableNames)
        {
            string sql = $@"SELECT TABLE_SCHEMA AS `Owner`,
	                        TABLE_NAME AS `TableName`,
	                        INDEX_NAME AS `Name`,
	                        COLUMN_NAME AS `ColumnName`,
	                        CASE  NON_UNIQUE WHEN 1 THEN 0 ELSE 1 END AS `IsUnique`,
	                        SEQ_IN_INDEX  AS `Order`,
	                        0 AS `IsDesc`
	                        FROM INFORMATION_SCHEMA.STATISTICS 
	                        WHERE INDEX_NAME NOT IN('PRIMARY', 'FOREIGN')
	                        AND TABLE_SCHEMA = '{ConnectionInfo.Database}'";

            if (tableNames != null && tableNames.Any())
            {
                string strTableNames = StringHelper.GetSingleQuotedString(tableNames);
                sql += $" AND TABLE_NAME IN ({ strTableNames })";
            }

            return sql;
        }
        #endregion

        #region View   
        public override Task<List<View>> GetViewsAsync(params string[] viewNames)
        {
            return base.GetDbObjectsAsync<View>(this.GetSqlForViews(viewNames));
        }

        public override Task<List<View>> GetViewsAsync(DbConnection dbConnection, params string[] viewNames)
        {
            return base.GetDbObjectsAsync<View>(dbConnection, this.GetSqlForViews(viewNames));
        }

        private string GetSqlForViews(params string[] viewNames)
        {
            bool isSimpleMode = this.IsObjectFectchSimpleMode();

            string sql = $@"SELECT TABLE_SCHEMA AS `Owner`,TABLE_NAME AS `Name`, {(isSimpleMode ? "''" : "VIEW_DEFINITION")} AS `Definition` 
                        FROM INFORMATION_SCHEMA.`VIEWS`
                        WHERE TABLE_SCHEMA = '{ConnectionInfo.Database}'";

            if (viewNames != null && viewNames.Any())
            {
                string strNames = StringHelper.GetSingleQuotedString(viewNames);
                sql += $" AND TABLE_NAME IN ({ strNames })";
            }

            sql += " ORDER BY TABLE_NAME";

            return sql;
        }

        #endregion

        #region Trigger  
        public override Task<List<Trigger>> GetTriggersAsync(params string[] triggerNames)
        {
            return base.GetDbObjectsAsync<Trigger>(this.GetSqlForTriggers(triggerNames));
        }

        public override Task<List<Trigger>> GetTriggersAsync(DbConnection dbConnection, params string[] triggerNames)
        {
            return base.GetDbObjectsAsync<Trigger>(dbConnection, this.GetSqlForTriggers(triggerNames));
        }

        private string GetSqlForTriggers(params string[] triggerNames)
        {
            bool isSimpleMode = this.IsObjectFectchSimpleMode();

            string sql = $@"SELECT TRIGGER_NAME AS `Name`, TRIGGER_SCHEMA AS `Owner`, EVENT_OBJECT_TABLE AS `TableName`, 
                         {(isSimpleMode ? "''" : "ACTION_STATEMENT")} AS `Definition`
                        FROM INFORMATION_SCHEMA.`TRIGGERS`
                        WHERE TRIGGER_SCHEMA = '{ConnectionInfo.Database}'
                        ";

            if (triggerNames != null && triggerNames.Any())
            {
                string strNames = StringHelper.GetSingleQuotedString(triggerNames);
                sql += $" AND TRIGGER_NAME IN ({ strNames })";
            }

            sql += " ORDER BY TRIGGER_NAME";

            return sql;
        }
        #endregion

        #region Procedure    
        public override Task<List<Procedure>> GetProceduresAsync(params string[] procedureNames)
        {
            return base.GetDbObjectsAsync<Procedure>(this.GetSqlForRoutines("PROCEDURE", procedureNames));
        }

        public override Task<List<Procedure>> GetProceduresAsync(DbConnection dbConnection, params string[] procedureNames)
        {
            return base.GetDbObjectsAsync<Procedure>(dbConnection, this.GetSqlForRoutines("PROCEDURE", procedureNames));
        }      
        #endregion
        #endregion

        #region Datbase Operation
        public override async Task SetIdentityEnabled(DbConnection dbConnection, TableColumn column, bool enabled)
        {
            Table table = new Table() { Name = column.TableName, Owner = column.Owner };
            await this.ExecuteNonQueryAsync(dbConnection, $"ALTER TABLE {GetQuotedObjectName(table)} MODIFY COLUMN {ParseColumn(table, column)} {(enabled ? "AUTO_INCREMENT" : "")}");
        }       

        public override async Task<int> BulkCopyAsync(DbConnection connection, DataTable dataTable, string destinationTableName = null, int? bulkCopyTimeout = null, int? batchSize = null)
        {
            if (dataTable == null || dataTable.Rows.Count <= 0)
            {
                return 0;
            }

            var loader = this.GetBulkLoader(connection, dataTable, destinationTableName, bulkCopyTimeout);

            if (loader == null)
            {
                return 0;
            }

            return await this.BulkLoadAsync(loader, dataTable);
        }

        private class NullDateTimeConverter : DateTimeConverter
        {
            public override string ConvertToString(object value, IWriterRow row, MemberMapData memberMapData)
            {
                if (value == null)
                {
                    return "NULL";
                }

                return base.ConvertToString(value, row, memberMapData);
            }
        }

        private async Task<int> BulkLoadAsync(MySqlBulkLoader loader, DataTable dataTable)
        {
            if (loader == null)
            {
                return 0;
            }

            string path = loader.FileName;

            if (string.IsNullOrEmpty(path))
            {
                path = Path.GetTempFileName();
                loader.FileName = path;
            }

            try
            {
                using (var writer = new StreamWriter(path))
                {
                    var configuration = new CsvConfiguration(CultureInfo.CurrentCulture)
                    {
                        HasHeaderRecord = false
                    };
                    configuration.TypeConverterCache.AddConverter<DateTime?>(new NullDateTimeConverter());
                    using (var csv = new CsvWriter(writer, configuration))
                    {
                        using (var dt = dataTable.Copy())
                        {
                            foreach (DataColumn column in dt.Columns)
                            {
                                var columnName = column.ColumnName;
                                if (columnName != RowNumberColumnName)
                                {
                                    loader.Columns.Add(columnName);
                                }
                            }

                            foreach (DataRow row in dt.Rows)
                            {
                                for (var i = 0; i < dt.Columns.Count; i++)
                                {
                                    var column = dt.Columns[i];
                                    var columnName = column.ColumnName;
                                    if (columnName != RowNumberColumnName)
                                    {
                                        csv.WriteField(row[i]);
                                    }
                                }
                                csv.NextRecord();
                            }
                        }
                    }
                }

                return await loader.LoadAsync();
            }
            catch (Exception e)
            {
                throw e;
            }
            finally
            {
                File.Delete(path);
            }
        }

        private MySqlBulkLoader GetBulkLoader(DbConnection connection, DataTable dataTable, string destinationTableName = null, int? bulkCopyTimeout = null)
        {
            if (dataTable == null || dataTable.Rows.Count <= 0)
            {
                return null;
            }

            if (!(connection is MySqlConnection conn))
            {
                return null;
            }

            var loader = new MySqlBulkLoader(conn)
            {
                LineTerminator = Environment.NewLine,
                FieldQuotationCharacter = '"',
                EscapeCharacter = '"',
                FieldTerminator = ",",
                ConflictOption = MySqlBulkLoaderConflictOption.Ignore,
                TableName = this.GetQuotedString(destinationTableName),
                CharacterSet = "utf8"
            };

            string fileName = Path.GetFileName(Path.GetTempFileName());

            if (string.IsNullOrEmpty(this.loaderPath))
            {
                string path = conn.Query<string>(@"select @@global.secure_file_priv;").FirstOrDefault() ?? "";

                if (Directory.Exists(path))
                {
                    this.loaderPath = path;
                }
            }

            if (Directory.Exists(this.loaderPath))
            {
                loader.FileName = Path.Combine(this.loaderPath, fileName);
            }

            if (bulkCopyTimeout.HasValue)
            {
                loader.Timeout = bulkCopyTimeout.Value;
            }

            return loader;
        }

        #endregion

        #region Generate Schema Scripts 

        public override string GenerateSchemaScripts(SchemaInfo schemaInfo)
        {
            StringBuilder sb = new StringBuilder();

            #region Create Table
            foreach (Table table in schemaInfo.Tables)
            {
                this.FeedbackInfo(OperationState.Begin, "table", table.Name);

                string tableName = table.Name;
                string quotedTableName = this.GetQuotedObjectName(table);

                IEnumerable<TableColumn> tableColumns = schemaInfo.TableColumns.Where(item => item.TableName == tableName).OrderBy(item => item.Order);

                string primaryKey = "";

                IEnumerable<TablePrimaryKey> primaryKeys = schemaInfo.TablePrimaryKeys.Where(item => item.TableName == tableName);

                #region Primary Key
                if (this.Option.GenerateKey && primaryKeys.Count() > 0)
                {
                    //string primaryKeyName = primaryKeys.First().KeyName;
                    //if(primaryKeyName=="PRIMARY")
                    //{
                    //    primaryKeyName = "PK_" + tableName ;
                    //}
                    primaryKey =
$@"
,PRIMARY KEY
(
{string.Join(Environment.NewLine, primaryKeys.Select(item => $"{ this.GetQuotedString(item.ColumnName)},")).TrimEnd(',')}
)";
                }
                #endregion

                List<string> foreignKeysLines = new List<string>();
                #region Foreign Key
                if (this.Option.GenerateKey)
                {
                    IEnumerable<TableForeignKey> foreignKeys = schemaInfo.TableForeignKeys.Where(item => item.TableName == tableName);
                    if (foreignKeys.Count() > 0)
                    {
                        ILookup<string, TableForeignKey> foreignKeyLookup = foreignKeys.ToLookup(item => item.Name);

                        IEnumerable<string> keyNames = foreignKeyLookup.Select(item => item.Key);

                        foreach (string keyName in keyNames)
                        {
                            TableForeignKey tableForeignKey = foreignKeyLookup[keyName].First();

                            string columnNames = string.Join(",", foreignKeyLookup[keyName].Select(item => this.GetQuotedString(item.ColumnName)));
                            string referenceColumnName = string.Join(",", foreignKeyLookup[keyName].Select(item => $"{ this.GetQuotedString(item.ReferencedColumnName)}"));

                            string line = $"CONSTRAINT { this.GetQuotedString(keyName)} FOREIGN KEY ({columnNames}) REFERENCES { this.GetQuotedString(tableForeignKey.ReferencedTableName)}({referenceColumnName})";

                            if (tableForeignKey.UpdateCascade)
                            {
                                line += " ON UPDATE CASCADE";
                            }
                            else
                            {
                                line += " ON UPDATE NO ACTION";
                            }

                            if (tableForeignKey.DeleteCascade)
                            {
                                line += " ON DELETE CASCADE";
                            }
                            else
                            {
                                line += " ON DELETE NO ACTION";
                            }

                            foreignKeysLines.Add(line);
                        }
                    }
                }

                string foreignKey = $"{(foreignKeysLines.Count > 0 ? (Environment.NewLine + "," + string.Join("," + Environment.NewLine, foreignKeysLines)) : "")}";
    
                #endregion

                #region Table
                sb.AppendLine(
$@"
CREATE TABLE {quotedTableName}(
{string.Join("," + Environment.NewLine, tableColumns.Select(item => this.ParseColumn(table, item)))}{primaryKey}{foreignKey}
){(!string.IsNullOrEmpty(table.Comment) ? ($"comment='{ValueHelper.TransferSingleQuotation(table.Comment)}'") : "")}
DEFAULT CHARSET={DbCharset}" + this.ScriptsSplitString);
                #endregion               

                #region Index
                if (this.Option.GenerateIndex)
                {
                    IEnumerable<TableIndex> indices = schemaInfo.TableIndexes.Where(item => item.TableName == tableName).OrderBy(item => item.Order);
                    if (indices.Count() > 0)
                    {
                        sb.AppendLine();

                        List<string> indexColumns = new List<string>();

                        ILookup<string, TableIndex> indexLookup = indices.ToLookup(item => item.Name);
                        IEnumerable<string> indexNames = indexLookup.Select(item => item.Key);
                        foreach (string indexName in indexNames)
                        {
                            TableIndex tableIndex = indexLookup[indexName].First();

                            string columnNames = string.Join(",", indexLookup[indexName].Select(item => $"{this.GetQuotedString(item.ColumnName)}"));

                            if (indexColumns.Contains(columnNames))
                            {
                                continue;
                            }

                            var tempIndexName = tableIndex.Name;
                            if (tempIndexName.Contains("-"))
                            {
                                tempIndexName = tempIndexName.Replace("-", "_");
                            }

                            sb.AppendLine($"ALTER TABLE {quotedTableName} ADD {(tableIndex.IsUnique ? "UNIQUE" : "")} INDEX {tempIndexName} ({columnNames});");

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
                //    IEnumerable<TableColumn> defaultValueColumns = columns.Where(item => item.Owner== table.Owner && item.TableName == tableName && !string.IsNullOrEmpty(item.DefaultValue));
                //    foreach (TableColumn column in defaultValueColumns)
                //    {
                //        sb.AppendLine($"ALTER TABLE {quotedTableName} ALTER COLUMN {this.GetQuotedString(column.ColumnName)} SET DEFAULT {column.DefaultValue};");
                //    }
                //}
                //#endregion

                this.FeedbackInfo(OperationState.End, "table", table.Name);
            }
            #endregion

            if (sb.Length > 0)
            {
                sb.AppendLine();
            }

            #region View
            foreach (View view in schemaInfo.Views)
            {
                this.FeedbackInfo(OperationState.Begin, "view", view.Name);

                string viewName = view.Name;
                string quotedTableName = this.GetQuotedObjectName(view);

                sb.AppendLine(view.Definition + this.ScriptsSplitString);

                sb.AppendLine();

                this.FeedbackInfo(OperationState.End, "view", view.Name);
            }
            #endregion

            if (this.Option.ScriptOutputMode.HasFlag(GenerateScriptOutputMode.WriteToFile))
            {
                this.AppendScriptsToFile(sb.ToString(), GenerateScriptMode.Schema, true);
            }

            return sb.ToString();
        }

        public override string ParseColumn(Table table, TableColumn column)
        {
            string dataType = column.DataType;
            bool isChar = DataTypeHelper.IsCharType(dataType.ToLower());

            if (column.DataType.IndexOf("(") < 0)
            {
                if (isChar)
                {
                    dataType = $"{dataType}({column.MaxLength.ToString()})";
                }
                else if (!this.IsNoLengthDataType(dataType))
                {
                    long precision = column.Precision.HasValue ? column.Precision.Value : column.MaxLength.Value;
                    int scale = column.Scale.HasValue ? column.Scale.Value : 0;

                    dataType = $"{dataType}({precision},{scale})";
                }

                if (isChar || DataTypeHelper.IsTextType(dataType.ToLower()))
                {
                    dataType += $" CHARACTER SET {DbCharset} COLLATE {DbCharsetCollation} ";
                }
            }

            return $@"{this.GetQuotedString(column.Name)} {dataType} {(column.IsRequired ? "NOT NULL" : "NULL")} {(this.Option.GenerateIdentity && column.IsIdentity ? $"AUTO_INCREMENT" : "")} {(string.IsNullOrEmpty(column.DefaultValue) ? "" : " DEFAULT " + this.GetColumnDefaultValue(column))} {(!string.IsNullOrEmpty(column.Comment) ? $"comment '{ValueHelper.TransferSingleQuotation(column.Comment)}'" : "")}";
        }

        private bool IsNoLengthDataType(string dataType)
        {
            string[] flags = { "date", "time", "int", "text", "longblob", "longtext" };

            return flags.Any(item => dataType.ToLower().Contains(item));
        }

        #endregion

        #region Generate Data Script
        public override long GetTableRecordCount(DbConnection connection, Table table)
        {
            string sql = $"SELECT COUNT(1) FROM {this.GetQuotedObjectName(table)}";

            return base.GetTableRecordCount(connection, sql);
        }

        public override Task<long> GetTableRecordCountAsync(DbConnection connection, Table table)
        {
            string sql = $"SELECT COUNT(1) FROM {this.GetQuotedObjectName(table)}";

            return base.GetTableRecordCountAsync(connection, sql);
        }

        public override Task<string> GenerateDataScriptsAsync(SchemaInfo schemaInfo)
        {
            return base.GenerateDataScriptsAsync(schemaInfo);
        }

        protected override string GetPagedSql(string tableName, string columnNames, string primaryKeyColumns, string whereClause, long pageNumber, int pageSize)
        {
            var startEndRowNumber = PaginationHelper.GetStartEndRowNumber(pageNumber, pageSize);

            var pagedSql = $@"SELECT {columnNames}
							  FROM {tableName}
                             {whereClause} 
                             ORDER BY {(!string.IsNullOrEmpty(primaryKeyColumns) ? primaryKeyColumns : "1")}
                             LIMIT { startEndRowNumber.StartRowNumber - 1 } , {pageSize}";

            return pagedSql;
        }
        #endregion       
    }
}
