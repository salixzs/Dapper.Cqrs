using System.Diagnostics.CodeAnalysis;
using Salix.Dapper.Cqrs.Abstractions;

namespace Salix.Dapper.Cqrs.MsSql
{
    /// <summary>
    /// Returns True/False depending on whether given Stored Procedure exists in database.
    /// </summary>
    [ExcludeFromCodeCoverage]

    public sealed class StoredProcedureExistsQuery : MsSqlQuerySingleBase<bool>
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
    }
}
