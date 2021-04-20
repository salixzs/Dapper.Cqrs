using System;
using System.Threading.Tasks;
using Salix.Dapper.Cqrs.Abstractions;
using Sample.AspNet5Api.Domain;

namespace Sample.AspNet5Api.Commands
{
    public sealed class ArtistUpdateCommand : ICommand, ICommandValidator
    {
        private readonly Artist _dbObject;

        public ArtistUpdateCommand(Artist dbObject) =>
            _dbObject = dbObject ?? throw new ArgumentNullException(nameof(dbObject), "No data passed for Artist update");

        public string SqlStatement => @"
UPDATE Artist
   SET Name = @Name
 WHERE ArtistId = @ArtistId";

        public async Task ExecuteAsync(IDatabaseSession session) =>
            await session.ExecuteAsync(this.SqlStatement, _dbObject);
        public void Execute(IDatabaseSession session) => throw new NotImplementedException("Not using synchronous approach with MS SQL");
    }
}
