namespace Salix.Dapper.Cqrs.Abstractions
{
    /// <summary>
    /// Interface for Query which adds Validate method to query. Demands SQL statement and parameters for it to be accessible.
    /// </summary>
    public interface IQueryValidator
    {
        /// <summary>
        /// Property to hold SQL Statement used for Query class.
        /// </summary>
        string SqlStatement { get; }

        /// <summary>
        /// Anonymous object of SQL Query parameters.
        /// Should be overridden in derived class if needed.
        /// </summary>
        object Parameters { get; }

        /// <summary>
        /// Executes Query inside IQuery against database engine to validate its correctness.
        /// In case of invalid statement will throw exception.
        /// </summary>
        /// <param name="session">The database connection session.</param>
        void Validate(IDatabaseSession session);
    }
}
