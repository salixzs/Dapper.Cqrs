using System.Diagnostics.CodeAnalysis;
using Xunit;
using Xunit.Abstractions;

namespace Salix.Dapper.Cqrs.MsSql.Testing.XUnit
{
    [Trait("Category", "Database")]
    [ExcludeFromCodeCoverage]
    public abstract class MsSqlTestBase
    {
        /// <summary>
        /// Access to Database tests fixture - common class for tests, holding database connection objects/context and CQRS context.
        /// </summary>
        protected SqlDatabaseFixture TestFixture { get; set; }

        /// <summary>
        /// Method to get SQL Server database connection string for tests to access.
        /// Usually here load DB connection string from configuration and return to caller.
        /// </summary>
        protected abstract string GetSqlConnectionString();

        /// <summary>
        /// Initializes the test context with necessary objects for each test class.
        /// Should be called from test class constructor.
        /// </summary>
        /// <example>
        /// public class MyTests : DatabaseTestsBase
        /// {
        ///     public MyTests(ITestOutputHelper outputHelper, SqlDatabaseFixture testFixture)
        ///     {
        ///         this.InitializeTestContext(outputHelper, testFixture);
        ///     }
        ///   // Here goes tests
        /// }
        /// </example>
        /// <param name="helper">The XUit output helper (like Console.Write).</param>
        /// <param name="fixture">The database tests fixture.</param>
        protected void InitializeTestContext(ITestOutputHelper helper, SqlDatabaseFixture fixture)
        {
            this.TestFixture = fixture;
            this.TestFixture.SqlConnection = this.GetSqlConnectionString();
            this.TestFixture.InstantiateDatabaseObjects(helper);
        }
    }
}
