namespace DatabaseMigration.Core
{
    public interface IDbProvider
    {
        string ProviderName { get; }
    }
}
