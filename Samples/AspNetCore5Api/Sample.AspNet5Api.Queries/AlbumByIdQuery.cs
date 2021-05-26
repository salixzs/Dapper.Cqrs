using Salix.Dapper.Cqrs.Abstractions;
using Sample.AspNet5Api.Domain;

namespace Sample.AspNet5Api.Queries
{
    /// <summary>
    /// Retrieves an Album from database by its ID.
    /// </summary>
    public sealed class AlbumByIdQuery : MsSqlQuerySingleBase<Album>
    {
        private readonly int _objectId;

        /// <summary>
        /// Retrieves an Album from database by its ID.
        /// </summary>
        /// <param name="objectId">Artist ID in database.</param>
        public AlbumByIdQuery(int objectId) => _objectId = objectId;

        /// <summary>
        /// Actual SQL Statement to execute against MS SQL database.
        /// </summary>
        public override string SqlStatement => @"
SELECT AlbumId,
       Title,
       ArtistId
  FROM Album
 WHERE AlbumId = @id";

        /// <summary>
        /// Anonymous object of SqlQuery parameter(s).
        /// </summary>
        public override object Parameters => new { id = _objectId };
    }
}
