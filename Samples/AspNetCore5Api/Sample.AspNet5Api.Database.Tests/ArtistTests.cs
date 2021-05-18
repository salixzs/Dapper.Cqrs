using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Salix.Dapper.Cqrs.MsSql.Testing.XUnit;
using Sample.AspNet5Api.Domain;
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
    }
}
