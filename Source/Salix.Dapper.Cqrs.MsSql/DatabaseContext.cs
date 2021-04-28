using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Salix.Dapper.Cqrs.Abstractions;

namespace Salix.Dapper.Cqrs.MsSql
{
    /// <inheritdoc cref="IMsSqlContext"/>
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public sealed class DatabaseContext : IMsSqlContext
    {
        private readonly string _connectionString;
        private readonly ILogger<DatabaseContext> _logger;
        private SqlConnection _connection;

        /// <inheritdoc cref="IMsSqlContext.Connection"/>
        public SqlConnection Connection
        {
            get
            {
                this.EnsureOpenConnection();
                this.EnsureOpenTransaction();
                return _connection;
            }
        }

        /// <inheritdoc/>
        DbConnection IDatabaseContext.Connection => this.Connection;

        /// <summary>
        /// Makes sure SQL connection is Created and Open.
        /// </summary>
        private void EnsureOpenConnection()
        {
            if (_connection == null)
            {
                _connection = new SqlConnection(_connectionString);
                _logger.LogTrace("Created new MS SQL connection with Hash: {Hash} to {Server}:{Database}", _connection.GetHashCode(), _connection.DataSource, _connection.Database);
                if (_logger.IsEnabled(LogLevel.Trace))
                {
                    _connection.InfoMessage += new SqlInfoMessageEventHandler(this.OnInfoMessage);
                    _connection.StateChange += new StateChangeEventHandler(this.OnStateChange);
                    _connection.Disposed += this.OnDisposed;
                }
            }

            if (_connection.State == ConnectionState.Closed)
            {
                // This happens if Connection property of this context is disposed (used in USING statement)
                if (string.IsNullOrEmpty(_connection.ConnectionString))
                {
                    _connection.ConnectionString = _connectionString;
                }

                _logger.LogTrace("Opening SQL connection (Hash: {Hash}).", _connection.GetHashCode());
                var counter = Stopwatch.StartNew();
                _connection.Open();
                counter.Stop();
                _logger.LogDebug("Connection to {DatabaseName} opened in {ConnOpenTime:l} (Hash: {ConnHash}).", _connection.Database, counter.Elapsed.ToHumanReadableString(), _connection.GetHashCode());
            }
        }

        /// <summary>
        /// Event Handler on Disposed for SQL connection. Used only when Logging is set to Trace (verbose).
        /// </summary>
        private void OnDisposed(object sender, EventArgs eventArgs) =>
            _logger.LogTrace("Event: MS SQL Connection got disposed");

        /// <summary>
        /// Event Handler for SQL connection State change event. Used only when Logging is set to Trace (verbose).
        /// </summary>
        private void OnStateChange(object sender, StateChangeEventArgs eventArgs) =>
            _logger.LogTrace("Event: MS SQL Connection StateChange ({OriginalState} => {CurrentState})", eventArgs.OriginalState, eventArgs.CurrentState);

        /// <summary>
        /// Event Handler for SQL connection InfoMessage event. Used only when Logging is set to Trace (verbose).
        /// </summary>
        private void OnInfoMessage(object sender, SqlInfoMessageEventArgs eventArgs)
        {
            _logger.LogTrace("MS SQL InfoEvent fired for {SqlInfoEventSource} with message {SqlInfoEventMessage}", eventArgs.Source, eventArgs.Message);
            foreach (SqlError err in eventArgs.Errors)
            {
                _logger.LogTrace(
                    "The {0} has received a severity {1}, state {2} error number {3} on line {4} of procedure {5} on server {6}:{7}",
                        err.Source,
                        err.Class,
                        err.State,
                        err.Number,
                        err.LineNumber,
                        err.Procedure,
                        err.Server,
                        err.Message);
            }
        }

        /// <inheritdoc/>
        public IDbTransaction Transaction { private set; get; }

        /// <summary>
        /// Makes sure there is open transaction on connection.
        /// </summary>
        /// <remarks>
        /// There are 4 main transaction isolation levels:
        /// 1. READ COMMITTED: This is the default setting for most SQL Server queries.It defines that a transaction within the current session
        ///    cannot read data that has been modified by another transaction. For this reason, dirty reads are prevented when this setting is turned on.
        /// 2. READ UNCOMMITTED: This says that a transaction within the current session can read data that has been modified or deleted by another transaction
        ///    but not yet committed. This imposes the least restrictions of isolation levels as the database engine doesn't issue any shared locks.
        ///    As a result of this, it is highly likely that the transaction will end up reading data that has been inserted, updated, or deleted but never committed to the database.
        ///    Such a scenario is known as dirty reads.
        /// 3. REPEATABLE READ: In this setting, a transaction not only can read data that is modified by another transaction that has been committed
        ///    but also imposes a restriction that no other transaction can modify the data that is being read until the first transaction completes.
        ///    This eliminates the condition of non-repeatable reads.
        /// 4. SERIALIZABLE: There are multiple properties that are being set by this isolation level.
        ///    This isolation level is the most restrictive as compared to the others, thus there might be some performance issues with this.
        ///    The properties are mentioned as below:
        ///      - The current transaction can only read data modified by other transaction that has been committed.
        ///      - Other transactions have to wait in queue until the execution of the first transaction is completed.
        ///      - No other transactions are allowed to insert data, which matches the condition of the current transaction.
        /// </remarks>
        /// <param name="isolationLevel">The isolation level for transaction. Defaults to ReadCommited.</param>
        private void EnsureOpenTransaction(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            if (this.Transaction != null)
            {
                return;
            }

            this.Transaction = _connection.BeginTransaction(isolationLevel);
            _logger.LogTrace("Created new SQL transaction with Hash: {Hash} with Isolation level {IsolationLevel} onto connection {ConnHash}.", this.Transaction.GetHashCode(), this.Transaction.IsolationLevel.ToString(), this.Transaction.Connection.GetHashCode());
        }

        /// <inheritdoc/>
        public TimeSpan ExecutionTime { get; private set; }

        /// <summary>
        /// Creates SQL database Context to use for CQRS functionalities to access database through Dapper ORM.
        /// Used in <see cref="IDatabaseSession"/> implementation methods to do lowest level
        /// Dapper executions against database, using DB connection provided here.
        /// </summary>
        /// <param name="connectionString">The SQL connection string from configuration.</param>
        /// <param name="logger">The logger implementation object to issue logging statements.</param>
        public DatabaseContext(string connectionString, ILogger<DatabaseContext> logger)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentNullException(nameof(connectionString), "SQL Database Context object did not receive SQL Connection string during its construction.");
            }

