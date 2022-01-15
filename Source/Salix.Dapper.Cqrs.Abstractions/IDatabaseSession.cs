using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Salix.Dapper.Cqrs.Abstractions
{
    /// <summary>
    /// Database session object, used by <see cref="CommandQueryContext"/> which is passed as parameter into "Execute" method of
    /// <see cref="IQuery{T}"/> or <see cref="ICommand"/> implementations and used there to use methods in this class for actual data manipulation.
    /// </summary>
    public interface IDatabaseSession
    {
        /// <summary>
        /// Stores execution time of last method call of SQL passed into <see cref="IDatabaseContext.ExecuteSql{T}(Func{System.Data.IDbTransaction, T})"/> method.
        /// Gets overridden by next called method, so store it away it needed for performance checks.
        /// </summary>
        TimeSpan ExecutionTime { get; }

        /// <summary>
        /// Executes the specified SQL Statement via Dapper through <see cref="IDatabaseContext">context</see>
        /// to get collection of typed objects (database records).
        /// Example:
        /// <code>
        /// var resultingList = session.Query{DomainObjType}("SELECT * FROM SomeDomainObjectTable WHERE group = @grp", new { grp = groupId })
        /// </code>
        /// </summary>
        /// <typeparam name="T">Domain object type (as in DDD).</typeparam>
        /// <param name="sqlQuery">Valid SQL Query statement.</param>
        /// <param name="parameter">The parameter(s) for SQL Query statement (can be anonymous object).</param>
        IEnumerable<T> Query<T>(string sqlQuery, object parameter = null);

        /// <summary>
        /// Executes the specified SQL Statement via Dapper through <see cref="IDatabaseContext">context</see>
        /// to get collection of typed objects (database records).
        /// Example:
        /// <code>
        /// var resultingList = await session.QueryAsync{DomainObjType}("SELECT * FROM SomeDomainObjectTable WHERE group = @grp", new { grp = groupId })
        /// </code>
        /// </summary>
        /// <typeparam name="T">Domain object type (as in DDD).</typeparam>
        /// <param name="sqlQuery">Valid SQL Query statement.</param>
        /// <param name="parameter">The parameter(s) for SQL Query statement (can be anonymous object).</param>
        /// <param name="cancellationToken">Operation cancellation token.</param>
        Task<IEnumerable<T>> QueryAsync<T>(string sqlQuery, object parameter = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Executes the specified SQL Statement via Dapper through <see cref="IDatabaseContext">context</see>
        /// to get single typed object (single database record or record field(s)).
        /// Example:
        /// <code>
        /// var result = session.QueryFirstOrDefault{DomainObjType}("SELECT field1, field2 FROM DomainObjectTable WHERE fieldId = @id", new { id = paramId })
        /// </code>
        /// </summary>
        /// <typeparam name="T">Domain object type (as in DDD).</typeparam>
        /// <param name="sqlQuery">Valid SQL Query statement.</param>
        /// <param name="parameter">The parameter(s) for SQL Query statement (can be anonymous object).</param>
        T QueryFirstOrDefault<T>(string sqlQuery, object parameter = null);

        /// <summary>
        /// Executes the specified SQL Statement via Dapper through <see cref="IDatabaseContext">context</see>
        /// to get single typed object (single database record or record field(s)).
        /// Example:
        /// <code>
        /// var result = await session.QueryFirstOrDefaultAsync{DomainObjType}("SELECT field1, field2 FROM DomainObjectTable WHERE fieldId = @id", new { id = paramId })
        /// </code>
        /// </summary>
        /// <typeparam name="T">Domain object type (as in DDD).</typeparam>
        /// <param name="sqlQuery">Valid SQL Query statement.</param>
        /// <param name="parameter">The parameter(s) for SQL Query statement (can be anonymous object).</param>
        /// <param name="cancellationToken">Operation cancellation token.</param>
        Task<T> QueryFirstOrDefaultAsync<T>(string sqlQuery, object parameter = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Executes the specified SQL Statement via Dapper through <see cref="IDatabaseContext">context</see>.
        /// Usually statement types are INSERT, UPDATE and DELETE and similar, which are not expected to return any result.
        /// Example:
        /// <code>
        /// session.Execute("DELETE FROM DomainObjectTable WHERE fieldId = @id", new { id = paramId })
        /// </code>
        /// </summary>
        /// <param name="sql">Valid SQL statement.</param>
        /// <param name="parameter">The parameter(s) for SQL Query statement (can be anonymous object).</param>
        void Execute(string sql, object parameter = null);

        /// <summary>
        /// Executes the specified SQL Statement via Dapper through <see cref="IDatabaseContext">context</see>.
        /// Returns result from passed statement.
        /// General practice should be to use <see cref="Query{T}(string, object)"/> or <see cref="QueryFirstOrDefault{T}(string, object)"/> instead of this.
        /// Example:
        /// <code>
        /// var result = session.Execute("SELECT something FROM DomainObjectTable WHERE fieldId = @id", new { id = paramId })
        /// </code>
        /// </summary>
        /// <typeparam name="T">Domain object type (as in DDD).</typeparam>
        /// <param name="sql">Valid SQL statement.</param>
        /// <param name="parameter">The parameter(s) for SQL Query statement (can be anonymous object).</param>
        T Execute<T>(string sql, object parameter);

        /// <summary>
        /// Executes the specified SQL statement via Dapper through context.
        /// Example in Command object:
        /// <code>
        /// await session.ExecuteAsync("UPDATE SomeTable SET field = @val", new { val = variable });
        /// </code>
        /// </summary>
        /// <param name="sql">The SQL statement.</param>
        /// <param name="parameter">The parameter(s) for SQL statement (can be anonymous object).</param>
        /// <param name="cancellationToken">Operation cancellation token.</param>
        Task ExecuteAsync(string sql, object parameter = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Executes the specified SQL statement via Dapper through context.
        /// Example in Command object:
        /// <code>
        /// var result = await session.ExecuteAsync{int}("UPDATE SomeTable SET field = @val", new { val = variable });
        /// </code>
        /// </summary>
        /// <typeparam name="T">Type of return value.</typeparam>
        /// <param name="sql">The SQL statement.</param>
        /// <param name="parameter">The parameter(s) for SQL statement (can be anonymous object).</param>
        /// <param name="cancellationToken">Operation cancellation token.</param>
        Task<T> ExecuteAsync<T>(string sql, object parameter = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Explicitly Rollbacks the database transaction to rollback any transaction
        /// during exception handling in external dependency calls (Integrations, External API etc. exceptions).
        /// Also usable for integration testing when created data is not desirable to leave in database.
        /// NOTE: Transaction is rollback-ed automatically when there is exception caught in logic with database connection involved.
        /// NOTE: After Transaction Rollback, a new transaction is created on existing connection. If you have to complete business operation,
        /// make sure you close connection as soon as possible to avoid partial database updates.
        /// </summary>
        void RollbackTransaction();

        /// <summary>
        /// Explicitly Commits existing transaction, saving and making data available to queries outside this business session.
        /// Normally you should not call this method, as it is automatically called when business session (service request) ends.
        /// Could be used in some external integration or other service calls within business operation making new data available to them.
        /// NOTE: If you explicitly call this method and later business operation fails -
        /// you have to make sure data saved by this transaction gets removed (depending on business functionality requirements).
        /// NOTE: After Transaction Commit, a new transaction is created on existing connection to continue serve business operation
        /// (and commit at the end of it automatically).
        /// </summary>
        void CommitTransaction();

        /// <summary>
        /// Creates the properly aligned with database engine instance of dynamic parameters collection
        /// to be used as input into Stored Procedure Queries and Executions.
        /// </summary>
        IDynamicParameters CreateDynamicParameters();
    }
}
