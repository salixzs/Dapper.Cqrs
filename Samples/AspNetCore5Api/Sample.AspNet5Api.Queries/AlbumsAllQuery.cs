using Salix.Dapper.Cqrs.Abstractions;
using Sample.AspNet5Api.Domain;

namespace Sample.AspNet5Api.Queries
{
    /// <summary>
    /// Retrieves a list of Albums from Database.
    /// </summary>
    public sealed class AlbumsAllQuery : MsSqlQueryMultipleBase<Album>
    {
        /// <summary>
        /// Actual SQL Statement to execute against MS SQL database.
        /// </summary>
        public override string SqlStatement => @"
SELECT AlbumId,
       ArtistId,
       Title
  FROM Album";
    }
}
