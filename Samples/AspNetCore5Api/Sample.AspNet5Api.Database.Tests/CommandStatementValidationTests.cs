using System;
using Salix.Dapper.Cqrs.Abstractions;
using Salix.Dapper.Cqrs.MsSql.Testing.XUnit;
using Sample.AspNet5Api.Commands;
using Sample.AspNet5Api.Database.Tests.Faker;
using Xunit;
using Xunit.Abstractions;

namespace Sample.AspNet5Api.Database.Tests
{
    /// <summary>
    /// Test validates all existing ICommand implementations in Sample.AspNet5Api.Commands project
    /// against database.
    /// </summary>
    public class CommandStatementValidationTests : DatabaseTestsBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CommandStatementValidationTests"/> class.
        /// Should call Collection/Fixture method to set up all database and logging infrastructure.
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
        /// <remarks>
        /// [MemberData] attribute parameters are:
        /// 1 - method to get all types of ICommandValidator classes,
        /// 2 - any existing ICommandValidator class from assembly where all those classes are located,
        /// 3 - Class (static), where Method (1) is to be found.
        /// </remarks>
        [Theory(DisplayName = "Command validate for: ")]
        [MemberData(nameof(HelperQueryCommandClasses.CommandValidatorClassesForAssemblyType), typeof(ArtistCreateCommand), MemberType = typeof(HelperQueryCommandClasses))]
        public void CommandClass_SqlValidate_Succeeds(Type commandClassType)
        {
            // We need as-if-real parameters supplied for query (if any)
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
