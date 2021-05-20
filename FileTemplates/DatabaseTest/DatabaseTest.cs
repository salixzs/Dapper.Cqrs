using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Salix.Dapper.Cqrs.MsSql.Testing.XUnit;
using Xunit;
using Xunit.Abstractions;

namespace $rootnamespace$
{
    public class $safeitemname$ : DatabaseTestsBase
    {
        public $safeitemname$(ITestOutputHelper outputHelper, SqlDatabaseFixture testFixture) =>
            this.InitializeTestContext(outputHelper, testFixture);

        [Fact]
        public async Task Columns_Db2Dto_Match()
        {
            this.TestFixture.ReopenTransaction();
            List<string> compareProblems = await HelperDatabaseTests.CompareDatabaseWithContract<DbPoco>(this.TestFixture);
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

            // === Any dependencies needs to be created
            // DbDependencyPoco dependencyBogus = TestObjectFactory.Instance.GetTestObject<DbDependencyPoco>();
            // int dependencyBogusId = await this.TestFixture.Db.ExecuteAsync(new DbDependencyCreateCommand(dependencyBogus));
            // this.TestFixture.WriteOutput($"Created dependency with ID: {dependencyBogusId}");

            // Preparing and creating new Test record
            DbPoco testBogus = TestObjectFactory.Instance.GetTestObject<DbPoco>();
            // testBogus.DependencyId = dependencyBogusId;
            int testBogusId = await this.TestFixture.Db.ExecuteAsync(new DbPocoCreateCommand(testBogus));
            this.TestFixture.WriteOutput($"Created test record with ID: {testBogusId}");

            // Now read back saved data in testable object
            Album testable = await this.TestFixture.Db.QueryAsync(new AlbumByIdQuery(testBogusId));

            // To avoid leaving saved data in database
            this.TestFixture.RollbackTransaction();

            testable.Should().NotBeNull();
            testBogus.Id = testBogusId; // Make initial bogus ID equivalent to auto-incremented value for object comparison below.
            testable.Should().BeEquivalentTo(testBogus);
        }
    }
}
