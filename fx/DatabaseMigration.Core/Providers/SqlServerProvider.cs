using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseMigration.Core
{
    public class SqlServerProvider:IDbProvider
    {
        public string ProviderName => "System.Data.SqlClient";           
    }
}
