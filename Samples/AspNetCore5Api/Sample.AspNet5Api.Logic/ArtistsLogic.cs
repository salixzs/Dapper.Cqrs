using System.Collections.Generic;
using System.Threading.Tasks;
using Salix.Dapper.Cqrs.Abstractions;
using Sample.AspNet5Api.Commands;
using Sample.AspNet5Api.Domain;
using Sample.AspNet5Api.Queries;

namespace Sample.AspNet5Api.Logic
{
    public class ArtistsLogic : IArtistsLogic
    {
        private readonly ICommandQueryContext _db;

        public ArtistsLogic(ICommandQueryContext db) => _db = db;

        public async Task<IEnumerable<Artist>> GetAll() => await _db.QueryAsync(new ArtistsAllQuery());

        public async Task<Artist> GetById(int artistId) => await _db.QueryAsync(new ArtistByIdQuery(artistId));

        public async Task<int> Create(Artist artist) => await _db.ExecuteAsync(new ArtistCreateCommand(artist));

        public async Task Update(Artist artist) => await _db.ExecuteAsync(new ArtistUpdateCommand(artist));

        public async Task Delete(int artistId) => await _db.ExecuteAsync(new ArtistDeleteCommand(artistId));
    }
}
