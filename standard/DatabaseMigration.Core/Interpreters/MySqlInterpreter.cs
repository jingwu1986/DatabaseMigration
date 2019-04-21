using MySql.Data.MySqlClient;
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

namespace DatabaseMigration.Core
{
    public class MySqlInterpreter : DbInterpreter
    {
        #region Property
        public override string CommandParameterChar { get { return "@"; } }
        public override char QuotationLeftChar { get { return '`'; } }
        public override char QuotationRightChar { get { return '`'; } }
        public override DatabaseType DatabaseType { get { return DatabaseType.MySql; } }
        public override bool SupportBulkCopy { get { return true; } }

        public readonly string DbCharset = SettingManager.Setting.MySqlCharset;
        public readonly string DbCharsetCollation = SettingManager.Setting.MySqlCharsetCollation;
        private string loaderPath = "";
        #endregion

        #region Constructor
        public MySqlInterpreter(ConnectionInfo connectionInfo, GenerateScriptOption options) : base(connectionInfo, options) { }
        #endregion

        #region Common Method
        public override DbConnector GetDbConnector()
        {
            return new DbConnector(new MySqlProvider(), new MySqlConnectionBuilder(), this.ConnectionInfo);
        }

        protected override IEnumerable<DbParameter> BuildCommandParameters(Dictionary<string, object> paramaters)
        {
            foreach (KeyValuePair<string, object> kp in paramaters)
            {
                yield return new MySqlParameter(kp.Key, kp.Value);
            }
        }

        public override async Task<int> BulkCopyAsync(
            DbConnection connection,
            DataTable dataTable,
            string destinationTableName = null,
            int? bulkCopyTimeout = null,
            int? batchSize = null)
        {
            if (dataTable == null || dataTable.Rows.Count <= 0)
            {
                return 0;
            }

            var loader = GetMySqlBulkLoader(
                connection,
                dataTable,
                destinationTableName,
                bulkCopyTimeout);

            if (loader == null)
            {
                return 0;
            }

            return await LoaderAsync(loader, dataTable);
        }

        public override int BulkCopy(
            DbConnection connection,
            DataTable dataTable,
            string destinationTableName = null,
            int? bulkCopyTimeout = null,
            int? batchSize = null)
        {
            if (dataTable == null || dataTable.Rows.Count <= 0)
            {
                return 0;
            }

            var loader = GetMySqlBulkLoader(
                connection,
                dataTable,
                destinationTableName,
                bulkCopyTimeout);

            if (loader == null)
            {
                return 0;
            }

            return Loader(loader, dataTable);
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
        private int Loader(MySqlBulkLoader loader, DataTable dataTable)
        {
            return this.InternalLoader(loader, dataTable, false).Result;
        }

        private Task<int> LoaderAsync(MySqlBulkLoader loader, DataTable dataTable)
        {
            return this.InternalLoader(loader, dataTable, true);
        }

        private async Task<int> InternalLoader(MySqlBulkLoader loader, DataTable dataTable, bool async=false)
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
                    var configuration = new Configuration
                    {
                        HasHeaderRecord = false,
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

                return async? await loader.LoadAsync(): loader.Load();
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

        private MySqlBulkLoader GetMySqlBulkLoader(
            DbConnection connection,
            DataTable dataTable,
            string destinationTableName = null,
            int? bulkCopyTimeout = null)
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
                TableName = this.GetQuotedString(destinationTableName)               
            };

            if(string.IsNullOrEmpty(this.loaderPath))
            {
                this.loaderPath = conn.Query<string>(@"select @@global.secure_file_priv;").FirstOrDefault() ?? "";
            }           

            loader.FileName = Path.Combine(this.loaderPath, Path.GetFileName(Path.GetTempFileName()));

            if (bulkCopyTimeout.HasValue)
            {
                loader.Timeout = bulkCopyTimeout.Value;
            }

            return loader;
        }
        #endregion

