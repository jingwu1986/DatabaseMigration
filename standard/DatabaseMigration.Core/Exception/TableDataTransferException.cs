﻿using System;

namespace DatabaseMigration.Core
{
    public class TableDataTransferException: Exception
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
    }
}
