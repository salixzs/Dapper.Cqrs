using Microsoft.Extensions.Configuration;
using Salix.Dapper.Cqrs.MsSql;
using Salix.Dapper.Cqrs.MsSql.Testing.XUnit;
using Sample.AspNet5Api.Database.Tests.Preparation;
using Xunit;

namespace Sample.AspNet5Api.Database.Tests
{
    /// <summary>
    /// Own database test collection base class, which need to implement how SQL connection string is to be retrieved.
    /// Also utilizes [Collection] attribute so actual test classes don't have to.
    /// </summary>
    /// <seealso cref="MsSqlTestBase" />
    [Collection(nameof(SqlTestsCollectionAttr))]
    public abstract class DatabaseTestsBase : MsSqlTestBase
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

            if (!this.TestFixture.Db.Query(new TableOrViewExistsQuery("ArtistAlbumView")))
            {
                this.TestFixture.Db.Execute(new ArtistAlbumViewCreateCommand());
            }

            if (!this.TestFixture.Db.Query(new StoredProcedureExistsQuery("ArtistAlbumSp")))
            {
                this.TestFixture.Db.Execute(new ArtistAlbumProcedureCreateCommand());
            }

            if (!this.TestFixture.Db.Query(new TableOrViewExistsQuery("TestColumnTypes")))
            {
                this.TestFixture.Db.Execute(new TestTableCreateCommand());
            }

            if (!this.TestFixture.Db.Query(new FunctionExistsQuery("CheckSql")))
            {
                this.TestFixture.Db.Execute(new CheckSqlFunctionCreateCommand());
            }

            this.TestFixture.Db.CommitTransaction();
        }
    }
}
