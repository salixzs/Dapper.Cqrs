using System.Threading;
using System.Threading.Tasks;

namespace Salix.Dapper.Cqrs.Abstractions
{
#pragma warning disable RCS1060 // Declare each type in separate file.
    /// <summary>
    /// Command Interface for CQRS pattern "C" part.
    /// This is actual work handler for saving/updating/deleting data in database for executing non-returning statements.
    /// </summary>
    public interface ICommand
    {
        /// <summary>
        /// Executes the specified SQL statement(s) onto database session.
        /// </summary>
        /// <param name="session">The database connection session.</param>
        /// <param name="cancellationToken">Operation cancellation token.</param>
        Task ExecuteAsync(IDatabaseSession session, CancellationToken cancellationToken = default);

        /// <summary>
        /// Executes the specified SQL statement(s) onto database session and returns data.
        /// </summary>
        /// <param name="session">The database connection session.</param>
        void Execute(IDatabaseSession session);
    }

    /// <summary>
    /// Command Interface for CQRS pattern "C" part.
    /// This is actual work handler for saving/updating/deleting data in database.
    /// This can be used for executing statements which returns some data.
    /// </summary>
    /// <typeparam name="T">Type of return data. Usually to return inserted row ID for autoincrement fields.</typeparam>
    public interface ICommand<T>
    {
        /// <summary>
        /// Executes the specified SQL statement onto session.
        /// </summary>
        /// <param name="session">The database connection session.</param>
        /// <param name="cancellationToken">Operation cancellation token.</param>
        Task<T> ExecuteAsync(IDatabaseSession session, CancellationToken cancellationToken);

        /// <summary>
        /// Executes the specified SQL statement(s) onto database session and returns data.
        /// </summary>
        /// <param name="session">The database connection session.</param>
        /// <param name="cancellationToken">Operation cancellation token.</param>
        T Execute(IDatabaseSession session);
    }
#pragma warning restore RCS1060 // Declare each type in separate file.
}
