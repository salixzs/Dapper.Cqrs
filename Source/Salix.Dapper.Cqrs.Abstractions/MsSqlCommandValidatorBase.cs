namespace Salix.Dapper.Cqrs.Abstractions
{
    /// <summary>
    /// Base class for <see cref="ICommandValidator"/> implementations.
    /// Provides overrideable properties and methods common for MsSqmCommandBase classes, which are inheriting this class.
    /// Provides Validation method, using special "CheckSql" SQL function. (See documentation).
    /// NOTE: SHould not be inherited in application code, instead inherit from <see cref="MsSqlCommandBase"/> or <see cref="MsSqlCommandBase{T}"/>.
    /// </summary>
    [System.Diagnostics.DebuggerDisplay("{DebuggerDisplay,nq}")]
    public abstract class MsSqlCommandValidatorBase : ICommandValidator
    {
        /// <summary>
        /// Property to hold SQL Statement used for Command class.
        /// Should be overridden in derived class.
        /// </summary>
        public virtual string SqlStatement => "SQL Statement is not overridden in inheriting class";

        /// <summary>
        /// Anonymous object of SqlStatement parameters.
        /// Should be overridden in derived class if needed.
        /// </summary>
        public virtual object Parameters => null;

        /// <summary>
        /// Validates the SQL statement against database engine for its syntax and structural correctness.
        /// Validates only statement in <see cref="SqlStatement"/>. If derived class contains more than this one
        /// SQL statement - override this implementation and add all statement validations to it.
        /// </summary>
        /// <param name="session">The database session object.</param>
        /// <exception cref="DatabaseStatementSyntaxException">Throws when <see cref="SqlStatement"/> is incorrect.</exception>
        public virtual void Validate(IDatabaseSession session)
        {
            string result = session.QueryFirstOrDefault<string>("SELECT dbo.CheckSql(@tsql, @parameterTypes)", new { tsql = this.SqlStatement, parameterTypes = MsSqlValidationHelpers.GetParameterTypes(this.Parameters) });
            if (result != "OK")
            {
                throw new DatabaseStatementSyntaxException(result, this.SqlStatement);
            }
        }

        /// <summary>
        /// Shows a shortened SQL statement (of inheriting class).
        /// </summary>
        public override string ToString() => this.SqlStatement.ToShortSql();

        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        private string DebuggerDisplay => this.SqlStatement.ToShortSql();
    }
}
