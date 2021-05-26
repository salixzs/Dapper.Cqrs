using System.Diagnostics.CodeAnalysis;
using Salix.Dapper.Cqrs.Abstractions;

namespace Salix.Dapper.Cqrs.MsSql
{
    /// <summary>
    /// Returns True/False depending on whether given Function exists in database.
    /// </summary>
    [ExcludeFromCodeCoverage]

    public sealed class FunctionExistsQuery : MsSqlQuerySingleBase<bool>
    {
        private readonly string _objectName;

        /// <summary>
        /// Returns True/False depending on whether given Function exists in database.
        /// </summary>
        /// <param name="objectName">The exact function name in database.</param>
        public FunctionExistsQuery(string objectName) => _objectName = objectName;

        /// <summary>
        /// Actual SQL Statement to execute against MS SQL database.
        /// </summary>
        public override string SqlStatement => @"
SELECT CONVERT(bit, COUNT(*))
  FROM INFORMATION_SCHEMA.ROUTINES 
 WHERE ROUTINE_NAME = @ObjectName
       AND ROUTINE_TYPE = 'FUNCTION'
";

        /// <summary>
        /// Anonymous object of Table Name.
        /// </summary>
        public override object Parameters => new { ObjectName = _objectName };
    }
}
