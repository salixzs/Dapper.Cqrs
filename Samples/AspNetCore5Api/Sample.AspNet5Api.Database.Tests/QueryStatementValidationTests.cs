using System;
using Salix.Dapper.Cqrs.Abstractions;
using Salix.Dapper.Cqrs.MsSql.Testing.XUnit;
using Sample.AspNet5Api.Queries;
using Xunit;
using Xunit.Abstractions;

namespace Sample.AspNet5Api.Database.Tests
{
    /// <summary>
    /// Test validates all existing IQuery implementations in Sample.AspNet5Api.Queries project
    /// against database.
    /// </summary>
    public class QueryStatementValidationTests : DatabaseTestsBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="QueryStatementValidationTests"/> class.
        /// Should call Collection/Fixture method to set up all database and logging infrastructure.
        /// </summary>
        /// <param name="outputHelper">The XUnit output helper (like Console.Write).</param>
        /// <param name="testFixture">The database tests fixture injected by collection definition.</param>
        public QueryStatementValidationTests(ITestOutputHelper outputHelper, SqlDatabaseFixture testFixture) =>
            this.InitializeTestContext(outputHelper, testFixture);

        /// <summary>
        /// MEGA-TEST, which looks up all IQuery implementations (see <see cref="QueryValidatorClasses"/> property above)
        /// It validates SqlStatement inside IQuery class by calling its Validate method.
        /// </summary>
        /// <param name="queryClassType">Type of the query class (supplied by [MemberData] attribute method).</param>
        /// <remarks>
        /// [MemberData] attribute parameters are:
        /// 1 - method to get all types of IQueryValidator classes,
        /// 2 - any existing IQueryValidator class from assembly where all those classes are located,
        /// 3 - Class (static), where Method (1) is to be found.
        /// </remarks>
        [Theory(DisplayName = "Query validate for: ")]
        [MemberData(nameof(HelperQueryCommandClasses.QueryValidatorClassesForAssemblyType), typeof(AlbumByIdQuery), MemberType = typeof(HelperQueryCommandClasses))]
        public void QueryClass_SqlValidate_Succeeds(Type queryClassType)
        {
            // We need as-if-real parameters supplied for query (if any)
            object[] parameters = HelperQueryCommandClasses.CreateDummyParametersForType(queryClassType);
            var instance = (IQueryValidator)Activator.CreateInstance(queryClassType, parameters);
            instance.Validate(this.TestFixture.SqlSession);
        }
    }
}
