using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Salix.Dapper.Cqrs.MsSql.Testing.XUnit;
using Sample.AspNet5Api.Domain;


namespace Sample.AspNet5Api.Database.Tests.Faker
{
    [ExcludeFromCodeCoverage]
    public sealed class TestObjectFactory : ITestObjectFactory
    {
        /// <summary>
        /// Dictionary with DB Class names and method to execute which creates appropriate object.
        /// </summary>
        private readonly Dictionary<string, Func<object>> _instantiators = new();

        /// <summary>
        /// Explicit static constructor to tell C# compiler not to mark type as beforefieldinit.
        /// </summary>
        static TestObjectFactory()
        {
        }

        /// <summary>
        /// Prevents a default instance of the <see cref="TestObjectFactory"/> class from being created.
        /// Normal Singleton implementation where class is instantiated in static readonly field.
        /// Here Dictionary of DB POCO test object implementation classes/methods should be added.
        /// </summary>
        private TestObjectFactory()
        {
            // Add all DDD database objects here with their Faker implementations.
            // Key = exact type full name of DB POCO Class;  Value = Function which needs to be called when is asked for this object fake
            _instantiators.Add(typeof(Artist).FullName, ArtistBogus.GetFake);
            _instantiators.Add(typeof(Album).FullName, AlbumBogus.GetFake);
        }

        /// <summary>
        /// Determines whether Factory has fake generator for the specified type (full name).
        /// </summary>
        /// <param name="fullName">The full name for DDD object type.</param>
        public bool HasFakeFor(string fullName) => _instantiators.ContainsKey(fullName);

        /// <summary>
        /// Singleton instance of this Factory object.
        /// </summary>
        public static TestObjectFactory Instance { get; } = new();

        /// <summary>
        /// Returns a test object (filled with dummy, but "real" data) for testing purposes.
        /// </summary>
        /// <typeparam name="T">Type of a DB POCO class.</typeparam>
        /// <exception cref="ArgumentException">There is no logic added for given type in this factory.</exception>
        public T GetTestObject<T>()
            where T : class
        {
            string classFullName = typeof(T).FullName;
            if (!_instantiators.ContainsKey(classFullName))
            {
                throw new ArgumentException($"There is no defined instantiation for {classFullName} object.");
            }

            return (T)_instantiators[classFullName]();
        }

        /// <summary>
        /// Returns a test object (filled with dummy, but "real" data) for testing purposes.
        /// </summary>
        /// <exception cref="ArgumentException">There is no logic added for given type in this factory.</exception>
        public object GetTestObject(Type objectType)
        {
            string classFullName = objectType.FullName;
            if (!_instantiators.ContainsKey(classFullName))
            {
                throw new ArgumentException($"There is no defined instantiation for {classFullName} object.");
            }

            return _instantiators[classFullName]();
        }
    }
}
