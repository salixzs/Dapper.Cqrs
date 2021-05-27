using System;
using Salix.Dapper.Cqrs.Abstractions;

namespace $rootnamespace$
{
    /// <summary>
    /// A Command to create (INSERT) object into database.
    /// </summary>
    public sealed class $safeitemname$ : MsSqlCommandBase<int>
    {
        private readonly DbPoco _dbObject;

        /// <summary>
        /// A Command to create (INSERT) object into database.
        /// </summary>
        /// <param name="dbObject">Database POCO DTO class to create in database.</param>
        public $safeitemname$(DbPoco dbObject) =>
            _dbObject = dbObject ?? throw new ArgumentNullException(nameof(dbObject), "No data passed for data create");

        /// <summary>
        /// Anonymous object of SqlQuery parameter(s).
        /// </summary>
        public override object Parameters => _dbObject;

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
    }
}
