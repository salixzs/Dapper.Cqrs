using System;

namespace Salix.Dapper.Cqrs.MsSql.Testing.XUnit
{
    /// <summary>
    /// Interface for Factory class returning Domain Test object instances for integration tests.
    /// </summary>
    public interface ITestObjectFactory
    {
        /// <summary>
        /// Returns the domain object suitable for integration testing (writing to database).
        /// </summary>
        /// <param name="objectType">Type of the object.</param>
        object GetTestObject(Type objectType);

        /// <summary>
        /// Returns the domain object suitable for integration testing (writing to database).
        /// </summary>
        /// <typeparam name="T">Type of the test object.</typeparam>
        T GetTestObject<T>() where T : class;

        /// <summary>
        /// Determines whether Factory has fake generator for the specified type (full name).
        /// </summary>
        /// <param name="fullName">The full name for DDD object type.</param>
        bool HasFakeFor(string fullName);
    }
}
