using System;
using System.Data.SqlClient;
using Salix.Dapper.Cqrs.Abstractions;

namespace Salix.Dapper.Cqrs.MsSql
{
    /// <summary>
    /// Creates Microsoft SQL database Context to use for CQRS functionalities to access database through Dapper ORM.
    /// Used in <see cref="IDatabaseSession" /> implementation methods to do lowest level
    /// Dapper executions against database, using DB connection provided here.
    /// </summary>
    /// <seealso cref="IDisposable" />
    public interface IMsSqlContext : IDatabaseContext
    {
        /// <summary>
        /// Access to actual SqlConnection object.
        /// Gets created if not exist when accessed.
        /// Gets Open when accessed (if closed).
        /// Creates transaction on it, if not exist.
        /// Do not dispose it separately, use entire context dispose when Unit of Work completes.
        /// </summary>
        new SqlConnection Connection { get; }
    }
}
