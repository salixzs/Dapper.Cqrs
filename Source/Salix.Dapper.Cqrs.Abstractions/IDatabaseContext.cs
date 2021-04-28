using System;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;

namespace Salix.Dapper.Cqrs.Abstractions
{
    public interface IDatabaseContext : IDisposable
    {
        /// <summary>
        /// Access to actual Database (Engine) Connection object.
        /// Gets created if not exist when accessed.
        /// Gets Open when accessed (if closed).
        /// Creates transaction on it, if it does not exist.
        /// Do not dispose it separately, use entire context dispose when Unit of Work completes.
        /// </summary>
        DbConnection Connection { get; }

        /// <summary>
        /// Stores execution time of last statement, passed to <see cref="ExecuteSql{T}(Func{T}, string)"/> method.
        /// </summary>
        TimeSpan ExecutionTime { get; }

        /// <summary>
        /// Get current active transaction. Trying to retrieve this value will NOT initiate SqlContext object.
        /// This is intended to be used only when constructing manual SQL commands, which is *NOT STANDARD* use case for this class.
        /// </summary>
        IDbTransaction Transaction { get; }

        /// <summary>
        /// Explicitly Rollbacks the database transaction. This is provided to rollback any transaction
        /// during exception handling in non-database calls.
        /// Also usable for integration testing when created data is not desirable to leave in database.
        /// NOTE: Should handle Rollback when application throws exception.
        /// </summary>
        /// <param name="isFinal">False - will reopen new transaction after rollbacking current one. True - Rollback current transaction and do not open new one. Default = false.</param>
        void RollbackTransaction(bool isFinal = false);

        /// <summary>
        /// Explicitly Commits existing transaction, saving and making data available to queries outside this business session.
        /// Normally you should not call this method, as it is automatically called when business session (service request) ends - this context gets disposed..
        /// Could be used in some external integration or other service calls within business operation making new data available to them.
        /// NOTE: If you explicitly call this method and later business operation fails -
        /// you have to make sure data saved by this transaction gets removed (depending on business functionality requirements).
        /// </summary>
        /// <param name="isFinal">False - will reopen new transaction after committing current one. True - Commits current transaction and do not open new one. Default = false.</param>
        void CommitTransaction(bool isFinal = false);

        /// <summary>
        /// Close/Release/Nullify transaction (with Commit) and database connection.
        /// Implemented this way due to DA tests does not have OutputHelper available in Dispose methods.
        /// See https://github.com/xunit/xunit/issues/565 .
        /// So this method is mainly for unit-testing, where you should call these in order:
        /// 1) RollbackTransaction();
        /// 2) ReleaseConnection(false);
        /// 3) [Called by runtime] - Dispose();
        /// </summary>
        void ReleaseConnection();

        /// <summary>
        /// Executes given Query, regardless of its type (SELECT, INSERT, UPDATE, DELETE, EXECUTE etc.).
        /// Example:
        /// <code>
        /// string document = sqlContext.ExecuteSql(() =&gt; sqlContext.Connection.QueryFirstOrDefault{string}("SELECT * FROM Document WHERE DocumentId=1219"));
        /// </code>
        /// NOTE: This usually should be used through <see cref="IDatabaseSession"/> implementation class helper methods.
        /// </summary>
        /// <typeparam name="T">Domain (as in DDD) object.</typeparam>
        /// <param name="sqlStatement">The function, usually incorporating SQL statement.</param>
        T ExecuteSql<T>(Func<IDbTransaction, T> sqlStatement);


        /// <summary>
        /// Executes given Query, regardless of its type (SELECT, INSERT, UPDATE, DELETE, EXECUTE etc.).
        /// Example:
        /// <code>
        /// string document = await sqlContext.ExecuteSql(() =&gt; sqlContext.Connection.QueryFirstOrDefaultAsync{string}("SELECT * FROM Document WHERE DocumentId=1219"));
        /// </code>
        /// NOTE: This usually should be used through <see cref="IDatabaseSession"/> implementation class helper methods.
        /// </summary>
        /// <typeparam name="T">Domain (as in DDD) object.</typeparam>
        /// <param name="sqlStatement">The function, usually incorporating SQL statement.</param>
        Task<T> ExecuteSql<T>(Func<IDbTransaction, Task<T>> sqlStatement);
    }
}
