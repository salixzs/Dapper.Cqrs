using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Salix.Dapper.Cqrs.MsSql;
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

            List<PocoPropertyMetadata> pocoProperties = HelperQueryCommandClasses.GetPocoObjectProperties<TestColumnTypes>();
            var databaseColumnNames = (await this.TestFixture.Db
                .QueryAsync(new DatabaseObjectColumnsMetadataQuery("TestColumnTypes"))).ToList();



            // databaseColumnNames.ExceptWith(propertyNames);
            // databaseColumnNames.Count.Should().Be(0);
        }
    }
}
