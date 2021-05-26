using Salix.Dapper.Cqrs.Abstractions;
using Sample.AspNet5Api.Domain;

namespace Sample.AspNet5Api.Queries
{
    /// <summary>
    /// Retrieves an Artist from database by its ID.
    /// </summary>
    public sealed class ArtistByIdQuery : MsSqlQuerySingleBase<Artist>
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
    }
}
