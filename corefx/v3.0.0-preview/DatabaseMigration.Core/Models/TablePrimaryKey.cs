using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseMigration.Core
{
    public class TablePrimaryKey
    {
        public string Owner { get; set; }
        public string TableName { get; set; }
        public string KeyName { get; set; }
        public string ColumnName { get; set; }
        public int Order { get; set; }
        public bool IsDesc { get; set; }
    }
}
