﻿using DatabaseInterpreter.Core;
using DatabaseInterpreter.Model;
using DatabaseInterpreter.Utility;
using DatabaseMigration.Core;
using System;
using System.Threading.Tasks;

namespace DatabaseMigration.Demo
{
    public class ConvertorDemo: IObserver<FeedbackInfo>
    {
        private DbInterpreter sourceInterpreter;
        private DbInterpreter targetInterpreter;

        public DbInterpreter SourceInterpreter => this.sourceInterpreter;
        public DbInterpreter TargetInterpreter => this.targetInterpreter;

        public ConvertorDemo(DbInterpreter sourceInterpreter, DbInterpreter targetInterpreter)
        {
            this.sourceInterpreter = sourceInterpreter;
            this.targetInterpreter = targetInterpreter;
        }

        public async Task Convert()
        {
            DatabaseType sourceDbType = this.sourceInterpreter.DatabaseType;
            DatabaseType targetDbType = this.targetInterpreter.DatabaseType;

            int dataBatchSize = 500;

            DbInterpreterOption sourceScriptOption = new DbInterpreterOption() { ScriptOutputMode = GenerateScriptOutputMode.WriteToString, DataBatchSize = dataBatchSize };
            DbInterpreterOption targetScriptOption = new DbInterpreterOption() { ScriptOutputMode = (GenerateScriptOutputMode.WriteToFile | GenerateScriptOutputMode.WriteToString), DataBatchSize = dataBatchSize };

            this.sourceInterpreter.Option = sourceScriptOption;
            this.targetInterpreter.Option = targetScriptOption;

            GenerateScriptMode scriptMode = GenerateScriptMode.Schema | GenerateScriptMode.Data;

            DbConvetorInfo source = new DbConvetorInfo() { DbInterpreter = sourceInterpreter };
            DbConvetorInfo target = new DbConvetorInfo() { DbInterpreter = targetInterpreter };            

            try
            {
                using (DbConvertor dbConvertor = new DbConvertor(source, target))
                {                    
                    dbConvertor.Option.GenerateScriptMode = scriptMode;

                    dbConvertor.Subscribe(this);

                    if (sourceDbType == DatabaseType.MySql)
                    {
                        source.DbInterpreter.Option.InQueryItemLimitCount = 2000;
                    }

                    if (targetDbType == DatabaseType.SqlServer)
                    {
                        target.DbOwner = "dbo";
                    }
                    else if (targetDbType == DatabaseType.MySql)
                    {
                        target.DbInterpreter.Option.RemoveEmoji = true;
                    }
                    else if (targetDbType == DatabaseType.Oracle)
                    {
                        dbConvertor.Option.SplitScriptsToExecute = true;
                        dbConvertor.Option.ScriptSplitChar = ';';
                    }

                    FeedbackHelper.EnableLog = true;

                    await dbConvertor.Convert();
                }                   
            }
            catch (Exception ex)
            {
                string msg = ExceptionHelper.GetExceptionDetails(ex);

                this.Feedback(new FeedbackInfo() { InfoType = FeedbackInfoType.Error, Message = msg });
            }
        }

        private void Feedback(FeedbackInfo info)
        {
            Console.WriteLine(info.Message);

            if (info.InfoType == FeedbackInfoType.Error)
            {
                Console.ForegroundColor = ConsoleColor.Red;
            }                      
        }

        #region IObserver<FeedbackInfo>
        void IObserver<FeedbackInfo>.OnCompleted()
        {
        }
        void IObserver<FeedbackInfo>.OnError(Exception error)
        {
        }
        void IObserver<FeedbackInfo>.OnNext(FeedbackInfo info)
        {
            this.Feedback(info);
        }
        #endregion
    }
}
