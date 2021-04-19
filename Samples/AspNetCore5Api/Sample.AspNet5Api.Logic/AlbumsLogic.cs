using System.Collections.Generic;
using System.Threading.Tasks;
using Salix.Dapper.Cqrs.Abstractions;
using Sample.AspNet5Api.Domain;
using Sample.AspNet5Api.Queries;

namespace Sample.AspNet5Api.Logic
{
    public class AlbumsLogic : IAlbumsLogic
    {
        private readonly ICommandQueryContext _db;

        public AlbumsLogic(ICommandQueryContext db) => _db = db;

        public async Task<ArtistAlbums> GetArtistAlbums(int artistId)
        {
            Artist artist = await _db.QueryAsync(new ArtistByIdQuery(artistId));
            IEnumerable<Album> albums = await _db.QueryAsync(new ArtistAlbumsQuery(artistId));
            return new ArtistAlbums
            {
                Artist = artist,
                Albums = albums
            };
        }

        public async Task<IEnumerable<Album>> GetAlbums() => await _db.QueryAsync(new AlbumsAllQuery());

        public async Task<Album> GetById(int albumId) => await _db.QueryAsync(new AlbumByIdQuery(albumId));
    }
}
