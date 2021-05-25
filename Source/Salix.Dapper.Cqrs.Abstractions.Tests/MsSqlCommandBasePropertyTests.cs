using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Xunit;

namespace Salix.Dapper.Cqrs.Abstractions.Tests
{
    [ExcludeFromCodeCoverage]
    public class MsSqlCommandBasePropertyTests
    {
        [Fact]
        public void EmptyCommand_Properties_Check()
        {
            var testable = new EmptyCommand();
            _ = testable.SqlStatement.Should().Be("SQL Statement is not overridden in inheriting class");
            _ = testable.Parameters.Should().BeNull();
        }

        [Fact]
        public void SimpleCommand_Properties_Check()
        {
            var testable = new SimpleCommand();
            _ = testable.SqlStatement.Should().Be("UPDATE Table SET Fld = @val");
            _ = testable.Parameters.Should().Be(new { val = 12 });
        }

        [Fact]
        public void ToString_Shows_Command()
        {
            var testable = new SimpleCommand();
            testable.ToString().Should().Be("UPDATE Table SET Fld = @val");
        }
    }
}
