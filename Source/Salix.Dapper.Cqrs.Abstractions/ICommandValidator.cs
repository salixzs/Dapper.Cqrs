namespace Salix.Dapper.Cqrs.Abstractions
{
    /// <summary>
    /// Additional Interface for <seealso cref="ICommand"/> and <seealso cref="ICommand{T}"/> implementations to validate their SQL statements against database.
    /// </summary>
    public interface ICommandValidator
    {
        /// <summary>
        /// Property to hold SQL Statement used for Command (INSERT, UPDATE, DELETE) class.
        /// </summary>
        string SqlStatement { get; }

        /// <summary>
        /// Anonymous object of SQL Command parameters.
        /// Should be overridden in derived class if needed.
        /// </summary>
        object Parameters { get; }

        /// <summary>
        /// Executes Statement inside ICommand against SQL database engine to validate its correctness.
        /// In case of invalid statement will throw exception.
        /// </summary>
        /// <param name="session">The SQL database connection session.</param>
        void Validate(IDatabaseSession session);
    }
}