        #region Database
        public override List<Database> GetDatabases()
        {
            DbConnector dbConnector = this.GetDbConnector();

            string sql = $@"SELECT SCHEMA_NAME AS `Name` FROM INFORMATION_SCHEMA.`SCHEMATA`";

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
            string sql = $@"SELECT TABLE_SCHEMA AS `Owner`, TABLE_NAME AS `Name`, TABLE_COMMENT AS `Comment`,
                        1 AS `IdentitySeed`, 1 AS `IdentityIncrement`
                        FROM INFORMATION_SCHEMA.`TABLES`
                        WHERE TABLE_SCHEMA ='{ConnectionInfo.Database}' 
                        ";

            if (tableNames != null && tableNames.Any())
            {
                string strTableNames = StringHelper.GetSingleQuotedString(tableNames);
                sql += $" AND TABLE_NAME IN ({ strTableNames })";
            }

            sql += " ORDER BY TABLE_NAME";

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

            string sql = $@"SELECT TABLE_SCHEMA AS `Owner`, TABLE_NAME AS TableName, COLUMN_NAME AS ColumnName, COLUMN_TYPE AS DataType, 
                        CHARACTER_MAXIMUM_LENGTH AS MaxLength, CASE IS_NULLABLE WHEN 'YES' THEN 1 ELSE 0 END AS IsNullable,ORDINAL_POSITION AS `Order`,
                        NUMERIC_PRECISION AS `Precision`,NUMERIC_SCALE AS `Scale`, COLUMN_DEFAULT AS `DefaultValue`,COLUMN_COMMENT AS `Comment`,
                        CASE EXTRA WHEN 'auto_increment' THEN 1 ELSE 0 END AS `IsIdentity`,'' AS `TypeOwner`
                        FROM INFORMATION_SCHEMA.`COLUMNS`
                        WHERE TABLE_SCHEMA ='{ConnectionInfo.Database}'";

            if (tableNames != null && tableNames.Count() > 0)
            {
                string strTableNames = StringHelper.GetSingleQuotedString(tableNames);
                sql += $" AND TABLE_NAME IN ({ strTableNames })";
            }

            return base.GetTableColumns(dbConnector, sql);
        }
        #endregion

        #region Table Primary Key
        public override List<TablePrimaryKey> GetTablePrimaryKeys(params string[] tableNames)
        {
            DbConnector dbConnector = this.GetDbConnector();

            string sql = $@"SELECT C.`CONSTRAINT_SCHEMA` AS `Owner`, K.TABLE_NAME AS TableName, K.CONSTRAINT_NAME AS KeyName, K.COLUMN_NAME AS ColumnName, K.`ORDINAL_POSITION` AS `Order`, 0 AS IsDesc
                        FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS AS C
                        JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE AS K ON C.TABLE_NAME = K.TABLE_NAME AND C.CONSTRAINT_CATALOG = K.CONSTRAINT_CATALOG AND C.CONSTRAINT_SCHEMA = K.CONSTRAINT_SCHEMA AND C.CONSTRAINT_NAME = K.CONSTRAINT_NAME
                        WHERE C.CONSTRAINT_TYPE = 'PRIMARY KEY'
                        AND C.`CONSTRAINT_SCHEMA` ='{ConnectionInfo.Database}'";

            if (tableNames != null && tableNames.Count() > 0)
            {
                string strTableNames = StringHelper.GetSingleQuotedString(tableNames);
                sql += $" AND C.TABLE_NAME IN ({ strTableNames })";
            }

            return base.GetTablePrimaryKeys(dbConnector, sql);
        }
        #endregion

        #region Table Foreign Key
        public override List<TableForeignKey> GetTableForeignKeys(params string[] tableNames)
        {
            DbConnector dbConnector = this.GetDbConnector();

            string sql = $@"SELECT C.`CONSTRAINT_SCHEMA` AS `Owner`, K.TABLE_NAME AS TableName, K.CONSTRAINT_NAME AS KeyName, K.COLUMN_NAME AS ColumnName, K.`REFERENCED_TABLE_NAME` AS ReferencedTableName,K.`REFERENCED_COLUMN_NAME` AS ReferencedColumnName,
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

            return base.GetTableForeignKeys(dbConnector, sql);
        }
        #endregion

        #region Table Index
        public override List<TableIndex> GetTableIndexes(params string[] tableNames)
        {
            DbConnector dbConnector = this.GetDbConnector();

            string sql = $@"SELECT TABLE_SCHEMA AS `Owner`,
	                        TABLE_NAME AS TableName,
	                        INDEX_NAME AS IndexName,
	                        COLUMN_NAME AS ColumnName,
	                        CASE  NON_UNIQUE WHEN 1 THEN 0 ELSE 1 END AS IsUnique,
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

            return base.GetTableIndexes(dbConnector, sql);
        }
        #endregion