            _connectionString = connectionString;
            _logger = logger;
        }

        /// <inheritdoc/>
        public T ExecuteSql<T>(Func<IDbTransaction, T> sqlStatement)
        {
            try
            {
                this.EnsureOpenConnection();
                this.EnsureOpenTransaction();
                _logger.LogTrace($"Attempting to execute SQL statement (ExecuteSql).");
                var counter = Stopwatch.StartNew();
                T queryResult = sqlStatement(this.Transaction);
                counter.Stop();
                _logger.LogDebug($"SQL statement executed in {counter.Elapsed.ToHumanReadableString()}.");
                this.ExecutionTime = counter.Elapsed;
                return queryResult;
            }
            catch (SqlException ex)
            {
                _logger.LogDebug($"SQL Transaction Rollback due to exception thrown in DatabaseContext.ExecuteSql. {ex.Message}");
                if (this.Transaction != null)
                {
                    this.Transaction.Rollback();
                    this.Transaction.Dispose();
                    this.Transaction = null;
                    this.EnsureOpenConnection();
                    this.EnsureOpenTransaction();
                }

                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<T> ExecuteSql<T>(Func<IDbTransaction, Task<T>> sqlStatement)
        {
            try
            {
                this.EnsureOpenConnection();
                this.EnsureOpenTransaction();
                _logger.LogTrace($"Attempting to execute SQL statement (ExecuteSql).");
                var counter = Stopwatch.StartNew();
                T queryResult = await sqlStatement(this.Transaction);
                counter.Stop();
                _logger.LogDebug($"SQL statement executed in {counter.Elapsed.ToHumanReadableString()}.");
                this.ExecutionTime = counter.Elapsed;
                return queryResult;
            }
            catch (SqlException ex)
            {
                _logger.LogDebug($"SQL Transaction Rollback due to exception thrown in DatabaseContext.ExecuteSql. {ex.Message}");
                if (this.Transaction != null)
                {
                    this.Transaction.Rollback();
                    this.Transaction.Dispose();
                    this.Transaction = null;
                    this.EnsureOpenConnection();
                    this.EnsureOpenTransaction();
                }

                throw;
            }
        }

        /// <inheritdoc/>
        public void CommitTransaction(bool isFinal = false)
        {
            if (this.Transaction == null)
            {
                return;
            }

            if (this.Transaction.Connection != null && this.Transaction.Connection.State == ConnectionState.Open)
            {
                _logger.LogDebug("Explicit Transaction Commit (Hash: {Hash}).", this.Transaction.GetHashCode());
                this.Transaction.Commit();
                this.Transaction.Dispose();
            }

            this.Transaction = null;
            if (!isFinal)
            {
                this.EnsureOpenConnection();
                this.EnsureOpenTransaction();
            }
        }

        /// <inheritdoc/>
        public void RollbackTransaction(bool isFinal = false)
        {
            if (this.Transaction == null)
            {
                return;
            }

            if (this.Transaction.Connection != null && this.Transaction.Connection.State == ConnectionState.Open)
            {
                _logger.LogTrace("Explicit Transaction Rollback (Hash: {Hash}).", this.Transaction.GetHashCode());
                this.Transaction.Rollback();
                this.Transaction.Dispose();
            }

            this.Transaction = null;
            if (!isFinal)
            {
                this.EnsureOpenConnection();
                this.EnsureOpenTransaction();
            }
        }

        /// <inheritdoc/>
        public void ReleaseConnection()
        {
            if (this.Transaction != null)
            {
                if (this.Transaction.Connection != null && this.Transaction.Connection.State == ConnectionState.Open)
                {
                    _logger.LogTrace("SQL Transaction (Hash: {Hash}) Commit in ReleaseConnection (business operation complete).", this.Transaction.GetHashCode());
                    this.Transaction.Commit();
                    this.Transaction.Dispose();
                }
                else
                {
                    _logger.LogTrace("SQL Transaction is already completed (Commit or Rollback) in ReleaseConnection.");
                }

                this.Transaction = null;
            }
            else
            {
                _logger.LogTrace("SQL Transaction is already NULL in ReleaseConnection.");
            }

            if (_connection != null)
            {
                if (_connection.State != ConnectionState.Closed)
                {
                    _connection.Close();
                    _logger.LogDebug("Connection closed (ReleaseConnection). (Hash: {Hash})", _connection.GetHashCode());
                }
                else
                {
                    _logger.LogTrace("SQL Connection is already Closed in ReleaseConnection operation.");
                }
            }
            else
            {
                _logger.LogTrace("SQL Connection is already NULL in ReleaseConnection.");
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose() => this.Dispose(true);

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        private void Dispose(bool disposing)
        {
            if (!disposing)
            {
                return;
            }

            this.ReleaseConnection();

            if (_connection != null)
            {
                _connection.Dispose();
            }
        }

        /// <summary>
        /// String representation of current SQL Context.
        /// </summary>
        public override string ToString()
        {
            string debugText = string.Empty;
            if (_connection != null)
            {
                debugText += $"SqlConnection: {_connection.GetHashCode().ToString(CultureInfo.InvariantCulture)} ({_connection.State.ToString().ToUpperInvariant()}) to {_connection.Database} on {_connection.DataSource}; ";
            }
            else
            {
                debugText = "Connection not open";
            }

            if (this.Transaction != null)
            {
                debugText += $"Transaction: {this.Transaction.GetHashCode().ToString(CultureInfo.InvariantCulture)}; ";
            }

            return debugText;
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string DebuggerDisplay => this.ToString();
    }
}
