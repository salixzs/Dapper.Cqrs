using System.Collections.Generic;
using System.Threading.Tasks;
using Salix.Dapper.Cqrs.Abstractions;

namespace $rootnamespace$
{
    /// <summary>
    /// Retrieves all database records.
    /// </summary>
    public sealed class $safeitemname$ : MsSqlQueryMultipleBase<DbPoco>
    {
        /// <summary>
        /// Actual SQL Statement to execute against MS SQL database.
        /// </summary>
        public override string SqlStatement => @"
SELECT Field1,
       Field2
  FROM DbTable
";
    }
}
