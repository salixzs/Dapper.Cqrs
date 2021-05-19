using System.Collections.Generic;
using System.Threading.Tasks;
using Salix.Dapper.Cqrs.Abstractions;

namespace $rootnamespace$
{
    /// <summary>
    /// Retrieves multiple database records.
    /// </summary>
    public sealed class $safeitemname$ : MsSqlQueryBase<IEnumerable<DbPoco>>, IQuery<IEnumerable<DbPoco>>
    {
        private readonly int _filter;

        /// <summary>
        /// Retrieves multiple database records.
        /// </summary>
        /// <param name="filter">Some filtering parameter.</param>
        public $safeitemname$(int filter) => _filter = filter;

        /// <summary>
        /// Actual SQL Statement to execute against MS SQL database.
        /// </summary>
        public override string SqlStatement => @"
SELECT Field1,
       Field2
  FROM DbTable
 WHERE ReferenceId = @refid
";

        /// <summary>
        /// Anonymous object of SqlQuery parameter(s).
        /// </summary>
        public override object Parameters => new { refid = _filter };

        /// <summary>
        /// Executes the query in <see cref="SqlStatement"/> asynchronously, using parameters in <see cref="Parameters"/>.
        /// </summary>
        /// <param name="session">The database session object, injected by IoC.</param>
        public async Task<IEnumerable<DbPoco>> ExecuteAsync(IDatabaseSession session)
            => await session.QueryAsync<DbPoco>(this.SqlStatement, this.Parameters);
    }
}
