using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseMigration.Core
{
    public class DbConvetorInfo
    {
        public DbInterpreter DbInterpreter { get; set; }       
        public string DbOwner { get; set; }              
    }
}
