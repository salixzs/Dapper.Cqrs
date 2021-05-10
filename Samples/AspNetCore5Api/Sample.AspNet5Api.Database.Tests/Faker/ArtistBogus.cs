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
    public static class ArtistBogus
    {
        public static Faker<Artist> GetBogus() =>
            new Faker<Artist>()
                .RuleFor(p => p.ArtistId, f => f.Random.Int(min: 1))
                .RuleFor(p => p.Name, f => f.Person.FullName)
        ;

        public static Artist GetFake() => GetBogus().Generate();
    }
}
