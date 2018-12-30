using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseMigration.Core
{
    public class SchemaInfo
    {
        public List<Table> Tables { get; set; } = new List<Table>();
        public List<TableColumn> Columns { get; set; } = new List<TableColumn>();
        public List<TablePrimaryKey> TablePrimaryKeys { get; set; }= new List<TablePrimaryKey>();
        public List<TableForeignKey> TableForeignKeys { get; set; }= new List<TableForeignKey>();
        public List<TableIndex> TableIndices { get; set; } = new List<TableIndex>();

        public Table PickupTable { get; set; }       
    }
}
