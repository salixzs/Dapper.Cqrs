using Salix.Dapper.Cqrs.MsSql.Testing.XUnit;
using Xunit;

namespace Sample.AspNet5Api.Database.Tests
{
    /// <summary>
    /// Collection attribute for Database tests, which requires the same (single) setup and dispose/cleanup.
    /// This attribute should be put on all tests classes which require actual database to work with.
    /// (Or on base class for all such tests).
    /// </summary>
    [CollectionDefinition(nameof(SqlTestsCollectionAttr))]
    public class SqlTestsCollectionAttr : ICollectionFixture<SqlDatabaseFixture>
    {
        // This class has no code, and is never created. Its purpose is simply
        // to be the place to apply [CollectionDefinition] and all the
        // ICollectionFixture<> interfaces.
    }
}
