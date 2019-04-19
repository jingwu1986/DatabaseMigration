using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseMigration.Core
{
    public interface IConnectionBuilder
    {
        string BuildConntionString(ConnectionInfo connectionInfo);
    }
}
