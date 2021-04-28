using System;

namespace Salix.Dapper.Cqrs.Abstractions
{
    /// <summary>
    /// Base class for <see cref="IQuery{T}"/> implementations.
    /// Expects ExecuteAsync to be implemented in derived class, meanwhile implementing (throws NotImplementedException) synchronous Execute method.
    /// Provides Validation method, using special "CheckSql" SQL function. (See documentation).
    /// Implements RealSqlStatement property, which can be used in debug time to get SQL statement with parameter definitions for copy/paste into SQL Development Studio.
    /// </summary>
    [System.Diagnostics.DebuggerDisplay("{DebuggerDisplay,nq}")]
    public abstract class MsSqlQueryBase<T>
    {
        /// <summary>
        /// Property to hold SQL Statement used for Query class.
        /// Should be overridden in derived class.
        /// </summary>
        public virtual string SqlStatement => "SQL Statement is not overridden in inheriting class";

        /// <summary>
        /// Anonymous object of SqlQuery parameters.
        /// Should be overridden in derived class if needed.
        /// </summary>
        public virtual object Parameters => null;

        /// <summary>
        /// Use during Code Debugging (by developer) to get statements, which can be copy-pasted into SQL Management Studio for checking against database.
        /// </summary>
        public string RealSqlStatement => $@"{MsSqlValidationHelpers.GetParameterStatements(this.Parameters)}
{this.SqlStatement}";

        /// <summary>
        /// Validates the Query SQL statement against database engine for its syntax and structural correctness.
        /// Validates only statement in <see cref="SqlStatement"/>. If derived class contains more than this one
        /// SQL statement - override this implementation and add all statement validations to it.
        /// </summary>
        /// <param name="databaseSession">The database session object.</param>
        /// <exception cref="DatabaseStatementSyntaxException">Throws when <see cref="SqlStatement"/> is incorrect.</exception>
        public virtual void Validate(IDatabaseSession databaseSession)
        {
            string result = databaseSession.QueryFirstOrDefault<string>("SELECT dbo.CheckSql(@tsql, @parameterTypes)", new { tsql = this.SqlStatement, parameterTypes = MsSqlValidationHelpers.GetParameterTypes(this.Parameters) });
            if (result != "OK")
            {
                throw new DatabaseStatementSyntaxException(result, this.SqlStatement);
            }
        }

        /// <summary>
        /// Actual executable method of database query which returns data from database.
        /// NOTE: Abstract class method throws <see cref="NotImplementedException"/>. Override in implementing class if needed.
        /// </summary>
        /// <param name="session">The database connection session.</param>
        public virtual T Execute(IDatabaseSession session) => throw new NotImplementedException("For MS SQL Server best use ExecuteAsync() or implement this method (overdrive) in IQuery class.");

        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        private string DebuggerDisplay => this.SqlStatement.ToShortSql();

        public override string ToString() => nameof(T);
    }
}
