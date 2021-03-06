using System.Threading;
using System.Threading.Tasks;

namespace Salix.Dapper.Cqrs.Abstractions
{
    /// <summary>
    /// Base class for <see cref="ICommand"/> implementations.
    /// Provides standard methods Execute and ExecuteAsync (using SqlStatement + Parameters properties).
    /// </summary>
    [System.Diagnostics.DebuggerDisplay("{DebuggerDisplay,nq}")]
    public abstract class MsSqlCommandBase : MsSqlCommandValidatorBase, ICommand
    {
        /// <summary>
        /// Executes the <see cref="MsSqlCommandValidatorBase.SqlStatement"/> onto SQL Server Session asynchronously.
        /// </summary>
        /// <param name="session">The MS SQL connection session.</param>
        /// <param name="cancellationToken">Asynchronous operation cancellation token.</param>
        public virtual async Task ExecuteAsync(IDatabaseSession session, CancellationToken cancellationToken) => await session.ExecuteAsync(this.SqlStatement, this.Parameters, cancellationToken);

        /// <summary>
        /// Executes the <see cref="MsSqlCommandValidatorBase.SqlStatement"/> onto SQL Server Session synchronously.
        /// </summary>
        /// <param name="session">The MS SQL connection session.</param>
        public virtual void Execute(IDatabaseSession session) => session.Execute(this.SqlStatement, this.Parameters);
    }

    /// <summary>
    /// Base class for <see cref="ICommand{T}"/> implementations returning some data.
    /// Provides standard methods Execute and ExecuteAsync (using SqlStatement + Parameters properties).
    /// </summary>
    [System.Diagnostics.DebuggerDisplay("{DebuggerDisplay,nq}")]
    public abstract class MsSqlCommandBase<T> : MsSqlCommandValidatorBase, ICommand<T>
    {
        /// <summary>
        /// Executes the <see cref="MsSqlCommandValidatorBase.SqlStatement"/> onto SQL Server Session asynchronously.
        /// </summary>
        /// <param name="session">The MS SQL connection session.</param>
        /// <param name="cancellationToken">Asynchronous operation cancellation token.</param>
        public virtual async Task<T> ExecuteAsync(IDatabaseSession session, CancellationToken cancellationToken) => await session.ExecuteAsync<T>(this.SqlStatement, this.Parameters, cancellationToken);

        /// <summary>
        /// Executes the <see cref="MsSqlCommandValidatorBase.SqlStatement"/> onto SQL Server Session synchronously.
        /// </summary>
        /// <param name="session">The MS SQL connection session.</param>
        public virtual T Execute(IDatabaseSession session) => session.Execute<T>(this.SqlStatement, this.Parameters);
    }
}
