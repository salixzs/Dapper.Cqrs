using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;
using Microsoft.Extensions.Logging;

namespace Salix.Dapper.Cqrs.MsSql.Testing.XUnit
{
    /// <summary>
    /// Data contract for Logging statement, used in XUnitLogger.
    /// </summary>
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    [ExcludeFromCodeCoverage]
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

        /// <summary>
        /// Logging statement string representation (human eyes view only).
        /// </summary>
        public override string ToString()
        {
            var dbgView = new StringBuilder();
            if (this.Level != LogLevel.None)
            {
                dbgView.Append($"[{this.Level.ToString().ToUpper(CultureInfo.InvariantCulture)}] ");
            }

            dbgView.Append(this.Message);
            if (this.Exception != null)
            {
                dbgView.Append($"; EXC: {this.Exception.Message}");
            }

            return dbgView.ToString();
        }

        /// <summary>
        /// Displays DB object column definition in Debug screen. (Only for development purposes).
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string DebuggerDisplay => this.ToString();
    }
}
