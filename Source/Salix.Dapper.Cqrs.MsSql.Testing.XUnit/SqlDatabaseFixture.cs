using System;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using Moq;
using Salix.Dapper.Cqrs.Abstractions;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Salix.Dapper.Cqrs.MsSql.Testing.XUnit
{
    /// <summary>
    /// Database related tests fixture providing common context and functionality.
    /// Note: SQL connection is reused within all tests in single test class (or for all classes marked in collection).
    /// Database connection is Commit after all tests are run. <seealso cref="RollbackTransaction"/> and <seealso cref="ReopenTransaction"/>
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class SqlDatabaseFixture : IDisposable
    {
        private readonly IMessageSink _messageSink;
        private IMsSqlContext _sqlContext;
        private ILogger<DatabaseContext> _sqlContextLogger;
        private ILogger<SqlDatabaseSession> _sqlSessionLogger;

        public string SqlConnection { get; set; }

        /// <summary>
        /// Access to database Session object.
        /// </summary>
        public IDatabaseSession SqlSession { get; private set; }

        /// <summary>
        /// Access to MS SQL Dapper command/query object.
        /// </summary>
        public ICommandQueryContext Db { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SqlDatabaseFixture" /> class.
        /// </summary>
        /// <param name="messageSink">
        /// The message sink to output diagnostic messages.
        /// To make it work, you have to enable it for XUnit runner (see xunit.runner.json file).
        /// </param>
        /// <remarks>
        /// Creating DB connection in constructor is important as constructor is called once per test class.
        /// For Test Collections constructor is called once for all tests in this collection.
        /// </remarks>
        public SqlDatabaseFixture(IMessageSink messageSink)
        {
            _messageSink = messageSink;
            _messageSink.OnMessage(new DiagnosticMessage("SqlTestsFixture initialization (should happen once per all database tests)."));
        }

        /// <summary>
        /// Instantiates the database and internal logger objects for tests.
        /// </summary>
        public void InstantiateDatabaseObjects(ITestOutputHelper output)
        {
            // Entry by next tests after first - reuse everything already set
            if (_sqlContext != null && _sqlContext.Connection.State == ConnectionState.Open)
            {
                return;
            }

            if (_messageSink == null)
            {
                _sqlContextLogger = new Mock<ILogger<DatabaseContext>>().Object;
                _sqlSessionLogger = new Mock<ILogger<SqlDatabaseSession>>().Object;
            }
            else
            {
                _sqlContextLogger = new XUnitLogger<DatabaseContext>(_messageSink).SetOutputHelper(output);
                _sqlSessionLogger = new XUnitLogger<SqlDatabaseSession>(_messageSink).SetOutputHelper(output);
            }

            _sqlContext = new DatabaseContext(this.SqlConnection, _sqlContextLogger);
            this.SqlSession = new SqlDatabaseSession(_sqlContext, _sqlSessionLogger);
            this.Db = new CommandQueryContext(this.SqlSession);
        }

        /// <summary>
        /// Does rollback of the SQL transaction.
        /// Note: Transaction is explicitly open during first use of SQL connection.
        /// It is reused for all tests within test-scope (single test class), so if you rollback this transaction -
        /// reopen it again for other tests.
        /// </summary>
        public void RollbackTransaction() => _sqlContext.RollbackTransaction();

        /// <summary>
        /// Reopens the transaction if it was rollback in other tests.
        /// Should be used in classes when <seealso cref="RollbackTransaction"/> is used.
        /// Should be used by all tests - in begin of test even if only one test is using <seealso cref="RollbackTransaction"/>
        /// as test run order is random within test class (except cases when only one test exists per test class).
        /// </summary>
        public void ReopenTransaction()
        {
            if (_sqlContext.Transaction != null)
            {
                _sqlContext.RollbackTransaction();
            }
        }

        /// <summary>
        /// Outputs the message to console from test.
        /// </summary>
        /// <param name="message">The log entry to be displayed in standard output.</param>
        public void WriteOutput(string message)
        {
            if (string.IsNullOrEmpty(message))
            {
                return;
            }

            _sqlContextLogger.LogDebug(message);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// Disposes DapperContext, which closes SQL transaction (commits it!!!) and connection after test class is finished executing tests
        /// </summary>
        public void Dispose()
        {
            _sqlContextLogger.LogDebug("Disposing Database Fixture...");
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposeManaged"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        internal void Dispose(bool disposeManaged)
        {
            if (!disposeManaged)
            {
                return;
            }

            _sqlContext.ReleaseConnection();
            _sqlContext.Dispose();
        }
    }
}
