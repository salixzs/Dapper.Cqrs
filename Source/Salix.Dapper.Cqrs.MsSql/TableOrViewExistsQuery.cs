using System.Diagnostics.CodeAnalysis;
using Salix.Dapper.Cqrs.Abstractions;

namespace Salix.Dapper.Cqrs.MsSql
{
    /// <summary>
    /// Returns True/False depending on whether supplied table or view exists in database.
    /// </summary>
    [ExcludeFromCodeCoverage]

    public sealed class TableOrViewExistsQuery : MsSqlQuerySingleBase<bool>
    {
        private readonly string _objectName;

        /// <summary>
        /// Returns True/False depending on whether supplied table or view exists in database.
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
    }
}
