using System.Collections.Generic;

namespace DatabaseMigration.Core
{
    public class SchemaInfo
    {
        public List<UserDefinedType> UserDefinedTypes { get; set; } = new List<UserDefinedType>();
        public List<Table> Tables { get; set; } = new List<Table>();
        public List<TableColumn> Columns { get; set; } = new List<TableColumn>();
        public List<TablePrimaryKey> TablePrimaryKeys { get; set; }= new List<TablePrimaryKey>();
        public List<TableForeignKey> TableForeignKeys { get; set; }= new List<TableForeignKey>();
        public List<TableIndex> TableIndices { get; set; } = new List<TableIndex>();

        public Table PickupTable { get; set; }       
    }
}
