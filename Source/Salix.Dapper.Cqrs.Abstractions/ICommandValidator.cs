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
    }
}
