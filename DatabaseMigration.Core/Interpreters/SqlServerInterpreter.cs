using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseMigration.Core
{
    public class SqlServerInterpreter : DbInterpreter
    {
        #region Property
        public override string CommandParameterChar { get { return "@"; } }
        public override char QuotationLeftChar { get { return '['; } }
        public override char QuotationRightChar { get { return ']'; } }
        public override DatabaseType DatabaseType { get { return DatabaseType.SqlServer;  } }
        #endregion

        #region Constructor
        public SqlServerInterpreter(ConnectionInfo connectionInfo, GenerateScriptOption options) : base(connectionInfo, options) { }
        #endregion

        #region Common Method
        public override DbConnector GetDbConnector()
        {
            return new DbConnector(new SqlServerProvider(), new SqlServerConnectionBuilder(), this.ConnectionInfo);
        }

        protected override IEnumerable<DbParameter> BuildCommandParameters(Dictionary<string, object> paramaters)
        {
            foreach (KeyValuePair<string, object> kp in paramaters)
            {
                yield return new SqlParameter(kp.Key, kp.Value);
            }
        }
        #endregion       

        #region Database
        public override List<Database> GetDatabases()
        {
            DbConnector dbConnector = this.GetDbConnector();

            string sql = $@"SELECT name AS Name FROM sys.databases WHERE owner_sid != 0x01 ORDER BY database_id";  

            return base.GetDatabases(dbConnector, sql);
        }
        #endregion 
        
        #region User Defined Type
        public override List<UserDefinedType> GetUserDefinedTypes(params string[] typeNames)
        {
            DbConnector dbConnector = this.GetDbConnector();

            string sql = @"SELECT schema_name(T.schema_id) AS [Owner],T.name as Name, ST.name AS Type, T.max_length AS MaxLength, T.precision AS Precision,T.scale AS Scale,T.is_nullable AS IsNullable
                            FROM sys.types T JOIN sys.systypes ST ON T.system_type_id=ST.xusertype
                            WHERE is_user_defined=1";

            if (typeNames != null && typeNames.Count() > 0)
            {
                string strTableNames = StringHelper.GetSingleQuotedString(typeNames);
                sql += $" AND T.name in ({ strTableNames })";
            }

            return base.GetUserDefinedTypes(dbConnector, sql);
        }
        #endregion

        #region Table
        public override List<Table> GetTables(params string[] tableNames)
        {
            DbConnector dbConnector = this.GetDbConnector();

            string sql = $@"SELECT schema_name(t.schema_id) as [Owner], t.name as Name, ext2.value as [Comment],
                        IDENT_SEED(schema_name(t.schema_id)+'.'+t.name) AS IdentitySeed,IDENT_INCR(schema_name(t.schema_id)+'.'+t.name) AS IdentityIncrement
                        FROM sys.tables t
                        LEFT JOIN sys.extended_properties ext ON t.object_id=ext.major_id AND ext.minor_id=0 AND ext.class=1 AND ext.name='microsoft_database_tools_support'
                        LEFT JOIN sys.extended_properties ext2 ON t.object_id=ext2.major_id and ext2.minor_id=0 AND ext2.class_desc='OBJECT_OR_COLUMN' AND ext2.name='MS_Description'
                        WHERE t.is_ms_shipped=0 AND ext.class is null
                       ";

            if (tableNames != null && tableNames.Count()>0)
            {
                string strTableNames = StringHelper.GetSingleQuotedString(tableNames);
                sql += $" AND t.name in ({ strTableNames })";
            }

            sql += " ORDER BY t.name";


            return base.GetTables(dbConnector, sql);
        }
        #endregion

        #region Table Column
        public override List<TableColumn> GetTableColumns(params string[] tableNames)
        {
            DbConnector dbConnector = this.GetDbConnector();

            string sql = @"SELECT schema_name(T.schema_id) AS [Owner], T.name AS TableName,C.name AS ColumnName, ST.name AS DataType,C.is_nullable AS IsNullable,C.max_length AS MaxLength, C.precision AS Precision,C.column_id as [Order], 
                           C.scale AS Scale,SCO.text As DefaultValue, EXT.value AS [Comment],C.is_identity AS IsIdentity,STY.is_user_defined AS IsUserDefined,schema_name(STY.schema_id) AS [TypeOwner]
                        FROM sys.columns C 
                        JOIN sys.systypes ST ON C.user_type_id = ST.xusertype
                        JOIN sys.tables T ON C.object_id=T.object_id
                        LEFT JOIN sys.syscomments SCO ON C.default_object_id=SCO.id
                        LEFT JOIN sys.extended_properties EXT on C.column_id=EXT.minor_id AND C.object_id=EXT.major_id AND EXT.class_desc='OBJECT_OR_COLUMN' AND EXT.name='MS_Description'
						LEFT JOIN sys.types STY on C.user_type_id = STY.user_type_id";

            if (tableNames != null && tableNames.Count() > 0)
            {
                string strTableNames = StringHelper.GetSingleQuotedString(tableNames);
                sql += $" WHERE T.name in ({ strTableNames })";
            }

            return base.GetTableColumns(dbConnector, sql);
        }
        #endregion

        #region Table Primary Key
        public override List<TablePrimaryKey> GetTablePrimaryKeys(params string[] tableNames)
        {
            DbConnector dbConnector = this.GetDbConnector();

            string sql = @"SELECT schema_name(T.schema_id) AS [Owner], object_name(IC.object_id) AS TableName,I.name AS KeyName, C.name AS ColumnName, IC.key_ordinal AS [Order],IC.is_descending_key AS IsDesc
                         FROM sys.index_columns IC
                         JOIN sys.columns C ON IC.object_id=C.object_id AND IC.column_id=C.column_id						
                         JOIN sys.indexes I ON IC.object_id=I.object_id AND IC.index_id=I.index_id
                         JOIN sys.tables T ON C.object_id=T.object_id
                         WHERE I.is_primary_key=1";

            if (tableNames != null && tableNames.Count() > 0)
            {
                string strTableNames = StringHelper.GetSingleQuotedString(tableNames);
                sql += $" AND object_name(IC.object_id) IN ({ strTableNames })";
            }

            return base.GetTablePrimaryKeys(dbConnector, sql);
        }
        #endregion

        #region Table Foreign Key
        public override List<TableForeignKey> GetTableForeignKeys(params string[] tableNames)
        {
            DbConnector dbConnector = this.GetDbConnector();

            string sql = @"SELECT schema_name(T.schema_id) AS [Owner],object_name(FK.parent_object_id) AS TableName,FK.name AS KeyName,C.name AS ColumnName,
                         object_name(FKC.referenced_object_id) AS ReferencedTableName,RC.name AS ReferencedColumnName,
                         FK.update_referential_action AS UpdateCascade,FK.delete_referential_action AS DeleteCascade
                         FROM sys.foreign_keys FK
                         JOIN sys.foreign_key_columns FKC ON FK.object_id=FKC.constraint_object_id AND FK.object_id=FKC.constraint_object_id
                         JOIN sys.columns C ON FK.parent_object_id=C.object_id AND  FKC.parent_column_id=C.column_id
                         JOIN sys.columns RC ON FKC.referenced_object_id= RC.object_id AND RC.column_id=FKC.referenced_column_id
                         JOIN sys.tables T ON C.object_id=T.object_id
                         JOIN sys.tables RT ON RC.object_id=RT.object_id AND RT.schema_id=T.schema_id";

            if (tableNames != null && tableNames.Count() > 0)
            {
                string strTableNames = StringHelper.GetSingleQuotedString(tableNames);
                sql += $" AND object_name(FK.parent_object_id) IN ({ strTableNames })";
            }

            return base.GetTableForeignKeys(dbConnector, sql);
        }
        #endregion

        #region Table Index
        public override List<TableIndex> GetTableIndexes(params string[] tableNames)
        {
            DbConnector dbConnector = this.GetDbConnector();

            string sql = @"SELECT schema_name(T.schema_id) AS [Owner],object_name(IC.object_id) AS TableName,I.name AS IndexName, I.is_unique AS IsUnique, C.name AS ColumnName, IC.key_ordinal AS [Order],IC.is_descending_key AS IsDesc
                        FROM sys.index_columns IC
                        JOIN sys.columns C ON IC.object_id=C.object_id AND IC.column_id=C.column_id
                        JOIN sys.indexes I ON IC.object_id=I.object_id AND IC.index_id=I.index_id
                        JOIN sys.tables T ON C.object_id=T.object_id
                        WHERE I.is_primary_key=0 and I.type_desc<>'XML'";

            if (tableNames != null && tableNames.Count() > 0)
            {
                string strTableNames = StringHelper.GetSingleQuotedString(tableNames);
                sql += $" AND object_name(IC.object_id) IN ({ strTableNames })";
            }

            return base.GetTableIndexes(dbConnector, sql);
        }
        #endregion

        #region Identity
        public override void SetIdentityEnabled(DbConnection dbConnection, TableColumn column, bool enabled)
        {
            this.ExecuteNonQuery(dbConnection, $"SET IDENTITY_INSERT {GetQuotedTableName(new Table() { Name=column.TableName, Owner=column.Owner })} {(enabled? "OFF": "ON")}");
        }
        #endregion       

        #region Generate Schema Script   

        public override string GenerateSchemaScripts(SchemaInfo schemaInfo)
        {
            StringBuilder sb = new StringBuilder();

            #region User Defined Type
            foreach (UserDefinedType userDefinedType in schemaInfo.UserDefinedTypes)
            {
                this.FeedbackInfo($"Begin generate user defined type {userDefinedType.Name} script.");

                TableColumn column = new TableColumn() { DataType=userDefinedType.Type, MaxLength=userDefinedType.MaxLength, Precision=userDefinedType.Precision, Scale=userDefinedType.Scale };
                string dataLength = this.GetColumnDataLength(column);

                sb.AppendLine($@"CREATE TYPE {GetQuotedString(userDefinedType.Owner)}.{GetQuotedString(userDefinedType.Name)} FROM {GetQuotedString(userDefinedType.Type)}{(dataLength==""? "": "("+dataLength+")")} {(userDefinedType.IsRequired? "NOT NULL":"NULL")};");

                this.FeedbackInfo($"End generate user defined type {userDefinedType.Name} script.");
            }

            sb.AppendLine("GO");
          
            #endregion

            foreach (Table table in schemaInfo.Tables)
            {
                this.FeedbackInfo($"Begin generate table {table.Name} script.");

                string tableName = table.Name;
                string quotedTableName = this.GetQuotedTableName(table);
                IEnumerable<TableColumn> tableColumns = schemaInfo.Columns.Where(item => item.Owner==table.Owner && item.TableName == tableName).OrderBy(item => item.Order);

                bool hasBigDataType = tableColumns.Any(item => this.IsBigDataType(item));

                string primaryKey = "";

                IEnumerable<TablePrimaryKey> primaryKeys = schemaInfo.TablePrimaryKeys.Where(item => item.Owner==table.Owner && item.TableName == tableName);

                #region Primary Key
                if (Option.GenerateKey && primaryKeys.Count() > 0)
                {
                    primaryKey =
$@"
,CONSTRAINT {GetQuotedString(primaryKeys.First().KeyName)} PRIMARY KEY CLUSTERED 
(
{string.Join(Environment.NewLine, primaryKeys.Select(item => $"{GetQuotedString(item.ColumnName)} {(item.IsDesc ? "DESC" : "ASC")},")).TrimEnd(',')}
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]";

                }

                #endregion

                #region Create Table
                sb.Append(
$@"
SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON

CREATE TABLE {quotedTableName}(
{string.Join("," + Environment.NewLine, tableColumns.Select(item => this.TranslateColumn(table, item) ))}{primaryKey}
) ON [PRIMARY]{(hasBigDataType ? " TEXTIMAGE_ON [PRIMARY]" : "")}");
                #endregion

                sb.AppendLine();

                #region Comment
                if (!string.IsNullOrEmpty(table.Comment))
                {
                    sb.AppendLine($"EXECUTE sp_addextendedproperty N'MS_Description',N'{ValueHelper.TransferSingleQuotation(table.Comment)}',N'SCHEMA',N'{table.Owner}',N'table',N'{tableName}',NULL,NULL;");
                } 
                
                foreach(TableColumn column in tableColumns.Where(item=>!string.IsNullOrEmpty(item.Comment)))
                {
                    sb.AppendLine($"EXECUTE sp_addextendedproperty N'MS_Description',N'{ValueHelper.TransferSingleQuotation(column.Comment)}',N'SCHEMA',N'{table.Owner}',N'table',N'{tableName}',N'column',N'{column.ColumnName}';");
                }
                #endregion               

                #region Foreign Key
                if (Option.GenerateKey)
                {                   
                    IEnumerable<TableForeignKey> foreignKeys = schemaInfo.TableForeignKeys.Where(item => item.Owner==table.Owner && item.TableName == tableName);
                    if (foreignKeys.Count() > 0)
                    {
                        ILookup<string, TableForeignKey> foreignKeyLookup = foreignKeys.ToLookup(item => item.KeyName);

                        IEnumerable<string> keyNames = foreignKeyLookup.Select(item => item.Key);

                        foreach (string keyName in keyNames)
                        {
                            TableForeignKey tableForeignKey = foreignKeyLookup[keyName].First();
                           
                            string columnNames = string.Join(",", foreignKeyLookup[keyName].Select(item => $"[{item.ColumnName}]"));
                            string referenceColumnName = string.Join(",", foreignKeyLookup[keyName].Select(item => $"[{item.ReferencedColumnName}]"));

                            sb.Append(
$@"
ALTER TABLE {quotedTableName} WITH CHECK ADD CONSTRAINT [{keyName}] FOREIGN KEY({columnNames})
REFERENCES {GetQuotedString(table.Owner)}.{GetQuotedString(tableForeignKey.ReferencedTableName)} ({referenceColumnName})
");

                            if (tableForeignKey.UpdateCascade)
                            {
                                sb.AppendLine("ON UPDATE CASCADE");
                            }

                            if (tableForeignKey.DeleteCascade)
                            {
                                sb.AppendLine("ON DELETE CASCADE");
                            }

                            sb.AppendLine($"ALTER TABLE {quotedTableName} CHECK CONSTRAINT [{keyName}];");
                        }
                    }
                }
                #endregion

                #region Index
                if (Option.GenerateIndex)
                {
                    IEnumerable<TableIndex> indices = schemaInfo.TableIndices.Where(item => item.Owner==table.Owner && item.TableName == tableName).OrderBy(item => item.Order);
                    if (indices.Count() > 0)
                    {
                        sb.AppendLine();

                        List<string> indexColumns = new List<string>();
                        ILookup<string, TableIndex> indexLookup = indices.ToLookup(item => item.IndexName);
                        IEnumerable<string> indexNames = indexLookup.Select(item => item.Key);
                        foreach (string indexName in indexNames)
                        {
                            TableIndex tableIndex = indexLookup[indexName].First();
                            string columnNames = string.Join(",", indexLookup[indexName].Select(item => $"{GetQuotedString(item.ColumnName)} {(item.IsDesc ? "DESC" : "ASC")}"));

                            if (indexColumns.Contains(columnNames))
                            {
                                continue;
                            }
                            sb.AppendLine($"CREATE {(tableIndex.IsUnique ? "UNIQUE" : "")} INDEX {tableIndex.IndexName} ON {quotedTableName}({columnNames});");
                            if (!indexColumns.Contains(columnNames))
                            {
                                indexColumns.Add(columnNames);
                            }
                        }
                    }
                }
                #endregion

                #region Default Value
                if (Option.GenerateDefaultValue)
                {
                    IEnumerable<TableColumn> defaultValueColumns = schemaInfo.Columns.Where(item => item.Owner== table.Owner && item.TableName == tableName && !string.IsNullOrEmpty(item.DefaultValue));
                    foreach (TableColumn column in defaultValueColumns)
                    {
                        sb.AppendLine($"ALTER TABLE {quotedTableName} ADD CONSTRAINT {GetQuotedString($" DF_{tableName}_{column.ColumnName}")}  DEFAULT {column.DefaultValue} FOR [{column.ColumnName}];");
                    }
                }
                #endregion

                this.FeedbackInfo($"End generate table {table.Name} script.");
            }

            if(Option.ScriptOutputMode==GenerateScriptOutputMode.WriteToFile)
            {
                this.AppendScriptsToFile(sb.ToString(), GenerateScriptMode.Schema, true);
            }           

            return sb.ToString();
        }

        public override string TranslateColumn(Table table, TableColumn column)
        {
            if(column.IsUserDefined)
            {
                
                return $@"{GetQuotedString(column.ColumnName)} {GetQuotedString(column.TypeOwner)}.{GetQuotedString(column.DataType)} {(column.IsRequired ? "NOT NULL" : "NULL")}";
            }

            string dataLength = this.GetColumnDataLength(column);            

            if (!string.IsNullOrEmpty(dataLength))
            {
                dataLength = $"({dataLength})";
            }         

            return $@"{GetQuotedString(column.ColumnName)} {GetQuotedString(column.DataType)} {dataLength} {(this.Option.GenerateIdentity && column.IsIdentity? $"IDENTITY({table.IdentitySeed},{table.IdentityIncrement})":"")} {(column.IsRequired ? "NOT NULL" : "NULL")}";
        }

        private string GetColumnDataLength(TableColumn column)
        {
            switch (column.DataType)
            {
                case "nchar":
                case "nvarchar":                    
                    if (column.MaxLength == -1)
                    {
                        return "max";
                    }
                    return ((column.MaxLength ?? 0) / 2).ToString();
                case "char":
                case "varchar":
                case "binary":
                case "varbinary":
                    if (column.MaxLength == -1)
                    {
                        return "max";
                    }
                    return column.MaxLength?.ToString();
                case "bit":
                case "tinyint":
                case "int":
                case "smallint":
                case "bigint":
                case "float":
                case "real":
                case "money":
                case "smallmoney":
                case "date":
                case "smalldatetime":
                case "datetime":
                case "timestamp":
                case "uniqueidentifier":
                case "xml":
                case "text":
                case "ntext":
                case "image":
                case "sql_variant":
                case "geography":
                case "geometry":
                case "hierarchyid":
                    return "";
                case "datetime2":
                case "datetitmeoffset":
                    return column.Scale?.ToString();
                case "decimal":
                case "numeric":
                    return $"{column.Precision},{column.Scale}";
            }

            return "";
        }

        private bool IsBigDataType(TableColumn column)
        {
            switch (column.DataType)
            {
                case "text":
                case "ntext":
                case "image":
                case "xml":
                    return true;
                case "varchar":
                case "nvarchar":
                case "varbinary":
                    if (column.MaxLength == -1)
                    {
                        return true;
                    }
                    return false;
                default:
                    return false;
            }
        }

        #endregion

        #region Generate Data Script       
        public override long GetTableRecordCount(DbConnection connection, Table table)
        {
            string sql = $"SELECT COUNT(1) FROM {this.GetQuotedTableName(table)}";

            return base.GetTableRecordCount(connection, sql);
        }
        public override string GenerateDataScripts(SchemaInfo schemaInfo)
        {
            return base.GenerateDataScripts(schemaInfo);
        }      

        protected override string GetPagedSql(string tableName, string columnNames, string primaryKeyColumns, string whereClause, long pageNumber, int pageSize)
        {
            var startEndRowNumber = PaginationHelper.GetStartEndRowNumber(pageNumber, pageSize);

            string pagedSql = $@"with PagedRecords as
								(
									SELECT TOP 100 PERCENT {columnNames}, ROW_NUMBER() OVER (ORDER BY (SELECT 0)) AS ROWNUMBER
									FROM {tableName}
                                    {whereClause}
								)
								SELECT *
								FROM PagedRecords
								WHERE ROWNUMBER BETWEEN {startEndRowNumber.StartRowNumber} AND {startEndRowNumber.EndRowNumber}";

            return pagedSql;
        }
        #endregion       
    }
}
