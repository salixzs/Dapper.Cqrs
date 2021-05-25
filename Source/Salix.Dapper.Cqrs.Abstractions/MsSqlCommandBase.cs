using System.Threading.Tasks;

namespace Salix.Dapper.Cqrs.Abstractions
{

    /// <summary>
    /// Base class for <see cref="ICommand"/> implementations.
    /// Provides standard methods for Execute (using SqlStatement + Parameters properties).
    /// Provides Validation method, using special "CheckSql" SQL function. (See documentation).
    /// </summary>
    [System.Diagnostics.DebuggerDisplay("{DebuggerDisplay,nq}")]
    public abstract class MsSqlCommandBase
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
        /// Executes the <see cref="SqlStatement"/> onto SQL Server Session asynchronously.
        /// </summary>
        /// <param name="session">The MS SQL connection session.</param>
        public virtual async Task ExecuteAsync(IDatabaseSession session) => await session.ExecuteAsync(this.SqlStatement, this.Parameters);

        /// <summary>
        /// Executes the <see cref="SqlStatement"/> onto SQL Server Session synchronously.
        /// </summary>
        /// <param name="session">The MS SQL connection session.</param>
        public virtual void Execute(IDatabaseSession session) => session.Execute(this.SqlStatement, this.Parameters);

        /// <summary>
        /// Validates the SQL statement against database engine for its syntax and structural correctness.
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

        public override string ToString() => this.SqlStatement.ToShortSql();

        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        private string DebuggerDisplay => this.SqlStatement.ToShortSql();
    }

    /// <summary>
    /// Base class for <see cref="ICommand{T}"/> implementations.
    /// Provides standard methods for Execute (using SqlStatement + Parameters properties).
    /// Provides Validation method, using special "CheckSql" SQL function. (See documentation).
    /// </summary>
    [System.Diagnostics.DebuggerDisplay("{DebuggerDisplay,nq}")]
    public abstract class MsSqlCommandBase<T>
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
        /// Executes the <see cref="SqlStatement"/> onto SQL Server Session asynchronously.
        /// </summary>
        /// <param name="session">The MS SQL connection session.</param>
        public virtual async Task<T> ExecuteAsync(IDatabaseSession session) => await session.ExecuteAsync<T>(this.SqlStatement, this.Parameters);

        /// <summary>
        /// Executes the <see cref="SqlStatement"/> onto SQL Server Session synchronously.
        /// </summary>
        /// <param name="session">The MS SQL connection session.</param>
        public virtual T Execute(IDatabaseSession session) => session.Execute<T>(this.SqlStatement, this.Parameters);

        /// <summary>
        /// Validates the SQL statement against database engine for its syntax and structural correctness.
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

        public override string ToString() => this.SqlStatement.ToShortSql();

        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        private string DebuggerDisplay => this.SqlStatement.ToShortSql();
    }
}
