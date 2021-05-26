using System;
using Salix.Dapper.Cqrs.Abstractions;

namespace Sample.AspNet5Api.Commands
{
    public sealed class ArtistDeleteCommand : MsSqlCommandBase
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

        public override string SqlStatement => @"
DELETE FROM Artist
      WHERE ArtistId = @id";

        public override object Parameters => new { id = _dbId };
    }
}
