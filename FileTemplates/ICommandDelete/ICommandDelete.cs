using System;
using Salix.Dapper.Cqrs.Abstractions;

namespace $rootnamespace$
{
    /// <summary>
    /// A Command to DELETE object in database.
    /// </summary>
    public sealed class $safeitemname$ : MsSqlCommandBase
    {
        private readonly int _id;

        /// <summary>
        /// A Command to DELETE object in database.
        /// </summary>
        /// <param name="id">Record identifier to delete.</param>
        public $safeitemname$(int id)
        {
            if (id == 0)
            {
                throw new ArgumentException("ID is zero for delete operation.", nameof(id));
            }

            _id = id;
        }

        /// <summary>
        /// Anonymous object of SqlQuery parameter(s).
        /// </summary>
        public override object Parameters => new { id = _id };

        /// <summary>
        /// Actual SQL Statement to execute against MS SQL database.
        /// </summary>
        public override string SqlStatement => @"
DELETE FROM [DbTable]
      WHERE Id = @id
";
    }
}
