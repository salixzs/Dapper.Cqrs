using System.Collections.Generic;
using System.Threading.Tasks;
using Salix.Dapper.Cqrs.Abstractions;

namespace $rootnamespace$
{
    /// <summary>
    /// Retrieves multiple database records.
    /// </summary>
    public sealed class $safeitemname$ : MsSqlQueryMultipleBase<DbPoco>
    {
        private readonly int _filter;

        /// <summary>
        /// Retrieves multiple database records.
        /// </summary>
        /// <param name="filter">Some filtering parameter.</param>
        public $safeitemname$(int filter) => _filter = filter;

        /// <summary>
        /// Anonymous object of SqlQuery parameter(s).
        /// </summary>
        public override object Parameters => new { refid = _filter };

        /// <summary>
        /// Actual SQL Statement to execute against MS SQL database.
        /// </summary>
        public override string SqlStatement => @"
SELECT Field1,
       Field2
  FROM DbTable
 WHERE ReferenceId = @refid
";
    }
}
