using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Xunit;
using Xunit.Abstractions;

namespace Salix.Dapper.Cqrs.MsSql.Tests
{
    /// <summary>
    /// Testing <see cref="SqlDatabaseSession" /> implementation.
    /// As methods does not have logic, tests are making sure necessary dependencies are called and logged properly.
    /// </summary>
#pragma warning disable CA1001 // Types that own disposable fields should be disposable - there is nothing to dispose. Just need to implement interface for it to work
    [ExcludeFromCodeCoverage]
    public class SqlDatabaseSessionTests
#pragma warning restore CA1001
    {
        private readonly Mock<IMsSqlContext> _sqlContext;
        private readonly XUnitLogger<SqlDatabaseSession> _logger;
        private readonly ITestOutputHelper _output;

        /// <summary>
        /// Initializes a new instance of the <see cref="SqlDatabaseSessionTests"/> class.
        /// This happens for each separate test (NOT once for all tests in class).
        /// </summary>
        /// <param name="output">The output.</param>
        public SqlDatabaseSessionTests(ITestOutputHelper output)
        {
            _output = output;
            _sqlContext = new Mock<IMsSqlContext>();
            _sqlContext.SetupGet(p => p.ExecutionTime).Returns(new System.TimeSpan(0, 0, 0, 0, 386)); // Execution time
            _logger = new XUnitLogger<SqlDatabaseSession>(_output);
        }

        [Fact]
        public void Query_Normally_AllHappens()
        {
            var testable = new SqlDatabaseSession(_sqlContext.Object, _logger);

            testable.Query<int>("SELECT Ids FROM Cable WHERE Name = @name", new { name = "Yop" });

            testable.ExecutionTime.Should().Be(new TimeSpan(0, 0, 0, 0, 386)); // Gets passed from internal dependency.
            _sqlContext.Verify(m => m.ExecuteSql(It.IsAny<Func<IDbTransaction, IEnumerable<int>>>()), Times.Once);
            _logger.LoggedMessages.Should().HaveCount(1);
            _logger.LoggedMessages[0]
                .Should()
                .Be("TRACE: Query<T>(SELECT Ids FROM Cable WHERE Name = @name) called with expected return of type IEnumerable<Int32>.");
        }

        [Fact]
        public async Task QueryAsync_Normally_AllHappens()
        {
            var testable = new SqlDatabaseSession(_sqlContext.Object, _logger);

            await testable.QueryAsync<int>("SELECT Ids FROM Cable WHERE Name = @name", new { name = "Yop" });

            testable.ExecutionTime.Should().Be(new TimeSpan(0, 0, 0, 0, 386)); // Gets passed from internal dependency.
            _sqlContext.Verify(m => m.ExecuteSql(It.IsAny<Func<IDbTransaction, Task<IEnumerable<int>>>>()), Times.Once);
            _logger.LoggedMessages.Should().HaveCount(1);
            _logger.LoggedMessages[0]
                .Should()
                .Be("TRACE: QueryAsync<T>(SELECT Ids FROM Cable WHERE Name = @name) called with expected return of type IEnumerable<Int32>.");
        }

        [Fact]
        public void QueryFirstOrDefault_Normally_AllHappens()
        {
            var testable = new SqlDatabaseSession(_sqlContext.Object, _logger);

            testable.QueryFirstOrDefault<int>("SELECT Label FROM Cable WHERE Id = @id", new { id = 12 });

            testable.ExecutionTime.Should().Be(new TimeSpan(0, 0, 0, 0, 386)); // Gets passed from internal dependency.
            _sqlContext.Verify(m => m.ExecuteSql(It.IsAny<Func<IDbTransaction, int>>()), Times.Once);
            _logger.LoggedMessages.Should().HaveCount(1);
            _logger.LoggedMessages[0]
                .Should()
                .Be("TRACE: QueryFirstOrDefault<T>(SELECT Label FROM Cable WHERE Id = @id) called with expected return of type Int32.");
        }

        [Fact]
        public async Task QueryFirstOrDefaultAsync_Normally_AllHappens()
        {
            var testable = new SqlDatabaseSession(_sqlContext.Object, _logger);

            await testable.QueryFirstOrDefaultAsync<int>("SELECT Label FROM Cable WHERE Id = @id", new { id = 12 });

            testable.ExecutionTime.Should().Be(new TimeSpan(0, 0, 0, 0, 386)); // Gets passed from internal dependency.
            _sqlContext.Verify(m => m.ExecuteSql(It.IsAny<Func<IDbTransaction, Task<int>>>()), Times.Once);
            _logger.LoggedMessages.Should().HaveCount(1);
            _logger.LoggedMessages[0]
                .Should()
                .Be("TRACE: QueryFirstOrDefaultAsync<T>(SELECT Label FROM Cable WHERE Id = @id) called with expected return of type Int32.");
        }

        [Fact]
        public void Execute_Normally_AllHappens()
        {
            var testable = new SqlDatabaseSession(_sqlContext.Object, _logger);

            testable.Execute("DELETE FROM Cable WHERE Id = @id", new { id = 12 });

            testable.ExecutionTime.Should().Be(new TimeSpan(0, 0, 0, 0, 386)); // Gets passed from internal dependency.
            _sqlContext.Verify(m => m.ExecuteSql(It.IsAny<Func<IDbTransaction, int>>()), Times.Once);
            _logger.LoggedMessages.Should().HaveCount(1);
            _logger.LoggedMessages[0].Should().Be("TRACE: Execute(DELETE FROM Cable WHERE Id = @id) issued for SqlDatabaseSession.");
        }

        [Fact]
        public async Task ExecuteAsync_Normally_AllHappens()
        {
            var testable = new SqlDatabaseSession(_sqlContext.Object, _logger);

            await testable.ExecuteAsync("DELETE FROM Cable WHERE Id = @id", new { id = 12 });

            testable.ExecutionTime.Should().Be(new TimeSpan(0, 0, 0, 0, 386)); // Gets passed from internal dependency.
            _sqlContext.Verify(m => m.ExecuteSql(It.IsAny<Func<IDbTransaction, Task<int>>>()), Times.Once);
            _logger.LoggedMessages.Should().HaveCount(1);
            _logger.LoggedMessages[0].Should().Be("TRACE: ExecuteAsync(DELETE FROM Cable WHERE Id = @id) issued for SqlDatabaseSession.");
        }

        [Fact]
        public void Execute_Typed_AllHappens()
        {
            var testable = new SqlDatabaseSession(_sqlContext.Object, _logger);

            string result = testable.Execute<string>("SELECT label FROM Somewhere WHERE Id = @id", new { id = 12 });

            testable.ExecutionTime.Should().Be(new TimeSpan(0, 0, 0, 0, 386)); // Gets passed from internal dependency.
            _sqlContext.Verify(m => m.ExecuteSql<object>(It.IsAny<Func<IDbTransaction, object>>()), Times.Once);
            _logger.LoggedMessages.Should().HaveCount(1);
            _logger.LoggedMessages[0]
                .Should()
                .Be("TRACE: Execute<T>(SELECT label FROM Somewhere WHERE Id = @id) called with expected return of type String.");
        }

        [Fact]
        public async Task ExecuteAsync_Typed_AllHappens()
        {
            var testable = new SqlDatabaseSession(_sqlContext.Object, _logger);

            string result = await testable.ExecuteAsync<string>("SELECT label FROM Somewhere WHERE Id = @id", new { id = 12 });

            testable.ExecutionTime.Should().Be(new TimeSpan(0, 0, 0, 0, 386)); // Gets passed from internal dependency.
            _sqlContext.Verify(m => m.ExecuteSql<string>(It.IsAny<Func<IDbTransaction, Task<string>>>()), Times.Once);
            _logger.LoggedMessages.Should().HaveCount(1);
            _logger.LoggedMessages[0]
                .Should()
                .Be("TRACE: Execute<T>(SELECT label FROM Somewhere WHERE Id = @id) called with expected return of type String.");
        }

        [Fact]
        public void CommitTransaction_Forced_IsPassedToContext()
        {
            var testable = new SqlDatabaseSession(_sqlContext.Object, _logger);

            testable.CommitTransaction();
            _sqlContext.Verify(m => m.CommitTransaction(false), Times.Once);
        }

        [Fact]
        public void RollbackTransaction_Forced_IsPassedToContext()
        {
            var testable = new SqlDatabaseSession(_sqlContext.Object, _logger);

            testable.RollbackTransaction();
            _sqlContext.Verify(m => m.RollbackTransaction(false), Times.Once);
        }
    }
}
