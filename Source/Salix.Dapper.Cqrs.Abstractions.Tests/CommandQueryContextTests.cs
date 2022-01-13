using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Xunit;

namespace Salix.Dapper.Cqrs.Abstractions.Tests
{
    /// <summary>
    /// Testing <see cref="CommandQueryContext" /> implementation.
    /// As methods does not have logic, tests are making sure necessary dependences are called and logged properly.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class CommandQueryContextTests
    {
        private readonly Mock<IDatabaseSession> _sqlSession;
        private readonly CommandQueryContext _testable;

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandQueryContextTests"/> class.
        /// This happens for each separate test (NOT once for all tests in class).
        /// </summary>
        public CommandQueryContextTests()
        {
            _sqlSession = new Mock<IDatabaseSession>();
            _testable = new CommandQueryContext(_sqlSession.Object);
        }

        [Fact]
        public async Task Query_SessionAndQuery_QueryExecutedWithSession()
        {
            var query = new Mock<IQuery<int>>();
            await _testable.QueryAsync(query.Object);
            query.Verify(q => q.ExecuteAsync(_sqlSession.Object, CancellationToken.None), Times.Once);
        }

        [Fact]
        public async Task Execute_SessionAndCommand_CommandExecutedWithSession()
        {
            var command = new Mock<ICommand>();
            await _testable.ExecuteAsync(command.Object);
            command.Verify(q => q.ExecuteAsync(_sqlSession.Object, CancellationToken.None), Times.Once);
        }

        [Fact]
        public async Task Execute_SessionAndTypedCommand_CommandExecutedWithSession()
        {
            var command = new Mock<ICommand<string>>();
            string _ = await _testable.ExecuteAsync(command.Object);
            command.Verify(q => q.ExecuteAsync(_sqlSession.Object, CancellationToken.None), Times.Once);
        }

        [Fact]
        public void CommitTransaction_Calling_PassedToSession()
        {
            _testable.CommitTransaction();
            _sqlSession.Verify(q => q.CommitTransaction(), Times.Once);
        }

        [Fact]
        public void RollbackTransaction_Calling_PassedToSession()
        {
            _testable.RollbackTransaction();
            _sqlSession.Verify(q => q.RollbackTransaction(), Times.Once);
        }
    }
}
