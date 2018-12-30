using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseMigration.Core
{
    public class DbInterpreterHelper
    {
        public static DbInterpreter GetDbInterpreter(DatabaseType dbType, ConnectionInfo connectionInfo, GenerateScriptOption generateScriptOption)
        {
            DbInterpreter dbInterpreter = null;
            switch (dbType)
            {
                case DatabaseType.SqlServer:
                    dbInterpreter = new SqlServerInterpreter(connectionInfo, generateScriptOption);
                    break;
                case DatabaseType.MySql:
                    dbInterpreter = new MySqlInterpreter(connectionInfo, generateScriptOption);
                    break;
                case DatabaseType.Oracle:
                    dbInterpreter = new OracleInterpreter(connectionInfo, generateScriptOption);
                    break;
                default:
                    throw new NotSupportedException($"Do not support {dbType} currently.");
            }
            return dbInterpreter;
        }       
    }
}
