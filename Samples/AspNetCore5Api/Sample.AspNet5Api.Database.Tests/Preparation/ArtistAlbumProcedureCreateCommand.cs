using System.Diagnostics.CodeAnalysis;
using Salix.Dapper.Cqrs.Abstractions;

namespace Sample.AspNet5Api.Database.Tests.Preparation
{
    /// <summary>
    /// Retrieves a list of columns and its properties in specified database object (table, view).
    /// </summary>
    [ExcludeFromCodeCoverage]

    public sealed class ArtistAlbumProcedureCreateCommand : MsSqlCommandBase, ICommand
    {
        /// <summary>
        /// Actual SQL Statement to execute against MS SQL database.
        /// </summary>
        public override string SqlStatement => @"
CREATE PROCEDURE [dbo].[ArtistAlbumSp] 
	@ArtistId int
AS
BEGIN
	SET NOCOUNT ON;

	SELECT A.ArtistId, A.Name, B.AlbumId, B.Title FROM Artist A INNER JOIN Album B ON B.ArtistId = A.ArtistId
	WHERE A.ArtistId = @ArtistId
END
";
    }
}
