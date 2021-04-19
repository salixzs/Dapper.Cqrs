using System.Collections.Generic;
using System.Threading.Tasks;
using Salix.Dapper.Cqrs.Abstractions;
using Sample.AspNet5Api.Domain;

namespace Sample.AspNet5Api.Queries
{
    /// <summary>
    /// Retrieves a list of Albums from Database.
    /// </summary>
    public sealed class AlbumsAllQuery : MsSqlQueryBase, IQuery<IEnumerable<Album>>
    {
        /// <summary>
        /// Actual SQL Statement to execute against MS SQL database.
        /// </summary>
        public override string SqlStatement => @"
SELECT AlbumId,
       ArtistId,
       Title
  FROM Album";

        /// <summary>
        /// Executes the query in <see cref="SqlStatement"/>.
        /// </summary>
        /// <param name="session">The database session object, injected by IoC.</param>
        public async Task<IEnumerable<Album>> ExecuteAsync(IDatabaseSession session)
            => await session.QueryAsync<Album>(this.SqlStatement);
    }
}
