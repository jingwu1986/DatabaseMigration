using DatabaseInterpreter.Model;
using System;

namespace DatabaseMigration.Core
{
    public class ViewTranslateException : MigrationException
    {
        public override string ObjectType => nameof(View);

        public ViewTranslateException(Exception ex) : base(ex) { }
    }
}
