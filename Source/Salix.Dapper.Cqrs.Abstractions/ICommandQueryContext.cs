using System;
using System.Threading.Tasks;

namespace Salix.Dapper.Cqrs.Abstractions
{
    /// <summary>
    /// CQRS abstraction for database operations. This interface implementation gets injected into Logic class.
    /// Logic takes this injected Database context implementation and provides <see cref="IQuery{T}"/> and <see cref="ICommand"/>
    /// implementations as parameter for these methods.
    /// </summary>
    public interface ICommandQueryContext
    {
        /// <summary>
        /// Executes Query object (in "cQrs") onto SQL database, returning Domain object(s).
        /// Example:
        /// <code>
        /// List{DomainObject} result = await cqrs.QueryAsync(new QueryInterfaceImplementation(parameter));
        /// </code>
        /// </summary>
        /// <remarks>
        /// Use Execute(<see cref="ICommand"/>) to change data in database.
        /// </remarks>
        /// <typeparam name="T">Domain (as in DDD) object.</typeparam>
        /// <param name="sqlQuery">The query class object, implementing IQuery interface.</param>
        Task<T> QueryAsync<T>(IQuery<T> sqlQuery);


        /// <summary>
        /// Executes Command object (in "Cqrs") onto SQL database, usually modifying data there.
        /// Example:
        /// <code>
        /// await cqrs.ExecuteAsync(new CommandInterfaceImplementation(parameters));
        /// </code>
        /// </summary>
        /// <remarks>
        /// Use <see cref="IQuery{T}" /> for reading data from database.
        /// </remarks>
        /// <param name="command">The command class object, implementing ICommand interface.</param>
        Task ExecuteAsync(ICommand command);

        /// <summary>
        /// Executes Command object (in "Cqrs") onto SQL database, probably modifying data there and returning data as well.
        /// Example:
        /// <code>
        /// var id = await cqrs.ExecuteAsync(new CommandInterfaceImplementation(parameters));
        /// </code>
        /// </summary>
        /// <remarks>
        /// Use <see cref="IQuery{T}" /> for reading data from database.
        /// </remarks>
        /// <typeparam name="T">Type expected to be returned by executing command.</typeparam>
        /// <param name="command">The command class object, implementing ICommand interface.</param>
        Task<T> ExecuteAsync<T>(ICommand<T> command);

        /// <summary>
        /// Executes Query object (in "cQrs") onto SQL database, returning Domain object(s).
        /// Example:
        /// <code>
        /// List{DomainObject} result = cqrs.Query(new QueryInterfaceImplementation(parameter));
        /// </code>
        /// </summary>
        /// <remarks>
        /// Use Execute(<see cref="ICommand"/>) to change data in database.
        /// </remarks>
        /// <typeparam name="T">Domain (as in DDD) object.</typeparam>
        /// <param name="sqlQuery">The query class object, implementing IQuery interface.</param>
        T Query<T>(IQuery<T> sqlQuery);


        /// <summary>
        /// Executes Command object (in "Cqrs") onto SQL database, usually modifying data there without returning any data.
        /// Example:
        /// <code>
        /// await cqrs.Execute(new CommandInterfaceImplementation(parameters));
        /// </code>
        /// </summary>
        /// <remarks>
        /// Use <see cref="IQuery{T}" /> for reading data from database.
        /// </remarks>
        /// <param name="command">The command class object, implementing ICommand interface.</param>
        void Execute(ICommand command);

        /// <summary>
        /// Executes Command object (in "Cqrs") onto SQL database, probably modifying data there and returning data as well.
        /// Example:
        /// <code>
        /// var id = cqrs.Execute(new CommandInterfaceImplementation(parameters));
        /// </code>
        /// </summary>
        /// <remarks>
        /// Use <see cref="IQuery{T}" /> for reading data from database.
        /// </remarks>
        /// <typeparam name="T">Type expected to be returned by executing command.</typeparam>
        /// <param name="command">The command class object, implementing ICommand interface.</param>
        T Execute<T>(ICommand<T> command);

        /// <summary>
        /// Explicitly Rollbacks the database transaction. This is provided to rollback any transaction
        /// during exception handling in non-database calls (External services etc. exceptions).
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
        /// Stores execution time of last method call of SQL passed into <see cref="IDatabaseContext.ExecuteSql{T}(Func{System.Data.IDbTransaction, T})"/> method.
        /// </summary>
        TimeSpan ExecutionTime { get; }
    }
}
