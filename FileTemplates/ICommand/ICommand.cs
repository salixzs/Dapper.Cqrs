using System;
using Salix.Dapper.Cqrs.Abstractions;

namespace $rootnamespace$
{
    /// <summary>
    /// Issue data manipulation command to database.
    /// </summary>
    public sealed class $safeitemname$ : MsSqlCommandBase<int>, ICommand<int>, ICommandValidator
    {
        private readonly DbPoco _dbObject;

        /// <summary>
        /// Issue data manipulation command to database.
        /// </summary>
        /// <param name="dbObject">Database POCO DTO class to write to database.</param>
        public $safeitemname$(DbPoco dbObject) =>
            _dbObject = dbObject ?? throw new ArgumentNullException(nameof(dbObject), "No data passed for data command");

        /// <summary>
        /// Actual SQL Statement to execute against MS SQL database.
        /// </summary>
        public override string SqlStatement => @"
INSERT INTO DbTable (
    Field1,
    Field2
) VALUES (
    @Prop1,
    @Prop2
);SELECT CAST(SCOPE_IDENTITY() as int)
";

        /// <summary>
        /// Anonymous object of SqlQuery parameter(s).
        /// </summary>
        public override object Parameters => _dbObject;
    }
}
