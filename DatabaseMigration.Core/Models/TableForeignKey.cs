using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseMigration.Core
{
    public class TableForeignKey
    {
        public string Owner { get; set; }
        public string TableName { get; set; }
        public string KeyName { get; set; }
        public string ColumnName { get; set; }
        public string ReferencedTableName { get; set; }
        public string ReferencedColumnName { get; set; }
        public bool UpdateCascade { get; set; }
        public bool DeleteCascade { get; set; }

        public string TableFullName => this.Owner + "." + this.TableName;
        public string ReferencedTableFullName=> this.Owner + "." + this.ReferencedTableName;
    }
}
