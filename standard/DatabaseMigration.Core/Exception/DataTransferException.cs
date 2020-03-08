using DatabaseInterpreter.Model;
using System;

namespace DatabaseMigration.Core
{
    public class DataTransferException: MigrationException
    {
        public override string ObjectType => nameof(Table);

        public DataTransferException(Exception ex) : base(ex) { }
    }
}
