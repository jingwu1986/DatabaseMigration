namespace DatabaseMigration.Core
{
    public class FeedbackInfo
    {
        public string Owner { get; set; }
        public FeedbackInfoType InfoType { get; set; }
        public string Message { get; set; }
    }

    public enum FeedbackInfoType
    {
        Info=0,
        Error=1
    }
}
