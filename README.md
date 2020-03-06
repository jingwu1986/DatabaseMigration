# DatabaseMigration
Migrate objects between different databases, including schema and data.
It supports to generate primary key, foreign key, index, identity, default value and comment, and supports datatype mapping via adding mapping file. It supports data batch submit, and considers data self reference of a table. Currently, it implements sync tables and views between SqlServer, Oracle and MySql.

## UI
![UI Screenshot](https://github.com/victor-wiki/StaticResources/blob/master/StaticResources/images/projs/DatabaseMigration/screenshot.png?raw=true)

## Test code

```
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
            InterpreterTestRuner.Run(new InterpreterTest(sqlServerInterpreter), new SelectionInfo() { });
            //InterpreterTestRuner.Run(new InterpreterTest(mySqlInterpreter), new SelectionInfo() { });
            //InterpreterTestRuner.Run(new InterpreterTest(oracleInterpreter), new SelectionInfo() { });
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
                string msg = ExceptionHelper.GetExceptionDetails(ex);               

                Feedback(new FeedbackInfo() { InfoType = FeedbackInfoType.Error, Message = msg  });
            }
        }

        private static void Feedback(FeedbackInfo info)
        {
            if (info.InfoType == FeedbackInfoType.Error)
            {
                Console.ForegroundColor = ConsoleColor.Red;               
            }
            
            LogHelper.LogInfo(info.Message);
            
            Console.WriteLine(info.Message);
        }
    }
    
```
