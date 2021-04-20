using System.Collections.Generic;
using System.Threading.Tasks;
using Salix.Dapper.Cqrs.Abstractions;
using Sample.AspNet5Api.Domain;

namespace Sample.AspNet5Api.Queries
{
    /// <summary>
    /// Retrieves a list of Artists from Database.
    /// </summary>
    public sealed class ArtistsAllQuery : MsSqlQueryBase<IEnumerable<Artist>>, IQuery<IEnumerable<Artist>>
    {
        /// <summary>
        /// Actual SQL Statement to execute against MS SQL database.
        /// </summary>
        public override string SqlStatement => @"
SELECT ArtistId,
       Name
  FROM Artist";

        /// <summary>
        /// Executes the query in <see cref="SqlStatement"/>.
        /// </summary>
        /// <param name="session">The database session object, injected by IoC.</param>
        public async Task<IEnumerable<Artist>> ExecuteAsync(IDatabaseSession session)
            => await session.QueryAsync<Artist>(this.SqlStatement);
    }
}
