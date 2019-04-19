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

        #region Schema Scripts
        public string GenerateSchemaScripts(params string[] tableNames)
        {
            SchemaInfo schemaInfo = Interpreter.GetSchemaInfo(tableNames);
            return Interpreter.GenerateSchemaScripts(schemaInfo);
        }
        #endregion

        #region Data Scripts
        public string GenerateDataScripts(params string[] tableNames)
        {
            SchemaInfo schemaInfo = Interpreter.GetSchemaInfo(tableNames);
            return Interpreter.GenerateDataScripts(schemaInfo);
        } 
        #endregion
    }
}
