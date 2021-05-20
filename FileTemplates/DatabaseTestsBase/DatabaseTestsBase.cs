using Microsoft.Extensions.Configuration;
using Salix.Dapper.Cqrs.MsSql;
using Salix.Dapper.Cqrs.MsSql.Testing.XUnit;
using Sample.AspNet5Api.Database.Tests.Preparation;
using Xunit;

namespace $rootnamespace$
{
    /// <summary>
    /// Database assembly/integration tests base class.
    /// </summary>
    [Collection(nameof(SqlTestsCollectionAttr))]
    public abstract class $safeitemname$ : MsSqlTestBase
    {
        /// <inheritdoc/>
        protected override string GetSqlConnectionString()
        {
            // On DevOps server builds it should use somehow transformed configuration file, if these tests are run on server.
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .AddJsonFile("testsettings.json")
                //.AddUserSecrets() // Use own developer settings, but beware to use actual database which might be specified in user secrets!
                //.AddEnvironmentVariables() // This can be used on Azure
                .Build();

            return configuration["Database:ConnectionString"];
        }

        /// <inheritdoc />
        protected override void PrepareDatabase()
        {
            this.TestFixture.WriteOutput("Preparing database. Creating test objects.");

            if (!this.TestFixture.Db.Query(new FunctionExistsQuery("CheckSql")))
            {
                this.TestFixture.Db.Execute(new CheckSqlFuctionCreateCommand());
            }

            if (!this.TestFixture.Db.Query(new TableOrViewExistsQuery("YourStructureChanges")))
            {
                // this.TestFixture.Db.Execute(new StructureCreateCommand());
            }

            this.TestFixture.Db.CommitTransaction();
        }
    }

    /// <summary>
    /// Collection attribute for Database tests, which requires the same (single) setup and dispose/cleanup.
    /// This attribute should be put on base class for all such tests (see above).
    /// </summary>
    [CollectionDefinition(nameof(SqlTestsCollectionAttr))]
    public class SqlTestsCollectionAttr : ICollectionFixture<SqlDatabaseFixture>
    {
        // This class has no code, and is never created. Its purpose is simply
        // to be the place to apply [CollectionDefinition] and all the
        // ICollectionFixture<> interfaces.
    }
}
