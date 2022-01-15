using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Xunit;

namespace Salix.Dapper.Cqrs.Abstractions.Tests
{
    [ExcludeFromCodeCoverage]
    public class MsSqlCommandBaseTests
    {
        private readonly Mock<IDatabaseSession> _dbSession;
        private string _sql;
        private object _parameterTypes;

        public MsSqlCommandBaseTests() => _dbSession = new Mock<IDatabaseSession>();
        private void SetupParamterRetrieval() =>
            _ = _dbSession.Setup(s => s.QueryFirstOrDefault<string>(It.IsAny<string>(), It.IsAny<object>()))
                .Callback<string, object>((q, p) =>
                {
                    var anonType = p.GetType();
                    _sql = (string)anonType.GetProperty("tsql").GetValue(p, null);
                    _parameterTypes = (string)anonType.GetProperty("parameterTypes").GetValue(p, null);
                })
                .Returns("OK");

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
            _ = RemoveNewLine(testable.RealSqlStatement).Should().Be("DECLARE @val INT = 12;UPDATE Table SET Fld = @val");
        }

        [Fact]
        public void ToString_Shows_Command()
        {
            var testable = new SimpleCommand();
            testable.ToString().Should().Be("UPDATE Table SET Fld = @val");
        }

        [Fact]
        public void Validate_OK_Works()
        {
            string query = null;
            object parameters = null;
            _ = _dbSession.Setup(s => s.QueryFirstOrDefault<string>(It.IsAny<string>(), It.IsAny<object>()))
                    .Callback<string, object>((q, p) => { parameters = p; query = q; })
                    .Returns("OK");
            var testable = new SimpleCommand();
            testable.Validate(_dbSession.Object);
            _dbSession.Verify(s => s.QueryFirstOrDefault<string>(It.IsAny<string>(), It.IsAny<object>()), Times.Once);
            _ = query.Should().Be("SELECT dbo.CheckSql(@tsql, @parameterTypes)");
            _ = parameters.Should().NotBeNull();
        }


        [Fact]
        public void Validate_Exception_Works()
        {
            _ = _dbSession.Setup(s => s.QueryFirstOrDefault<string>(It.IsAny<string>(), It.IsAny<object>()))
                    .Returns("Some error information is not OK.");
            var testable = new SimpleCommand();
            Action act = () => testable.Validate(_dbSession.Object);
            act.Should().Throw<DatabaseStatementSyntaxException>();
        }

        [Fact]
        public void Validate_Simple_AllSet()
        {
            this.SetupParamterRetrieval();
            var testable = new SimpleCommand();
            testable.Validate(_dbSession.Object);
            _ = _sql.Should().Be("UPDATE Table SET Fld = @val");
            _ = _parameterTypes.Should().Be("@val INT");
        }

        [Fact]
        public void CqrsCommandSync_Execute_GetsParameters()
        {
            var cqrs = new CommandQueryContext(_dbSession.Object);
            cqrs.Execute(new SimpleCommand());
            _dbSession.Verify(s => s.Execute(It.Is<string>(s => s == "UPDATE Table SET Fld = @val"), It.Is<object>(o => o.Equals(new { val = 12 }))), Times.Once);
            _dbSession.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task CqrsCommandAsync_Execute_GetsParameters()
        {
            var cqrs = new CommandQueryContext(_dbSession.Object);
            await cqrs.ExecuteAsync(new SimpleCommand());
            _dbSession.Verify(s => s.ExecuteAsync(It.Is<string>(s => s == "UPDATE Table SET Fld = @val"), It.Is<object>(o => o.Equals(new { val = 12 })), CancellationToken.None), Times.Once);
            _dbSession.VerifyNoOtherCalls();
        }

        [Fact]
        public void CqrsReturnCommandSync_Execute_GetsParameters()
        {
            var cqrs = new CommandQueryContext(_dbSession.Object);
            cqrs.Execute(new SimpleReturnCommand());
            _dbSession.Verify(s => s.Execute<int>(It.Is<string>(s => s == "INSERT INTO Table (Name) VALUES (@Name)"), It.Is<object>(o => o.Equals(new { Name = ".Net" }))), Times.Once);
            _dbSession.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task CqrsReturnCommandAsync_Execute_GetsParameters()
        {
            var cqrs = new CommandQueryContext(_dbSession.Object);
            await cqrs.ExecuteAsync(new SimpleReturnCommand());
            _dbSession.Verify(s => s.ExecuteAsync<int>(It.Is<string>(s => s == "INSERT INTO Table (Name) VALUES (@Name)"), It.Is<object>(o => o.Equals(new { Name = ".Net" })), CancellationToken.None), Times.Once);
            _dbSession.VerifyNoOtherCalls();
        }

        private static string RemoveNewLine(string sql) => sql.Replace("\r", string.Empty).Replace("\n", string.Empty);
    }
}
