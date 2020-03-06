using DatabaseMigration.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseMigration.Test
{
    public class InterpreterTest
    {
        public DbInterpreter Interpreter;
        public InterpreterTest(DbInterpreter dbInterpreter)
        {
            this.Interpreter = dbInterpreter;
        }

        #region Table
        public List<Table> GetTables(params string[] tableNames)
        {
            return Interpreter.GetTables(tableNames);
        }
        #endregion

        #region Table Column
        public List<TableColumn> GetTableColumns(params string[] tableNames)
        {
            return Interpreter.GetTableColumns(tableNames);
        }
        #endregion

        #region Table Primary Key
        public List<TablePrimaryKey> GetTablePrimaryKeys(params string[] tableNames)
        {
            return Interpreter.GetTablePrimaryKeys(tableNames);
        }
        #endregion

        #region Table Foreign Key
        public List<TableForeignKey> GetTableForeignKeys(params string[] tableNames)
        {
            return Interpreter.GetTableForeignKeys(tableNames);
        }
        #endregion

        #region Table Index
        public List<TableIndex> GetTableIndexes(params string[] tableNames)
        {
            return Interpreter.GetTableIndexes(tableNames);
        }
        #endregion

        #region View
        public List<View> GetViews(params string[] viewNames)
        {
            return Interpreter.GetViews(viewNames);
        }
        #endregion

        #region Schema Scripts
        public string GenerateSchemaScripts(SchemaInfo schemaInfo)
        {
            SelectionInfo selectionInfo = new SelectionInfo()
            {
                TableNames = schemaInfo.Tables.Select(item => item.Name).ToArray()
            };

            return Interpreter.GenerateSchemaScripts(Interpreter.GetSchemaInfo(selectionInfo, false));
        }
        #endregion

        #region Data Scripts
        public string GenerateDataScripts(SchemaInfo schemaInfo)
        {
            SelectionInfo selectionInfo = new SelectionInfo()
            {
                TableNames = schemaInfo.Tables.Select(item => item.Name).ToArray()
            };

            return Interpreter.GenerateDataScripts(Interpreter.GetSchemaInfo(selectionInfo, false));
        }
        #endregion
    }
}