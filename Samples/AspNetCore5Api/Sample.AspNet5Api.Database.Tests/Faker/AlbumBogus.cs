using System.Diagnostics.CodeAnalysis;
using Bogus;
using Sample.AspNet5Api.Domain;

namespace Sample.AspNet5Api.Database.Tests.Faker
{
    /// <summary>
    /// Bogus Fake object creation class containing routines to come up with Bogus/Fake object which is close to reality and possible to save to database.
    /// See more possibilities at https://github.com/bchavez/Bogus
    /// </summary>
    [ExcludeFromCodeCoverage]
    public static class AlbumBogus
    {
        public static Faker<Album> GetBogus() =>
            new Faker<Album>()
                .RuleFor(p => p.AlbumId, f => f.Random.Int(min: 1))
                .RuleFor(p => p.ArtistId, f => f.Random.Int(min: 1))
                .RuleFor(p => p.Title, f => f.Commerce.Product())
        ;

        public static Album GetFake() => GetBogus().Generate();
    }
}
