using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseMigration.Core
{
    public class OracleProvider:IDbProvider
    {
        public string ProviderName => "Oracle.ManagedDataAccess.Client";      
    }
}
