namespace DatabaseMigration.Core
{
    public class SqlServerProvider:IDbProvider
    {
        public string ProviderName => "System.Data.SqlClient";           
    }
}
