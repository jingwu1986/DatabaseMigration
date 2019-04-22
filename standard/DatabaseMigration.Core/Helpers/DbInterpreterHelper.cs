using System;
using System.Linq;
using System.Reflection;

namespace DatabaseMigration.Core
{
    public class DbInterpreterHelper
    {
        public static DbInterpreter GetDbInterpreter(DatabaseType dbType, ConnectionInfo connectionInfo, GenerateScriptOption generateScriptOption)
        {
            DbInterpreter dbInterpreter = null;

            var assembly = Assembly.GetExecutingAssembly();
            //var typeArray = assembly.GetTypes();
            var typeArray = assembly.ExportedTypes;

            var types = (from type in typeArray
                         where type.IsSubclassOf(typeof(DbInterpreter))
                         select type).ToList();

            foreach (var type in types)
            {
                dbInterpreter = (DbInterpreter)Activator.CreateInstance(type, connectionInfo, generateScriptOption);

                if (dbInterpreter.DatabaseType == dbType)
                {
                    return dbInterpreter;
                }
            }

            return dbInterpreter;
        }

        public static string GetOwnerName(DbInterpreter dbInterpreter)
        {
            if (dbInterpreter.DatabaseType == DatabaseType.Oracle)
            {
                return dbInterpreter.ConnectionInfo.UserId;
            }
            else
            {
                if(dbInterpreter.DatabaseType==DatabaseType.SqlServer)
                {
                    return "dbo";
                }

                return dbInterpreter.ConnectionInfo.Database;
            }
        }
    }
}
