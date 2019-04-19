using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseMigration.Core
{
    public class MySqlProvider : IDbProvider
    {
        public string ProviderName => "MySql.Data.MySqlClient";
    }
}
