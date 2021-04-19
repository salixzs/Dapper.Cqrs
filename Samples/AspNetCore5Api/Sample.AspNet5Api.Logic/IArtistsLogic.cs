using System.Collections.Generic;
using System.Threading.Tasks;
using Sample.AspNet5Api.Domain;

namespace Sample.AspNet5Api.Logic
{
    public interface IArtistsLogic
    {
        Task<IEnumerable<Artist>> GetAll();

        Task<Artist> GetById(int artistId);
        Task<int> Create(Artist artist);
        Task Update(Artist artist);
        Task Delete(int artistId);
    }
}
