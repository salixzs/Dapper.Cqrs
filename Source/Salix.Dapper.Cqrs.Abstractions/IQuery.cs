using System.Threading;
using System.Threading.Tasks;

namespace Salix.Dapper.Cqrs.Abstractions
{
    /// <summary>
    /// Query interface for use in CQRS pattern "Q" part.
    /// This is actual work handler for querying data from database.
    /// </summary>
    /// <typeparam name="T">Domain (as in DDD) object type.</typeparam>
    public interface IQuery<T> : IQueryValidator
    {
        /// <summary>
        /// Actual executable method of database query which returns data from database.
        /// </summary>
        /// <param name="session">The database connection session.</param>
        /// <param name="cancellationToken">Operation cancellation token.</param>
        Task<T> ExecuteAsync(IDatabaseSession session, CancellationToken cancellationToken = default);

        /// <summary>
        /// Actual executable method of database query which returns data from database.
        /// </summary>
        /// <param name="session">The database connection session.</param>
        T Execute(IDatabaseSession session);
    }
}
