namespace DatabaseMigration.Core
{
    public class View : DatabaseObject
    {        
        public string Definition { get; set; }
        public int Order { get; set; }       
    }
}
