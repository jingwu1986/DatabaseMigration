using DatabaseInterpreter.Model;
using System;

namespace DatabaseMigration.Core
{
    public class SchemaTransferException : MigrationException
    {
        public override string ObjectType => nameof(DatabaseObject);

        public SchemaTransferException(Exception ex) : base(ex) { }
    }
}
