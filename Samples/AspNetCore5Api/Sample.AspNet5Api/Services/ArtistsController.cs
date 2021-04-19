using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Sample.AspNet5Api.Domain;
using Sample.AspNet5Api.Logic;

namespace Sample.AspNet5Api.Services
{
    [ApiController]
    [Route("api/artists")]
    public class ArtistsController : ControllerBase
    {
        private readonly IArtistsLogic _logic;

        public ArtistsController(IArtistsLogic logic) => _logic = logic;

        [HttpGet]
        public async Task<IEnumerable<Artist>> GetAll() => await _logic.GetAll();

        [HttpGet("{artistId}")]
        public async Task<Artist> Get(int artistId) => await _logic.GetById(artistId);

        [HttpPost]
        public async Task<int> Create(Artist artist) => await _logic.Create(artist);

        [HttpPatch]
        public async Task Update(Artist artist) => await _logic.Update(artist);

        [HttpDelete]
        public async Task Delete(int artistId) => await _logic.Delete(artistId);
    }
}
