﻿using DatabaseInterpreter.Model;

namespace DatabaseMigration.Core
{
    public class DbConvertorOption
    {       
        public bool PickupTable { get; set; }
        public bool EnsurePrimaryKeyNameUnique { get; set; } = true;
        public bool EnsureIndexNameUnique { get; set; } = true;
        public bool SplitScriptsToExecute { get; set; }
        public bool ExecuteScriptOnTargetServer { get; set; } = true;      
        public GenerateScriptMode GenerateScriptMode { get; set; } = GenerateScriptMode.Schema | GenerateScriptMode.Data;
        public bool BulkCopy { get; set; }
        public bool UseTransaction { get; set; }
    }
}
