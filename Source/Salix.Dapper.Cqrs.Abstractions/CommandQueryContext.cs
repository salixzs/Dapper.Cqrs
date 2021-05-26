using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace Salix.Dapper.Cqrs.Abstractions
{
    /// <inheritdoc cref="ICommandQueryContext"/>
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class CommandQueryContext : ICommandQueryContext
    {
        private readonly IDatabaseSession _databaseSession;

        /// <summary>
        /// CQRS abstraction for database operations. This interface implementation gets injected into Logic class.
        /// Logic takes this injected Database context implementation and provides <see cref="IQuery{T}"/> and <see cref="ICommand"/>
        /// implementations as parameter for these methods.
        /// </summary>
        /// <param name="databaseSession">The Database Session object.</param>
        public CommandQueryContext(IDatabaseSession databaseSession) => _databaseSession = databaseSession;

        /// <inheritdoc/>
        public async Task<T> QueryAsync<T>(IQuery<T> sqlQuery) => await sqlQuery.ExecuteAsync(_databaseSession);

        /// <inheritdoc/>
        public async Task ExecuteAsync(ICommand command) => await command.ExecuteAsync(_databaseSession);

        /// <inheritdoc/>
        public async Task<T> ExecuteAsync<T>(ICommand<T> command) => await command.ExecuteAsync(_databaseSession);

        /// <inheritdoc/>
        public T Query<T>(IQuery<T> sqlQuery) => sqlQuery.Execute(_databaseSession);

        /// <inheritdoc/>
        public void Execute(ICommand command) => command.Execute(_databaseSession);

        /// <inheritdoc/>
        public T Execute<T>(ICommand<T> command) => command.Execute(_databaseSession);

        /// <inheritdoc/>
        public void CommitTransaction() => _databaseSession.CommitTransaction();

        /// <inheritdoc/>
        public void RollbackTransaction() => _databaseSession.RollbackTransaction();

        /// <inheritdoc/>
        public TimeSpan ExecutionTime => _databaseSession.ExecutionTime;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        [ExcludeFromCodeCoverage]
        private string DebuggerDisplay => "CQRS context on " + _databaseSession.ToString();
    }
}
