using DatabaseInterpreter.Core;
using DatabaseInterpreter.Model;
using DatabaseMigration.Demo;
using System;

namespace DatabaseMigration.Test
{
    class Program
    {
        static ConnectionInfo sqlServerConn = new ConnectionInfo() { Server = @".\sql2019", Database = "Northwind", IntegratedSecurity = true };
        static ConnectionInfo mySqlConn = new ConnectionInfo() { Server = "localhost", Database = "northwind", UserId = "sa", Password = "1234" };
        static ConnectionInfo oracleConn = new ConnectionInfo() { Server = "127.0.0.1/orcl", Database = "Northwind", UserId = "C##northwind", Password = "TEST" };

        static DbInterpreterOption option = new DbInterpreterOption()
        {
            ScriptOutputMode = GenerateScriptOutputMode.WriteToString | GenerateScriptOutputMode.WriteToFile,
            ScriptOutputFolder = "output"
        };

        static SqlServerInterpreter sqlServerInterpreter = new SqlServerInterpreter(sqlServerConn, option);
        static MySqlInterpreter mySqlInterpreter = new MySqlInterpreter(mySqlConn, option);
        static OracleInterpreter oracleInterpreter = new OracleInterpreter(oracleConn, option);

        static void Main(string[] args)
        {
            RunDemo();

            Console.ReadLine();
        }

        static async void RunDemo()
        {
            //await ConvertorDemoRuner.Run(new ConvertorDemo(sqlServerInterpreter, mySqlInterpreter));
            //await ConvertorDemoRuner.Run(new ConvertorDemo(sqlServerInterpreter, oracleInterpreter));
            await ConvertorDemoRuner.Run(new ConvertorDemo(mySqlInterpreter, oracleInterpreter));

            Console.WriteLine("OK");
        }
    }
}
