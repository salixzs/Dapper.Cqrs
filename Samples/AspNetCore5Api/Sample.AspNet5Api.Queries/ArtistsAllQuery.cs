using Salix.Dapper.Cqrs.Abstractions;
using Sample.AspNet5Api.Domain;

namespace Sample.AspNet5Api.Queries
{
    /// <summary>
    /// Retrieves a list of Artists from Database.
    /// </summary>
    public sealed class ArtistsAllQuery : MsSqlQueryMultipleBase<Artist>
    {
        /// <summary>
        /// Actual SQL Statement to execute against MS SQL database.
        /// </summary>
        public override string SqlStatement => @"
SELECT ArtistId,
       Name
  FROM Artist";
    }
}
