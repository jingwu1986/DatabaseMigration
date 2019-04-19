using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseMigration.Core
{
    public class TableIndex
    {
        public string Owner { get; set; }
        public string TableName { get; set; }
        public string IndexName { get; set; }
        public bool IsUnique { get; set; }
        public string ColumnName { get; set; }
        public int Order { get; set; }
        public bool IsDesc { get; set; }
    }
}
