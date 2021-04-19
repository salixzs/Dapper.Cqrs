using System.Threading.Tasks;
using Salix.Dapper.Cqrs.Abstractions;
using Sample.AspNet5Api.Domain;

namespace Sample.AspNet5Api.Queries
{
    /// <summary>
    /// Retrieves an Artist from database by its ID.
    /// </summary>
    public sealed class ArtistByIdQuery : MsSqlQueryBase, IQuery<Artist>
    {
        private readonly int _objectId;

        /// <summary>
        /// Retrieves an Artist from database by its ID.
        /// </summary>
        /// <param name="objectId">Artist ID in database.</param>
        public ArtistByIdQuery(int objectId) => _objectId = objectId;

        /// <summary>
        /// Actual SQL Statement to execute against MS SQL database.
        /// </summary>
        public override string SqlStatement => @"
SELECT ArtistId,
       Name
  FROM Artist
 WHERE ArtistId = @id";

        /// <summary>
        /// Anonymous object of SqlQuery parameter(s).
        /// </summary>
        public override object Parameters => new { id = _objectId };

        /// <summary>
        /// Executes the query in <see cref="SqlStatement"/> asynchronously, using parameters in <see cref="Parameters"/>.
        /// </summary>
        /// <param name="session">The database session object, injected by IoC.</param>
        public async Task<Artist> ExecuteAsync(IDatabaseSession session)
            => await session.QueryFirstOrDefaultAsync<Artist>(this.SqlStatement, this.Parameters);
    }
}
