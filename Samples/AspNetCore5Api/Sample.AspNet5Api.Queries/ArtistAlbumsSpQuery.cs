using Salix.Dapper.Cqrs.Abstractions;
using Sample.AspNet5Api.Domain;

namespace Sample.AspNet5Api.Queries
{
    /// <summary>
    /// Retrieves a list of Artist Albums from Database.
    /// </summary>
    public sealed class ArtistAlbumsSpQuery : MsSqlQueryMultipleBase<ArtistAlbum>
    {
        private readonly int _objectId;

        /// <summary>
        /// Retrieves an Artist Albums from database by artist ID.
        /// </summary>
        /// <param name="objectId">Artist ID in database.</param>
        public ArtistAlbumsSpQuery(int objectId) => _objectId = objectId;

        /// <summary>
        /// Actual SQL Statement to execute against MS SQL database.
        /// </summary>
        public override string SqlStatement => @"EXEC ArtistAlbumSp @id";

        /// <summary>
        /// Anonymous object of SqlQuery parameter(s).
        /// </summary>
        public override object Parameters => new { id = _objectId };
    }
}
