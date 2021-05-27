using System;
using Salix.Dapper.Cqrs.Abstractions;
using Sample.AspNet5Api.Domain;

namespace Sample.AspNet5Api.Commands
{
    public sealed class AlbumCreateCommand : MsSqlCommandBase<int>
    {
        private readonly Album _dbObject;

        public AlbumCreateCommand(Album dbObject) =>
            _dbObject = dbObject ?? throw new ArgumentNullException(nameof(dbObject), "No data passed for Album create");

        public override object Parameters => _dbObject;

        // Two statements - first inserts data, second selects (returns) last inserted record autoincrement value from DB.
        public override string SqlStatement => @"
INSERT INTO Album (
    Title,
    ArtistId
) VALUES (
    @Title,
    @ArtistId
);SELECT CAST(SCOPE_IDENTITY() as int)";
    }
}
