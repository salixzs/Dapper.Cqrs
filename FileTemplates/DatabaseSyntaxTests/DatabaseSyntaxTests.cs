using System;
using Salix.Dapper.Cqrs.Abstractions;
using Salix.Dapper.Cqrs.MsSql.Testing.XUnit;
using Xunit;
using Xunit.Abstractions;

namespace $rootnamespace$
{
    /// <summary>
    /// Test validates all existing IQuery implementations in your code base against database.
    /// </summary>
    public class QueryStatementValidationTests : DatabaseTestsBase
    {
        /// <summary>
        /// Test validates all existing IQuery implementations in your code base against database.
        /// </summary>
        /// <param name="outputHelper">The XUnit output helper (like Console.Write).</param>
        /// <param name="testFixture">The database tests fixture injected by collection definition.</param>
        public QueryStatementValidationTests(ITestOutputHelper outputHelper, SqlDatabaseFixture testFixture) =>
            this.InitializeTestContext(outputHelper, testFixture);

        /// <summary>
        /// MEGA-TEST, which looks up all IQuery implementations (see <see cref="HelperQueryCommandClasses.QueryValidatorClassesForAssemblyType"/> method in helper class)
        /// It validates SqlStatement inside IQuery class by calling its Validate method.
        /// </summary>
        /// <param name="queryClassType">Type of the query class (supplied by [MemberData] attribute method).</param>
        [Theory(DisplayName = "Query validate for: ")]
        [MemberData(nameof(HelperQueryCommandClasses.QueryValidatorClassesForAssemblyType), typeof(OneOfYourDbQueryClass), MemberType = typeof(HelperQueryCommandClasses))]
        public void QueryClass_SqlValidate_Succeeds(Type queryClassType)
        {
            object[] parameters = HelperQueryCommandClasses.CreateDummyParametersForType(queryClassType);
            var instance = (IQueryValidator)Activator.CreateInstance(queryClassType, parameters);
            instance.Validate(this.TestFixture.SqlSession);
        }
    }

    /// <summary>
    /// Test validates all existing ICommand implementations in your solution against database.
    /// </summary>
    public class CommandStatementValidationTests : DatabaseTestsBase
    {
        /// <summary>
        /// Test validates all existing ICommand implementations in your solution against database.
        /// </summary>
        /// <param name="outputHelper">The XUnit output helper (like Console.Write).</param>
        /// <param name="testFixture">The database tests fixture injected by collection definition.</param>
        public CommandStatementValidationTests(ITestOutputHelper outputHelper, SqlDatabaseFixture testFixture) =>
            this.InitializeTestContext(outputHelper, testFixture);

        /// <summary>
        /// MEGA-TEST, which looks up all ICommand implementations (see <see cref="HelperQueryCommandClasses.CommandValidatorClassesForAssemblyType"/> method in helper)
        /// It validates SqlStatement inside ICommand class by calling its Validate method.
        /// </summary>
        /// <param name="commandClassType">Type of the command class (supplied by [MemberData] attribute method).</param>
        [Theory(DisplayName = "Command validate for: ")]
        [MemberData(nameof(HelperQueryCommandClasses.CommandValidatorClassesForAssemblyType), typeof(OneOfYourDbCommandClass), MemberType = typeof(HelperQueryCommandClasses))]
        public void CommandClass_SqlValidate_Succeeds(Type commandClassType)
        {
            object[] parameters = HelperQueryCommandClasses.CreateDummyParametersForType(commandClassType, TestObjectFactory.Instance);
            var instance = (ICommandValidator)Activator.CreateInstance(commandClassType, parameters);
            instance.Validate(this.TestFixture.SqlSession);

            // Make sure UPDATE and DELETE statements are containing WHERE clause to avoid all-update or all-delete situations.
            if ((instance.SqlStatement.StartsWith(
                     "UPDATE",
                     StringComparison.InvariantCultureIgnoreCase)
                 || instance.SqlStatement.StartsWith("DELETE", StringComparison.InvariantCultureIgnoreCase))
                && !instance.SqlStatement.Contains("WHERE", StringComparison.InvariantCultureIgnoreCase))
            {
                throw new DatabaseStatementSyntaxException("UPDATE or DELETE statement without WHERE clause.", instance.SqlStatement);
            }
        }
    }
}
