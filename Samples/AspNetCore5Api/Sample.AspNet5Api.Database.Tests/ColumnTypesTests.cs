using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Salix.Dapper.Cqrs.MsSql.Testing.XUnit;
using Xunit;
using Xunit.Abstractions;

namespace Sample.AspNet5Api.Database.Tests
{
    /// <summary>
    /// A special test for NuGet package functionality checks.
    /// Not part of DEMO, although you can use it as reference to possibilities.
    /// </summary>
    public class ColumnTypesTests : DatabaseTestsBase
    {
        public ColumnTypesTests(ITestOutputHelper outputHelper, SqlDatabaseFixture testFixture) =>
            this.InitializeTestContext(outputHelper, testFixture);

        [Fact]
        public async Task Columns_Db2Dto_Match()
        {
            this.TestFixture.ReopenTransaction();
            var exceptPocoProperties = new HashSet<string> { "NotInDatabase" };
            List<string> compareProblems = await HelperDatabaseTests.CompareDatabaseWithContract<TestColumnTypes>(this.TestFixture, exceptPocoProperties: exceptPocoProperties);
            foreach (string problem in compareProblems)
            {
                this.TestFixture.WriteOutput(problem);
            }

            compareProblems.Should().BeEmpty("DB and Data contract should match, but there are {0} problems found with that. See Standard output", compareProblems.Count);
        }
    }
}
