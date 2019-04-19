using System;
using DatabaseMigration.Core;

namespace DatabaseMigration.Test
{
    class Program
    {
        static ConnectionInfo sqlServerConn = new ConnectionInfo() { Server = "127.0.0.1", Database = "Northwind", UserId = "sa", Password = "123" };
        static ConnectionInfo mySqlConn = new ConnectionInfo() { Server = "localhost", Database = "northwind", UserId = "sa", Password = "1234" };
        static ConnectionInfo oracleConn = new ConnectionInfo() { Server = "127.0.0.1/orcl", Database = "test", UserId = "C##TEST", Password = "test" };

        static GenerateScriptOption option = new GenerateScriptOption()
        {
            ScriptOutputMode = GenerateScriptOutputMode.WriteToString | GenerateScriptOutputMode.WriteToFile,
            ScriptOutputFolder = "output"
        };

        static SqlServerInterpreter sqlServerInterpreter = new SqlServerInterpreter(sqlServerConn, option);
        static MySqlInterpreter mySqlInterpreter = new MySqlInterpreter(mySqlConn, option);
        static OracleInterpreter oracleInterpreter = new OracleInterpreter(oracleConn, option);           

        static void Main(string[] args)
        {
            TestInterpreter();

            //TestConvertor(sqlServerInterpreter, mySqlInterpreter);           

            Console.ReadLine();
        }       

        static void TestInterpreter()
        {
            InterpreterTestRuner.Run(new InterpreterTest(sqlServerInterpreter));
            //InterpreterTestRuner.Run(new InterpreterTest(mySqlInterpreter));
            //InterpreterTestRuner.Run(new InterpreterTest(oracleInterpreter));
        }

        static void TestConvertor(DbInterpreter sourceInterpreter, DbInterpreter targetInterpreter)
        {
            DatabaseType sourceDbType = sourceInterpreter.DatabaseType;
            DatabaseType targetDbType = targetInterpreter.DatabaseType;

            int dataBatchSize = 500;

            GenerateScriptOption sourceScriptOption = new GenerateScriptOption() { ScriptOutputMode = GenerateScriptOutputMode.WriteToString, DataBatchSize = dataBatchSize };
            GenerateScriptOption targetScriptOption = new GenerateScriptOption() { ScriptOutputMode = ( GenerateScriptOutputMode.WriteToFile | GenerateScriptOutputMode.WriteToString), DataBatchSize = dataBatchSize };

            sourceInterpreter.Option = sourceScriptOption;
            targetInterpreter.Option = targetScriptOption;

            GenerateScriptMode scriptMode = GenerateScriptMode.Schema | GenerateScriptMode.Data;

            DbConvetorInfo source = new DbConvetorInfo() { DbInterpreter = sourceInterpreter };
            DbConvetorInfo target = new DbConvetorInfo() { DbInterpreter = targetInterpreter };

            DbConvertor dbConvertor = new DbConvertor(source, target, null);
            dbConvertor.Option.GenerateScriptMode = scriptMode;

            dbConvertor.OnFeedback += Feedback;

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

            try
            {
                dbConvertor.Convert();
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
                if (ex is TableDataTransferException)
                {
                    TableDataTransferException dataException = ex as TableDataTransferException;
                    msg = $"Error occurs when sync data of table {dataException.TargetTableName}:{msg}";
                }

                msg += Environment.NewLine + "StackTrace:" + Environment.NewLine + ex.StackTrace;

                Feedback(new FeedbackInfo() { InfoType = FeedbackInfoType.Error, Message = msg  });
            }
        }

        private static void Feedback(FeedbackInfo info)
        {
            if (info.InfoType == FeedbackInfoType.Error)
            {
                Console.ForegroundColor = ConsoleColor.Red;               
            }
            Console.WriteLine(info.Message);
        }
    }
}
