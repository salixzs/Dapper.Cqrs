using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Xunit;

namespace Salix.Dapper.Cqrs.Abstractions.Tests
{
    public class DatabaseStatementExceptionTests
    {
        [Fact]
        [ExcludeFromCodeCoverage]
        public void SqlStatment_Saved_InData()
        {
            var ex = new DatabaseStatementSyntaxException("testing", "Passed SQL statement");
            ex.Data.Should().NotBeNull();
            ex.Data.Should().HaveCount(1);
            ex.Data.Keys.Should().Contain("SQL");
            ex.Data["SQL"].Should().Be("Passed SQL statement");
            ex.Message.Should().Be("testing");
        }
    }
}
