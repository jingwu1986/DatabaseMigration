using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseMigration.Core
{
    public class DbCommander
    {
        private DbCommand dbCommand;

        public DbCommand DbCommand
        {
            get
            {
                return this.dbCommand;
            }
        }

        public DbCommander(DbConnection dbConnection, CommandType commandType, string commandText)
        {
            dbCommand = dbConnection.CreateCommand();
            dbCommand.Connection = dbConnection;
            dbCommand.CommandType = commandType;
            dbCommand.CommandText = commandText;
            dbCommand.CommandTimeout = SettingManager.Setting.CommandTimeout;
        }

        private void OpenConnection()
        {
            if (dbCommand.Connection.State != ConnectionState.Open)
            {
                dbCommand.Connection.Open();
            }
        }

        public void ExecuteNonQuery()
        {
            this.OpenConnection();
            dbCommand.ExecuteNonQuery();
        }

        public DbDataReader ExecteReader()
        {
            this.OpenConnection();
            return dbCommand.ExecuteReader();
        }

        public object ExecuteScalar()
        {
            this.OpenConnection();
            return dbCommand.ExecuteScalar();
        }

        public DataTable ExecteDataTable()
        {
            DataTable table = new DataTable();
            DbDataReader dataReader = this.ExecteReader();

            table.Load(dataReader);
            return table;
        }
    }
}
