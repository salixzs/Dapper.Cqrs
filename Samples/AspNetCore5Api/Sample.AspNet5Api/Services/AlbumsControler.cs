using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Sample.AspNet5Api.Domain;
using Sample.AspNet5Api.Logic;

namespace Sample.AspNet5Api.Services
{
    [ApiController]
    [Route("api/albums")]
    public class AlbumsController : ControllerBase
    {
        private readonly IAlbumsLogic _logic;

        public AlbumsController(IAlbumsLogic logic) => _logic = logic;

        [HttpGet]
        public async Task<IEnumerable<Album>> GetAll() => await _logic.GetAlbums();

        [HttpGet("{albumId}")]
        public async Task<Album> Get(int albumId) => await _logic.GetById(albumId);

        [HttpGet("/api/artists/{artistId}/albums")]
        public async Task<ArtistAlbums> GetArtistAlbums(int artistId) => await _logic.GetArtistAlbums(artistId);
    }
}
