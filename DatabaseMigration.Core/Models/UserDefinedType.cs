namespace DatabaseMigration.Core
{
    public class UserDefinedType
    {
        public string Owner { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public int MaxLength { get; set; }
        public int Precision { get; set; }
        public int Scale { get; set; }

        public bool IsRequired => !IsNullable;

        public bool IsNullable { get; set; }
    }
}
