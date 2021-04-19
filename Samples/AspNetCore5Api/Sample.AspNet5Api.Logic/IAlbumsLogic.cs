using System.Collections.Generic;
using System.Threading.Tasks;
using Sample.AspNet5Api.Domain;

namespace Sample.AspNet5Api.Logic
{
    public interface IAlbumsLogic
    {
        Task<ArtistAlbums> GetArtistAlbums(int artistId);

        Task<Album> GetById(int albumId);

        Task<IEnumerable<Album>> GetAlbums();
    }
}
