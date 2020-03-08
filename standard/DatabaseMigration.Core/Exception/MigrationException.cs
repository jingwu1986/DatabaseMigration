using System;
using System.Collections.Generic;
using System.Text;

namespace DatabaseMigration.Core
{
    public abstract class MigrationException : Exception
    {
        public Exception BaseException { get; set; }
        public abstract string ObjectType { get; }

        public MigrationException(Exception ex)
        {
            this.BaseException = ex;            
        }

        public string SourceServer { get; set; }
        public string SourceDatabase { get; set; }
        public string SourceObject { get; set; }

        public string TargetServer { get; set; }
        public string TargetDatabase { get; set; }
        public string TargetObject { get; set; }

        public override string Message => BaseException.Message;

        public override string StackTrace
        {
            get
            {
                return
                $"ObjectType:{this.ObjectType}" + Environment.NewLine +
                $"SourceServer:{this.SourceServer}" + Environment.NewLine +
                $"SourceDatabase:{this.SourceDatabase}" + Environment.NewLine +
                $"SourceObject:{this.SourceObject}" + Environment.NewLine +
                $"TargetServer:{this.TargetServer}" + Environment.NewLine +
                $"TargetDatabase:{this.TargetDatabase}" + Environment.NewLine +
                $"TargetObject:{this.TargetObject}" + Environment.NewLine +
                BaseException?.StackTrace;
            }
        }
    }    
}
