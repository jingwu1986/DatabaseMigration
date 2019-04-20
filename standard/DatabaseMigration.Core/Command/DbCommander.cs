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

        public int ExecuteNonQuery()
        {
            this.OpenConnection();
            return dbCommand.ExecuteNonQuery();
        }

        public Task<int> ExecuteNonQueryAsync()
        {
            this.OpenConnection();
            return dbCommand.ExecuteNonQueryAsync();
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
