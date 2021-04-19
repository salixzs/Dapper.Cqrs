using System;
using System.Diagnostics;

namespace Salix.Dapper.Cqrs.Abstractions
{
    /// <summary>
    /// SQL Statement Syntax exception for Dapper magic string statement validations.
    /// </summary>
    [DebuggerDisplay("{DebuggerDisplay,nq}")]

    public class DatabaseStatementSyntaxException : ApplicationException
    {
        /// <summary>
        /// SQL Statement Syntax exception for magic string statement validations.
        /// </summary>
        public DatabaseStatementSyntaxException()
        {
        }

        /// <summary>
        /// SQL Statement Syntax exception for magic string statement validations.
        /// </summary>
        /// <param name="message">A message of a problem in SQL statement.</param>
        public DatabaseStatementSyntaxException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// SQL Statement Syntax exception for magic string statement validations.
        /// </summary>
        /// <param name="message">A message of a problem in SQL statement.</param>
        /// <param name="sqlStatement">SQL Statement in question.</param>
        public DatabaseStatementSyntaxException(string message, string sqlStatement)
            : base(message) => this.Data.Add("SQL", sqlStatement);

        /// <summary>
        /// SQL Statement Syntax exception for magic string statement validations.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">
        /// The exception that is the cause of the current exception.
        /// If the innerException parameter is not a null reference,
        /// the current exception is raised in a catch block that handles the inner exception.
        /// </param>
        public DatabaseStatementSyntaxException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string DebuggerDisplay => this.Message;
    }
}
