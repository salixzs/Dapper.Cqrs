using System.Threading.Tasks;
using Salix.Dapper.Cqrs.Abstractions;

namespace $rootnamespace$
{
    /// <summary>
    /// Retrieves a database record by its ID.
    /// </summary>
    public sealed class $safeitemname$ : MsSqlQueryBase<DbPoco>, IQuery<DbPoco>
    {
        private readonly int _objectId;

        /// <summary>
        /// Retrieves a database record by its ID.
        /// </summary>
        /// <param name="objectId">Unique record ID in database.</param>
        public $safeitemname$(int objectId) => _objectId = objectId;

        /// <summary>
        /// Actual SQL Statement to execute against MS SQL database.
        /// </summary>
        public override string SqlStatement => @"
SELECT Field1,
       Field2
  FROM DbTable
 WHERE Id = @id
";

        /// <summary>
        /// Anonymous object of SqlQuery parameter(s).
        /// </summary>
        public override object Parameters => new { id = _objectId };

        /// <summary>
        /// Executes the query in <see cref="SqlStatement"/> asynchronously, using parameters in <see cref="Parameters"/>.
        /// </summary>
        /// <param name="session">The database session object, injected by IoC.</param>
        public async Task<DbPoco> ExecuteAsync(IDatabaseSession session)
            => await session.QueryFirstOrDefaultAsync<DbPoco>(this.SqlStatement, this.Parameters);
    }
}
