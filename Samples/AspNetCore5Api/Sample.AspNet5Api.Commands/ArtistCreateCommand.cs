using System;
using System.Threading.Tasks;
using Salix.Dapper.Cqrs.Abstractions;
using Sample.AspNet5Api.Domain;

namespace Sample.AspNet5Api.Commands
{
    public sealed class ArtistCreateCommand : ICommand<int>, ICommandValidator
    {
        private readonly Artist _dbObject;

        public ArtistCreateCommand(Artist dbObject) =>
            _dbObject = dbObject ?? throw new ArgumentNullException(nameof(dbObject), "No data passed for Artist create");

        // Two statements - first inserts data, second selects (returns) last inserted record autoincrement value from DB.
        public string SqlStatement => @"
INSERT INTO Artist (
    Name
) VALUES (
    @Name
);SELECT CAST(SCOPE_IDENTITY() as int)";

        public async Task<int> ExecuteAsync(IDatabaseSession session) =>
            await session.ExecuteAsync<int>(this.SqlStatement, _dbObject);
    }
}
