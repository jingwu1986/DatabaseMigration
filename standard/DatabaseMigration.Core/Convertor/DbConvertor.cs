using DatabaseInterpreter.Core;
using DatabaseInterpreter.Model;
using DatabaseInterpreter.Utility;
using DatabaseMigration.Profile;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseMigration.Core
{
    public delegate void FeedbackHandle(FeedbackInfo info);

    public class DbConvertor : IDisposable
    {
        private IObserver<FeedbackInfo> observer;
        private bool hasError = false;
        private DbConnection dataTransferDbConnection;

        public bool HasError => this.hasError;

        public DbConvetorInfo Source { get; set; }
        public DbConvetorInfo Target { get; set; }

        public DbConvertorOption Option { get; set; } = new DbConvertorOption();

        public event FeedbackHandle OnFeedback;

        public DbConvertor(DbConvetorInfo source, DbConvetorInfo target)
        {
            this.Source = source;
            this.Target = target;
        }

        public DbConvertor(DbConvetorInfo source, DbConvetorInfo target, DbConvertorOption option)
        {
            this.Source = source;
            this.Target = target;
            if (option != null)
            {
                this.Option = option;
            }
        }

        public void Subscribe(IObserver<FeedbackInfo> observer)
        {
            this.observer = observer;
        }

        public Task Convert(SchemaInfo schemaInfo = null, bool getAllIfNotSpecified = true)
        {
            return this.InternalConvert(schemaInfo, getAllIfNotSpecified);
        }

        private async Task InternalConvert(SchemaInfo schemaInfo = null, bool getAllIfNotSpecified = true)
        {
            DbInterpreter sourceInterpreter = this.Source.DbInterpreter;

            sourceInterpreter.Option.TreatBytesAsNullForScript = true;

            string[] tableNames = null;
            string[] userDefinedTypeNames = null;
            string[] viewNames = null;

            if (schemaInfo == null || getAllIfNotSpecified)
            {
                tableNames = (await sourceInterpreter.GetTablesAsync()).Select(item => item.Name).ToArray();
                userDefinedTypeNames = (await sourceInterpreter.GetUserDefinedTypesAsync()).Select(item => item.Name).ToArray();
                viewNames = (await sourceInterpreter.GetViewsAsync()).Select(item => item.Name).ToArray();
            }
            else
            {
                tableNames = schemaInfo.Tables.Select(t => t.Name).ToArray();
                userDefinedTypeNames = schemaInfo.UserDefinedTypes.Select(item => item.Name).ToArray();
                viewNames = schemaInfo.Views.Select(item => item.Name).ToArray();
            }

            SelectionInfo selectionInfo = new SelectionInfo()
            {
                UserDefinedTypeNames = userDefinedTypeNames,
                TableNames = tableNames,
                ViewNames = viewNames
            };

            SchemaInfo sourceSchemaInfo = await sourceInterpreter.GetSchemaInfoAsync(selectionInfo, getAllIfNotSpecified);

            #region Set data type by user define type          

            List<UserDefinedType> utypes = await sourceInterpreter.GetUserDefinedTypesAsync();

            if (utypes != null && utypes.Count > 0)
            {
                foreach (TableColumn column in sourceSchemaInfo.Columns)
                {
                    UserDefinedType utype = utypes.FirstOrDefault(item => item.Name == column.DataType);
                    if (utype != null)
                    {
                        column.DataType = utype.Type;
                        column.MaxLength = utype.MaxLength;
                    }
                }
            }

            #endregion

            SchemaInfo targetSchemaInfo = SchemaInfoHelper.Clone(sourceSchemaInfo);

            if (!string.IsNullOrEmpty(this.Target.DbOwner))
            {
                SchemaInfoHelper.TransformOwner(targetSchemaInfo, this.Target.DbOwner);
            }

            targetSchemaInfo.Columns = ColumnTranslator.Translate(targetSchemaInfo.Columns, this.Source.DbInterpreter.DatabaseType, this.Target.DbInterpreter.DatabaseType);

            ViewTranslator viewTranslator = new ViewTranslator(targetSchemaInfo.Views, sourceInterpreter, this.Target.DbInterpreter, this.Target.DbOwner);
            targetSchemaInfo.Views = viewTranslator.Translate();

            if (this.Option.EnsurePrimaryKeyNameUnique)
            {
                SchemaInfoHelper.EnsurePrimaryKeyNameUnique(targetSchemaInfo);
            }

            if (this.Option.EnsureIndexNameUnique)
            {
                SchemaInfoHelper.EnsureIndexNameUnique(targetSchemaInfo);
            }

            DbInterpreter targetInterpreter = this.Target.DbInterpreter;
            bool generateIdentity = targetInterpreter.Option.GenerateIdentity;

            if (generateIdentity)
            {
                targetInterpreter.Option.InsertIdentityValue = true;
            }

            string script = "";

            sourceInterpreter.Subscribe(this.observer);
            targetInterpreter.Subscribe(this.observer);

            if (this.Option.GenerateScriptMode.HasFlag(GenerateScriptMode.Schema))
            {
                script = targetInterpreter.GenerateSchemaScripts(targetSchemaInfo);

                if (string.IsNullOrEmpty(script))
                {
                    this.Feedback(targetInterpreter, $"The script to create schema is null.", FeedbackInfoType.Error);
                    return;
                }

                targetInterpreter.Feedback(FeedbackInfoType.Info, "Begin to sync schema...");

                try
                {
                    if (!this.Option.SplitScriptsToExecute)
                    {
                        if (targetInterpreter is SqlServerInterpreter)
                        {
                            string[] scriptItems = script.Split(new string[] { "GO" + Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

                            scriptItems.ToList().ForEach(async item =>
                            {
                                if (!targetInterpreter.HasError)
                                {
                                    targetInterpreter.Feedback(FeedbackInfoType.Info, $"executing {item}");

                                    await targetInterpreter.ExecuteNonQuery(item);
                                }
                            });
                        }
                        else
                        {
                            targetInterpreter.Feedback(FeedbackInfoType.Info, $"executing {script}");

                            await targetInterpreter.ExecuteNonQuery(script);
                        }
                    }
                    else
                    {
                        string[] sqls = script.Split(new char[] { this.Option.ScriptSplitChar }, StringSplitOptions.RemoveEmptyEntries);
                        int count = sqls.Count();

                        int i = 0;
                        foreach (string sql in sqls)
                        {
                            if (!string.IsNullOrEmpty(sql.Trim()))
                            {
                                i++;

                                if (!targetInterpreter.HasError)
                                {
                                    targetInterpreter.Feedback(FeedbackInfoType.Info, $"({i}/{count}), executing {sql}");
                                    await targetInterpreter.ExecuteNonQuery(sql.Trim());
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    targetInterpreter.CancelRequested = true;

                    ConnectionInfo sourceConnectionInfo = sourceInterpreter.ConnectionInfo;
                    ConnectionInfo targetConnectionInfo = targetInterpreter.ConnectionInfo;

                    SchemaTransferException schemaTransferException = new SchemaTransferException(ex)
                    {
                        SourceServer = sourceConnectionInfo.Server,
                        SourceDatabase = sourceConnectionInfo.Database,
                        TargetServer = targetConnectionInfo.Server,
                        TargetDatabase = targetConnectionInfo.Database
                    };

                    this.HandleError(schemaTransferException);
                }

                targetInterpreter.Feedback(FeedbackInfoType.Info, "End sync schema.");
            }

            if (!targetInterpreter.HasError && this.Option.GenerateScriptMode.HasFlag(GenerateScriptMode.Data) && sourceSchemaInfo.Tables.Count > 0)
            {
                List<TableColumn> identityTableColumns = new List<TableColumn>();
                if (generateIdentity)
                {
                    identityTableColumns = targetSchemaInfo.Columns.Where(item => item.IsIdentity).ToList();
                }

                if (this.Option.PickupTable != null)
                {
                    sourceSchemaInfo.PickupTable = this.Option.PickupTable;
                }

                sourceInterpreter.AppendScriptsToFile("", GenerateScriptMode.Data, true);
                targetInterpreter.AppendScriptsToFile("", GenerateScriptMode.Data, true);

                this.dataTransferDbConnection = targetInterpreter.GetDbConnector().CreateConnection();

                {
                    identityTableColumns.ForEach(item =>
                    {
                        if (targetInterpreter.DatabaseType == DatabaseType.SqlServer)
                        {
                            targetInterpreter.SetIdentityEnabled(this.dataTransferDbConnection, item, false);
                        }
                    });

                    if (this.Option.ExecuteScriptOnTargetServer || targetInterpreter.Option.ScriptOutputMode.HasFlag(GenerateScriptOutputMode.WriteToFile))
                    {
                        sourceInterpreter.OnDataRead += async (table, columns, data, dbDataReader) =>
                        {
                            if (!this.hasError)
                            {
                                try
                                {
                                    StringBuilder sb = new StringBuilder();

                                    (Table Table, List<TableColumn> Columns) targetTableAndColumns = this.GetTargetTableColumns(targetSchemaInfo, this.Target.DbOwner, table, columns);

                                    if (targetTableAndColumns.Table == null || targetTableAndColumns.Columns == null)
                                    {
                                        return;
                                    }

                                    Dictionary<string, object> paramters = targetInterpreter.AppendDataScripts(this.Target.DbInterpreter.Option, sb, targetTableAndColumns.Table, targetTableAndColumns.Columns, new Dictionary<long, List<Dictionary<string, object>>>() { { 1, data } });

                                    try
                                    {
                                        script = sb.ToString();
                                        sb.Clear();
                                    }
                                    catch (OutOfMemoryException)
                                    {
                                        sb.Clear();
                                    }

                                    if (this.Option.ExecuteScriptOnTargetServer)
                                    {
                                        if (!this.Option.SplitScriptsToExecute)
                                        {
                                            if (this.Option.BulkCopy && targetInterpreter.SupportBulkCopy)
                                            {
                                                await targetInterpreter.BulkCopyAsync(this.dataTransferDbConnection, dbDataReader, table.Name);
                                            }
                                            else
                                            {
                                                await targetInterpreter.ExecuteNonQuery(this.dataTransferDbConnection, script, paramters, false);
                                            }
                                        }
                                        else
                                        {
                                            string[] sqls = script.Split(new char[] { this.Option.ScriptSplitChar }, StringSplitOptions.RemoveEmptyEntries);

                                            foreach (string sql in sqls)
                                            {
                                                if (!string.IsNullOrEmpty(sql.Trim()))
                                                {
                                                    if (this.Option.BulkCopy && targetInterpreter.SupportBulkCopy)
                                                    {
                                                        await targetInterpreter.BulkCopyAsync(this.dataTransferDbConnection, dbDataReader, table.Name);
                                                    }
                                                    else
                                                    {
                                                        await targetInterpreter.ExecuteNonQuery(this.dataTransferDbConnection, sql, paramters, false);
                                                    }
                                                }
                                            }
                                        }

                                        targetInterpreter.FeedbackInfo($"Table \"{table.Name}\":{data.Count} records transferred.");
                                    }
                                }
                                catch (Exception ex)
                                {
                                    sourceInterpreter.CancelRequested = true;

                                    ConnectionInfo sourceConnectionInfo = sourceInterpreter.ConnectionInfo;
                                    ConnectionInfo targetConnectionInfo = targetInterpreter.ConnectionInfo;

                                    DataTransferException dataTransferException = new DataTransferException(ex)
                                    {
                                        SourceServer = sourceConnectionInfo.Server,
                                        SourceDatabase = sourceConnectionInfo.Database,
                                        SourceObject = table.Name,
                                        TargetServer = targetConnectionInfo.Server,
                                        TargetDatabase = targetConnectionInfo.Database,
                                        TargetObject = table.Name
                                    };

                                    this.HandleError(dataTransferException);

                                    DataTransferErrorProfileManager.Save(new DataTransferErrorProfile
                                    {
                                        SourceServer = sourceConnectionInfo.Server,
                                        SourceDatabase = sourceConnectionInfo.Database,
                                        SourceTableName = table.Name,
                                        TargetServer = targetConnectionInfo.Server,
                                        TargetDatabase = targetConnectionInfo.Database,
                                        TargetTableName = table.Name
                                    });
                                }
                            }
                        };
                    }

                    await sourceInterpreter.GenerateDataScriptsAsync(sourceSchemaInfo);

                    identityTableColumns.ForEach(item =>
                    {
                        if (targetInterpreter.DatabaseType == DatabaseType.SqlServer)
                        {
                            targetInterpreter.SetIdentityEnabled(this.dataTransferDbConnection, item, true);
                        }
                    });
                }
            }
        }

        private void HandleError(MigrationException ex)
        {
            this.hasError = true;
            string errMsg = ExceptionHelper.GetExceptionDetails(ex);
            this.Feedback(this, errMsg, FeedbackInfoType.Error);
        }

        private (Table Table, List<TableColumn> Columns) GetTargetTableColumns(SchemaInfo targetSchemaInfo, string targetOwner, Table sourceTable, List<TableColumn> sourceColumns)
        {
            Table targetTable = targetSchemaInfo.Tables.FirstOrDefault(item => (item.Owner == targetOwner || string.IsNullOrEmpty(targetOwner)) && item.Name == sourceTable.Name);
            if (targetTable == null)
            {
                this.Feedback(this, $"Source table {sourceTable.Name} cannot get a target table.", FeedbackInfoType.Error);
                return (null, null);
            }

            List<TableColumn> targetTableColumns = new List<TableColumn>();
            foreach (TableColumn sourceColumn in sourceColumns)
            {
                TableColumn targetTableColumn = targetSchemaInfo.Columns.FirstOrDefault(item => (item.Owner == targetOwner || string.IsNullOrEmpty(targetOwner)) && item.TableName == sourceColumn.TableName && item.ColumnName == sourceColumn.ColumnName);
                if (targetTableColumn == null)
                {
                    this.Feedback(this, $"Source column {sourceColumn.TableName} of table {sourceColumn.TableName} cannot get a target column.", FeedbackInfoType.Error);
                    return (null, null);
                }
                targetTableColumns.Add(targetTableColumn);
            }
            return (targetTable, targetTableColumns);
        }

        public void Feedback(object owner, string content, FeedbackInfoType infoType = FeedbackInfoType.Info, bool enableLog = true)
        {
            if (infoType == FeedbackInfoType.Error)
            {
                this.hasError = true;
            }

            FeedbackInfo info = new FeedbackInfo() { InfoType = infoType, Message = content, Owner = owner };

            FeedbackHelper.Feedback(this.observer, info, enableLog);

            if (this.OnFeedback != null)
            {
                this.OnFeedback(info);
            }
        }

        public void Dispose()
        {
            if (this.dataTransferDbConnection != null)
            {
                this.dataTransferDbConnection.Close();
                this.dataTransferDbConnection.Dispose();
            }
        }
    }
}
