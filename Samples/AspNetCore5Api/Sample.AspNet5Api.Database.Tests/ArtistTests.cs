using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Salix.Dapper.Cqrs.MsSql.Testing.XUnit;
using Sample.AspNet5Api.Commands;
using Sample.AspNet5Api.Database.Tests.Faker;
using Sample.AspNet5Api.Domain;
using Sample.AspNet5Api.Queries;
using Xunit;
using Xunit.Abstractions;

namespace Sample.AspNet5Api.Database.Tests
{
    public class ArtistTests : DatabaseTestsBase
    {
        public ArtistTests(ITestOutputHelper outputHelper, SqlDatabaseFixture testFixture) =>
            this.InitializeTestContext(outputHelper, testFixture);

        [Fact]
        public async Task Columns_Db2Dto_Match()
        {
            this.TestFixture.ReopenTransaction();
            List<string> compareProblems = await HelperDatabaseTests.CompareDatabaseWithContract<Artist>(this.TestFixture);
            foreach (string problem in compareProblems)
            {
                this.TestFixture.WriteOutput(problem);
            }

            compareProblems.Should().BeEmpty("DB and Data contract should match, but there are {0} problems found with that. See Standard output", compareProblems.Count);
        }

        [Fact]
        public async Task Save_Read_Match()
        {
            this.TestFixture.ReopenTransaction();

            // Preparing and creating new Artist record
            Artist testBogus = TestObjectFactory.Instance.GetTestObject<Artist>();
            int autoIncrementedIdFromDatabase = await this.TestFixture.Db.ExecuteAsync(new ArtistCreateCommand(testBogus));
            this.TestFixture.WriteOutput($"Created artist with ID: {autoIncrementedIdFromDatabase}");

            // Now read back saved data in testable object
            Artist testable = await this.TestFixture.Db.QueryAsync(new ArtistByIdQuery(autoIncrementedIdFromDatabase));

            // To avoid leaving saved data in database
            // (Do this before Asserts as asserting may fail test and not run this if it is the last statement in test)
            this.TestFixture.RollbackTransaction();

            testable.Should().NotBeNull();
            testBogus.ArtistId = autoIncrementedIdFromDatabase; // Make initial bogus ID equivalent to auto-incremented value for object comparison below.
            testable.Should().BeEquivalentTo(testBogus);
        }
    }
}
