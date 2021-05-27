using System.Diagnostics.CodeAnalysis;
using Salix.Dapper.Cqrs.Abstractions;

namespace Sample.AspNet5Api.Database.Tests.Preparation
{
    /// <summary>
    /// Creates a simple view to check view usage in SQL statements.
    /// </summary>
    [ExcludeFromCodeCoverage]

    public sealed class ArtistAlbumViewCreateCommand : MsSqlCommandBase
    {
        /// <summary>
        /// Actual SQL Statement to execute against MS SQL database.
        /// </summary>
        public override string SqlStatement => @"
CREATE VIEW [dbo].[ArtistAlbumView] AS
SELECT A.ArtistId,
       A.Name,
	   B.AlbumId,
	   B.Title
FROM   Artist A
       INNER JOIN Album B
	   ON B.ArtistId = A.ArtistId
";
    }
}
