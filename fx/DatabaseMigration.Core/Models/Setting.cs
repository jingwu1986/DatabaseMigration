namespace DatabaseMigration.Core
{
    public class Setting
    {
        public int CommandTimeout { get; set; } = 600;
        public int DataBatchSize { get; set; } = 500;
    }
}
