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

            var types = (from type in Assembly.GetExecutingAssembly().GetTypes()
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
    }
}
