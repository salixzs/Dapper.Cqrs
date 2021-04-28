using System;
using Microsoft.Extensions.Logging;

namespace Salix.Dapper.Cqrs.MsSql.Testing.XUnit
{
    /// <summary>
    /// Data contract for Logging statement, used in XUnitLogger.
    /// </summary>
    public class LoggingStatement
    {
        /// <summary>
        /// Log message.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Log level used for log statement.
        /// </summary>
        public LogLevel Level { get; set; }

        /// <summary>
        /// In case of included exception this contains exception object.
        /// </summary>
        public Exception Exception { get; set; }
    }
}
