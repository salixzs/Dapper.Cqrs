using System;
using System.Threading.Tasks;
using Salix.Dapper.Cqrs.Abstractions;

namespace Sample.AspNet5Api.Commands
{
    public sealed class ArtistDeleteCommand : ICommand, ICommandValidator
    {
        private readonly int _dbId;

        public ArtistDeleteCommand(int dbId)
        {
            if (dbId == 0)
            {
                throw new ArgumentNullException(nameof(dbId), "Artist delete got 0 as ID.");
            }

            _dbId = dbId;
        }

        public string SqlStatement => @"
DELETE FROM Artist
      WHERE ArtistId = @id";

        public async Task ExecuteAsync(IDatabaseSession session) =>
            await session.ExecuteAsync(this.SqlStatement, new { id = _dbId });
        public void Execute(IDatabaseSession session) => throw new NotImplementedException("Not using synchronous approach with MS SQL");
    }
}
