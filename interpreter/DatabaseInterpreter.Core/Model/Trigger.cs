namespace DatabaseInterpreter.Model
{
    public class Trigger : DatabaseObject
    {
        public string TableName { get; set; }
        public string Definition { get; set; }
    }
}
