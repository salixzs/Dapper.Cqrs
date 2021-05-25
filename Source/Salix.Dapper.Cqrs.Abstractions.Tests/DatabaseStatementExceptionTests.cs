using System;
using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Xunit;

namespace Salix.Dapper.Cqrs.Abstractions.Tests
{
    [ExcludeFromCodeCoverage]
    public class DatabaseStatementExceptionTests
    {
        [Fact]
        public void SqlStatment_Saved_InData()
        {
            var ex = new DatabaseStatementSyntaxException("testing", "Passed SQL statement");
            ex.Data.Should().NotBeNull();
            ex.Data.Should().HaveCount(1);
            ex.Data.Keys.Should().Contain("SQL");
            ex.Data["SQL"].Should().Be("Passed SQL statement");
            ex.Message.Should().Be("testing");
        }

        [Fact]
        public void Exception_MesageConstructor_Implemented()
        {
            var ex = new DatabaseStatementSyntaxException("testing");
            ex.Message.Should().Be("testing");
        }

        [Fact]
        public void Exception_InnerConstructor_Implemented()
        {
            var ex = new DatabaseStatementSyntaxException("testing", new ArgumentException("Internal fail"));
            ex.Message.Should().Be("testing");
            ex.InnerException.Should().NotBeNull();
        }
    }
}