        #region Identity
        public override void SetIdentityEnabled(DbConnection dbConnection, TableColumn column, bool enabled)
        {
            Table table = new Table() { Name = column.TableName, Owner = column.Owner };
            this.ExecuteNonQuery(dbConnection, $"ALTER TABLE {GetQuotedTableName(table)} MODIFY COLUMN {TranslateColumn(table, column)} {(enabled ? "AUTO_INCREMENT" : "")}");
        }
        #endregion

        #region Generate Schema Scripts 

        public override string GenerateSchemaScripts(SchemaInfo schemaInfo)
        {
            StringBuilder sb = new StringBuilder();

            #region Create Table
            foreach (Table table in schemaInfo.Tables)
            {
                string tableName = table.Name;
                string quotedTableName = this.GetQuotedTableName(table);

                IEnumerable<TableColumn> tableColumns = schemaInfo.Columns.Where(item => item.TableName == tableName).OrderBy(item => item.Order);

                string primaryKey = "";

                IEnumerable<TablePrimaryKey> primaryKeys = schemaInfo.TablePrimaryKeys.Where(item => item.TableName == tableName);

                #region Primary Key
                if (Option.GenerateKey && primaryKeys.Count() > 0)
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
{string.Join(Environment.NewLine, primaryKeys.Select(item => $"{GetQuotedString(item.ColumnName)},")).TrimEnd(',')}
)";
                }
                #endregion

                List<string> foreignKeysLines = new List<string>();
                #region Foreign Key
                if (Option.GenerateKey)
                {
                    IEnumerable<TableForeignKey> foreignKeys = schemaInfo.TableForeignKeys.Where(item => item.TableName == tableName);
                    if (foreignKeys.Count() > 0)
                    {
                        ILookup<string, TableForeignKey> foreignKeyLookup = foreignKeys.ToLookup(item => item.KeyName);

                        IEnumerable<string> keyNames = foreignKeyLookup.Select(item => item.Key);

                        foreach (string keyName in keyNames)
                        {
                            TableForeignKey tableForeignKey = foreignKeyLookup[keyName].First();

                            string columnNames = string.Join(",", foreignKeyLookup[keyName].Select(item => GetQuotedString(item.ColumnName)));
                            string referenceColumnName = string.Join(",", foreignKeyLookup[keyName].Select(item => $"{GetQuotedString(item.ReferencedColumnName)}"));

                            string line = $"CONSTRAINT {GetQuotedString(keyName)} FOREIGN KEY ({columnNames}) REFERENCES {GetQuotedString(tableForeignKey.ReferencedTableName)}({referenceColumnName})";

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
                #endregion

                #region Create Table
                sb.Append(
$@"
CREATE TABLE {quotedTableName}(
{string.Join("," + Environment.NewLine, tableColumns.Select(item => this.TranslateColumn(table, item)))}{primaryKey}
{(foreignKeysLines.Count > 0 ? ("," + string.Join("," + Environment.NewLine, foreignKeysLines)) : "")}
){(!string.IsNullOrEmpty(table.Comment) ? ($"comment='{ValueHelper.TransferSingleQuotation(table.Comment)}'") : "")}
DEFAULT CHARSET={DbCharset};");
                #endregion

                sb.AppendLine();

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

                            var tempIndexName = tableIndex.IndexName;
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
                //        sb.AppendLine($"ALTER TABLE {quotedTableName} ALTER COLUMN {GetQuotedString(column.ColumnName)} SET DEFAULT {column.DefaultValue};");
                //    }
                //}
                //#endregion
            }
            #endregion

            return sb.ToString();
        }

        public override string TranslateColumn(Table table, TableColumn column)
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

            return $@"{GetQuotedString(column.ColumnName)} {dataType} {(column.IsRequired ? "NOT NULL" : "NULL")} {(this.Option.GenerateIdentity && column.IsIdentity ? $"AUTO_INCREMENT" : "")} {(string.IsNullOrEmpty(column.DefaultValue) ? "" : " DEFAULT " + column.DefaultValue)} {(!string.IsNullOrEmpty(column.Comment) ? $"comment '{ValueHelper.TransferSingleQuotation(column.Comment)}'" : "")}";
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
            string sql = $"SELECT COUNT(1) FROM {this.GetQuotedTableName(table)}";

            return base.GetTableRecordCount(connection, sql);
        }
        public override async Task<long> GetTableRecordCountAsync(DbConnection connection, Table table)
        {
            string sql = $"SELECT COUNT(1) FROM {this.GetQuotedTableName(table)}";

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
