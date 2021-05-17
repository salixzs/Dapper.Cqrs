using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Salix.Dapper.Cqrs.Abstractions;

namespace Salix.Dapper.Cqrs.MsSql
{
    /// <summary>
    /// Returns True/False depending on whether given Stored Procedure exists in database.
    /// </summary>
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    [ExcludeFromCodeCoverage]

    public sealed class StoredProcedureExistsQuery : MsSqlQueryBase<bool>, IQuery<bool>
    {
        private readonly string _objectName;

        /// <summary>
        /// Returns True/False depending on whether given Stored Procedure exists in database.
        /// </summary>
        /// <param name="objectName">The exact name of Stored procedure in database .</param>
        public StoredProcedureExistsQuery(string objectName) => _objectName = objectName;

        /// <summary>
        /// Actual SQL Statement to execute against MS SQL database.
        /// </summary>
        public override string SqlStatement => @"
SELECT CONVERT(bit, COUNT(*))
  FROM INFORMATION_SCHEMA.ROUTINES 
 WHERE ROUTINE_NAME = @ObjectName
       AND ROUTINE_TYPE = 'PROCEDURE'
";

        /// <summary>
        /// Anonymous object of Table Name.
        /// </summary>
        public override object Parameters => new { ObjectName = _objectName };

        /// <summary>
        /// Executes the query in <see cref="SqlStatement"/> asynchronously, using parameters in <see cref="Parameters"/>.
        /// </summary>
        /// <param name="session">The database session object, injected by IoC.</param>
        public async Task<bool> ExecuteAsync(IDatabaseSession session)
            => await session.QueryFirstOrDefaultAsync<bool>(this.SqlStatement, this.Parameters);

        /// <summary>
        /// Actual executable method of database query which returns data from database.
        /// </summary>
        /// <param name="session">The database connection session.</param>
        public override bool Execute(IDatabaseSession session)
            => session.QueryFirstOrDefault<bool>(this.SqlStatement, this.Parameters);

        /// <summary>
        /// Displays DB object main properties in Debug screen. (Only for development purposes).
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string DebuggerDisplay => $"SELECT ? FROM schema.tables WHERE OBJECT_NAME = '{_objectName}'";
    }
}
