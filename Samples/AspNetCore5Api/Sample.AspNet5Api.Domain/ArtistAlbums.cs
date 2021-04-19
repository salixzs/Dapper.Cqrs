using System.Collections.Generic;

namespace Sample.AspNet5Api.Domain
{
    public class ArtistAlbums
    {
        public Artist Artist { get; set; }

        public IEnumerable<Album> Albums { get; set; }
    }
}
