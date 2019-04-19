using System.Data.Common;
using Westwind.Utilities;

namespace DatabaseMigration.Core
{
    public class DbConnector
    {
        private readonly IDbProvider dbProvider;
        private readonly string connectionString;

        public DbConnector(IDbProvider dbProvider, string connectionString)
        {
            this.dbProvider = dbProvider;
            this.connectionString = connectionString;
        }

        public DbConnector(IDbProvider dbProvider, IConnectionBuilder connectionBuilder, ConnectionInfo connectionInfo)
        {
            this.dbProvider = dbProvider;
            this.connectionString = connectionBuilder.BuildConntionString(connectionInfo);
        }

        public DbConnection CreateConnection()
        {
            DbProviderFactory factory = DataUtils.GetDbProviderFactory(this.dbProvider.ProviderName);
            DbConnection connection = factory.CreateConnection();
            connection.ConnectionString = this.connectionString;
            return connection;
        }
    }
}
