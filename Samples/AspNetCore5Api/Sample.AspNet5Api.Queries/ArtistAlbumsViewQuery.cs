using System.Collections.Generic;
using System.Threading.Tasks;
using Salix.Dapper.Cqrs.Abstractions;
using Sample.AspNet5Api.Domain;

namespace Sample.AspNet5Api.Queries
{
    /// <summary>
    /// Retrieves a list of Artist Albums from Database.
    /// </summary>
    public sealed class ArtistAlbumsViewQuery : MsSqlQueryBase<IEnumerable<ArtistAlbum>>, IQuery<IEnumerable<ArtistAlbum>>
    {
        private readonly int _objectId;

        /// <summary>
        /// Retrieves an Artist Albums from database by artist ID.
        /// </summary>
        /// <param name="objectId">Artist ID in database.</param>
        public ArtistAlbumsViewQuery(int objectId) => _objectId = objectId;

        /// <summary>
        /// Actual SQL Statement to execute against MS SQL database.
        /// </summary>
        public override string SqlStatement => @"
SELECT ArtistId,
       Name,
       AlbumId,
       Title
  FROM ArtistAlbumView
 WHERE ArtistId = @id";

        /// <summary>
        /// Anonymous object of SqlQuery parameter(s).
        /// </summary>
        public override object Parameters => new { id = _objectId };

        /// <summary>
        /// Executes the query in <see cref="SqlStatement"/>.
        /// </summary>
        /// <param name="session">The database session object, injected by IoC.</param>
        public async Task<IEnumerable<ArtistAlbum>> ExecuteAsync(IDatabaseSession session)
            => await session.QueryAsync<ArtistAlbum>(this.SqlStatement, this.Parameters);
    }
}
