using System;
using System.Collections.Generic;
using System.Text;

namespace DatabaseMigration.Core
{
    public class SelectionInfo
    {
        public string[] UserDefinedTypeNames { get; set; }
        public string[] TableNames { get; set; }
        public string[] ViewNames { get; set; }
    }
}
