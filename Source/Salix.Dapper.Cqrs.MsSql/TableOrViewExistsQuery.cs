using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Salix.Dapper.Cqrs.Abstractions;

namespace Salix.Dapper.Cqrs.MsSql
{
    /// <summary>
    /// Returns True/False depending on whether supplied table or view name exists in database.
    /// </summary>
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    [ExcludeFromCodeCoverage]

    public sealed class TableOrViewExistsQuery : MsSqlQueryBase<bool>, IQuery<bool>
    {
        private readonly string _objectName;

        /// <summary>
        /// Returns True/False depending on whether supplied table or view name exists in database.
        /// </summary>
        /// <param name="objectName">The database object (Table, View) name.</param>
        public TableOrViewExistsQuery(string objectName) => _objectName = objectName;

        /// <summary>
        /// Actual SQL Statement to execute against MS SQL database.
        /// </summary>
        public override string SqlStatement => @"
SELECT CONVERT(bit, COUNT(*))
  FROM INFORMATION_SCHEMA.TABLES 
 WHERE TABLE_NAME = @ObjectName
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
