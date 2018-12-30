using DatabaseMigration.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseMigration.Test
{
    public class InterpreterTestRuner
    {
        public static void Run(InterpreterTest test, params string []tableNames)
        {
            List<Table> tables = test.GetTables(tableNames);
            OutputHelper.Output(FormatName(test, "GetTables"), tables, true);

            List<TableColumn> tableColumns = test.GetTableColumns(tableNames);
            OutputHelper.Output(FormatName(test, "GetTableColumns"), tableColumns, true);

            List<TablePrimaryKey> tablePrimaryKeys = test.GetTablePrimaryKeys(tableNames);
            OutputHelper.Output(FormatName(test, "GetTablePrimaryKeys"), tablePrimaryKeys, true);

            List<TableForeignKey> tableForeignKeys = test.GetTableForeignKeys(tableNames);
            OutputHelper.Output(FormatName(test, "GetTableForeignKeys"), tableForeignKeys, true);

            List<TableIndex> tableIndices = test.GetTableIndexes(tableNames);
            OutputHelper.Output(FormatName(test, "GetTableIndexes"), tableIndices, true);

            string schema =  test.GenerateSchemaScripts(tableNames);
            OutputHelper.Output(FormatName(test, "GenerateSchemaScripts"), schema, false);

            string data = test.GenerateDataScripts(tableNames);
            OutputHelper.Output(FormatName(test, "GenerateDataScripts"), data, false);
        }     
        
        private static string FormatName(InterpreterTest test, string name)
        {
            return $"{test.Interpreter.GetType().Name}_{name}";
        }
    }
}
