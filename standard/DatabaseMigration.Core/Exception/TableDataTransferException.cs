using System;

namespace DatabaseMigration.Core
{
    public class TableDataTransferException : Exception
    {
        public Exception BaseException { get; set; }
        public TableDataTransferException(Exception ex)
        {
            this.BaseException = ex;
        }
        public string SourceServer { get; set; }
        public string SourceDatabase { get; set; }
        public string SourceTableName { get; set; }

        public string TargetServer { get; set; }
        public string TargetDatabase { get; set; }
        public string TargetTableName { get; set; }

        public override string Message => BaseException.Message;


        public override string StackTrace
        {
            get
            {
               return
               $"SourceServer:{this.SourceServer}" + Environment.NewLine +
               $"SourceDatabase:{this.SourceDatabase}" + Environment.NewLine +
               $"SourceTableName:{this.SourceTableName}" + Environment.NewLine +
               $"TargetServer:{this.TargetServer}" + Environment.NewLine +
               $"TargetDatabase:{this.TargetDatabase}" + Environment.NewLine +
               $"TargetTableName:{this.TargetTableName}" + Environment.NewLine +
               BaseException?.StackTrace;
            }
        }
    }
}
