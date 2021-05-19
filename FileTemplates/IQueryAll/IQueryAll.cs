using System.Collections.Generic;
using System.Threading.Tasks;
using Salix.Dapper.Cqrs.Abstractions;

namespace $rootnamespace$
{
    /// <summary>
    /// Retrieves all database records.
    /// </summary>
    public sealed class $safeitemname$ : MsSqlQueryBase<IEnumerable<DbPoco>>, IQuery<IEnumerable<DbPoco>>
    {
        /// <summary>
        /// Actual SQL Statement to execute against MS SQL database.
        /// </summary>
        public override string SqlStatement => @"
SELECT Field1,
       Field2
  FROM DbTable
";

        /// <summary>
        /// Executes the query in <see cref="SqlStatement"/> asynchronously, using parameters in <see cref="Parameters"/>.
        /// </summary>
        /// <param name="session">The database session object, injected by IoC.</param>
        public async Task<IEnumerable<DbPoco>> ExecuteAsync(IDatabaseSession session)
            => await session.QueryAsync<DbPoco>(this.SqlStatement);
    }
}
