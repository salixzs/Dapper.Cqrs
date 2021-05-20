using System;
using Salix.Dapper.Cqrs.Abstractions;

namespace $rootnamespace$
{
    /// <summary>
    /// A Command to UPDATE object in database.
    /// </summary>
    public sealed class $safeitemname$ : MsSqlCommandBase, ICommand, ICommandValidator
    {
        private readonly DbPoco _dbObject;

        /// <summary>
        /// A Command to UPDATE object in database.
        /// </summary>
        /// <param name="dbObject">Database POCO DTO class to update in database.</param>
        public $safeitemname$(DbPoco dbObject) =>
            _dbObject = dbObject ?? throw new ArgumentNullException(nameof(dbObject), "No data passed for data update");

        /// <summary>
        /// Actual SQL Statement to execute against MS SQL database.
        /// </summary>
        public override string SqlStatement => @"
UPDATE [DbTable]
   SET Field1 = @Property1,
       Field2 = @Property2
 WHERE Id = @Id
";

        /// <summary>
        /// Anonymous object of SqlQuery parameter(s).
        /// </summary>
        public override object Parameters => _dbObject;
    }
}
