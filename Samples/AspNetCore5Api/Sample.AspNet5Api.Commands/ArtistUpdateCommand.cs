using System;
using Salix.Dapper.Cqrs.Abstractions;
using Sample.AspNet5Api.Domain;

namespace Sample.AspNet5Api.Commands
{
    public sealed class ArtistUpdateCommand : MsSqlCommandBase
    {
        private readonly Artist _dbObject;

        public ArtistUpdateCommand(Artist dbObject) =>
            _dbObject = dbObject ?? throw new ArgumentNullException(nameof(dbObject), "No data passed for Artist update");

        public override object Parameters => _dbObject;

        public override string SqlStatement => @"
UPDATE Artist
   SET Name = @Name
 WHERE ArtistId = @ArtistId";
    }
}
