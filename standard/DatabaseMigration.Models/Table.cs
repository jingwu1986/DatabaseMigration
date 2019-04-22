﻿namespace DatabaseMigration.Core
{
    public class Table : DatabaseObject
    {
        public string Comment { get; set; }
        public int Order { get; set; }
        public int? IdentitySeed { get; set; }
        public int? IdentityIncrement { get; set; }

        //public string FullName => this.Owner + "." + this.Name;
    }
}
