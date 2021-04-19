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
        Task ExecuteAsync(IDatabaseSession session);
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
        Task<T> ExecuteAsync(IDatabaseSession session);
    }
#pragma warning restore RCS1060 // Declare each type in separate file.
}
