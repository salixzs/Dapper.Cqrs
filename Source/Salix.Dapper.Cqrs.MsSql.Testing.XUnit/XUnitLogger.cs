using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Salix.Dapper.Cqrs.MsSql.Testing.XUnit
{
    /// <summary>
    /// Microsoft ILogger implementation, which can be used in XUnit tests as stub for real logger.
    /// Writes messages to Output stream and also stores in internal property <seealso cref="LogStatements"/> so they can be asserted.
    /// </summary>
    /// <typeparam name="T">Logger class type (Typed logger)</typeparam>
    /// <seealso cref="ILogger{T}" />
    [ExcludeFromCodeCoverage]
    public sealed class XUnitLogger<T> : ILogger<T>, IDisposable
    {
        private readonly IMessageSink _messageSink;
        private ITestOutputHelper _outputHelper;

        /// <summary>
        /// Logged message store.
        /// </summary>
        public List<LoggingStatement> LogStatements { get; private set; } = new List<LoggingStatement>();

        /// <summary>
        /// Microsoft ILogger implementation, which can be used in XUnit tests as stub for real logger.
        /// Writes messages to Output stream and also stores in internal property <seealso cref="LogStatements" /> so they can be asserted.
        /// </summary>
        /// <param name="output">Accepting MessageSink as output from Fixtures.</param>
        public XUnitLogger(IMessageSink output) => _messageSink = output;

        /// <summary>
        /// Microsoft ILogger implementation, which can be used in XUnit tests as stub for real logger.
        /// Writes messages to Output stream and also stores in internal property <seealso cref="LogStatements" /> so they can be asserted.
        /// </summary>
        /// <param name="output">Accepting MessageSink as output from Fixtures.</param>
        public XUnitLogger(ITestOutputHelper output) => _outputHelper = output;

        /// <summary>
        /// Sets the output helper for test logging separately.
        /// </summary>
        /// <param name="output">The output helper from XUnit engine.</param>
        public XUnitLogger<T> SetOutputHelper(ITestOutputHelper output)
        {
            if (output != null)
            {
                _outputHelper = output;
            }

            return this;
        }

        /// <summary>
        /// Logs the message at specified log level.
        /// </summary>
        /// <typeparam name="TState">The type of the state.</typeparam>
        /// <param name="logLevel">The logging level used.</param>
        /// <param name="eventId">The event identifier.</param>
        /// <param name="state">The state = message.</param>
        /// <param name="exception">The exception, if it is a part of logging statement.</param>
        /// <param name="formatter">The logging statement formatter function.</param>
        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception exception,
            Func<TState, Exception, string> formatter)
        {
            this.LogStatements.Add(new LoggingStatement { Level = logLevel, Message = formatter.Invoke(state, exception), Exception = exception });

            if (_outputHelper != null)
            {
                // Need to wrap as when called from dispose method it throws "There is no currently active test" InvalidOperationException.
                try
                {
                    _outputHelper.WriteLine(state.ToString());
                    return;
                }
                catch
                {
                    // Happens in Dispose() - should be handled by message sink below or (huh) swallowed into void
                }
            }

            if (_messageSink != null)
            {
                _messageSink.OnMessage(new DiagnosticMessage(state.ToString()));
            }
        }

        /// <summary>
        /// Determines whether the specified log level is enabled.
        /// </summary>
        /// <param name="logLevel">The log level.</param>
        public bool IsEnabled(LogLevel logLevel) => true;

        /// <summary>
        /// Begins the scope.
        /// </summary>
        /// <typeparam name="TState">The type of the state.</typeparam>
        /// <param name="state">The state.</param>
        public IDisposable BeginScope<TState>(TState state) => this;

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
        }
    }

    /// <summary>
    /// Microsoft ILogger implementation, which can be used in XUnit tests as stub for real logger.
    /// Writes messages to Output stream and also stores in internal property <seealso cref="LogStatements"/> so they can be asserted.
    /// </summary>
    /// <seealso cref="ILogger" />
    [ExcludeFromCodeCoverage]
    public sealed class XUnitLogger : ILogger, IDisposable
    {
        private readonly IMessageSink _messageSink;
        private ITestOutputHelper _outputHelper;

        /// <summary>
        /// Logged message store.
        /// </summary>
        public List<LoggingStatement> LogStatements { get; private set; } = new List<LoggingStatement>();

        /// <summary>
        /// Microsoft ILogger implementation, which can be used in XUnit tests as stub for real logger.
        /// Writes messages to Output stream and also stores in internal property <seealso cref="LogStatements" /> so they can be asserted.
        /// </summary>
        /// <param name="output">Accepting MessageSink as output from Fixtures.</param>
        public XUnitLogger(IMessageSink output) => _messageSink = output;

        /// <summary>
        /// Microsoft ILogger implementation, which can be used in XUnit tests as stub for real logger.
        /// Writes messages to Output stream and also stores in internal property <seealso cref="LogStatements" /> so they can be asserted.
        /// </summary>
        /// <param name="output">Accepting MessageSink as output from Fixtures.</param>
        public XUnitLogger(ITestOutputHelper output) => _outputHelper = output;

        /// <summary>
        /// Sets the output helper for test logging separately.
        /// </summary>
        /// <param name="output">The output helper from XUnit engine.</param>
        public XUnitLogger SetOutputHelper(ITestOutputHelper output)
        {
            if (output != null)
            {
                _outputHelper = output;
            }

            return this;
        }

        /// <summary>
        /// Logs the message at specified log level.
        /// </summary>
        /// <typeparam name="TState">The type of the state.</typeparam>
        /// <param name="logLevel">The log level.</param>
        /// <param name="eventId">The event identifier.</param>
        /// <param name="state">The state.</param>
        /// <param name="exception">The exception.</param>
        /// <param name="formatter">The formatter.</param>
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            this.LogStatements.Add(new LoggingStatement { Level = logLevel, Message = formatter.Invoke(state, exception), Exception = exception });

            if (_outputHelper != null)
            {
                // Need to wrap as when called from dispose method it throws "There is no currently active test" InvalidOperationException.
                try
                {
                    _outputHelper.WriteLine(state.ToString());
                }
                catch
                {
                    // Happens in Dispose() - should be handled by message sink below or (huh) swallowed into void
                }

                return;
            }

            if (_messageSink != null)
            {
                _messageSink.OnMessage(new DiagnosticMessage(state.ToString()));
            }
        }

        /// <summary>
        /// Determines whether the specified log level is enabled.
        /// </summary>
        /// <param name="logLevel">The log level.</param>
        public bool IsEnabled(LogLevel logLevel) => true;

        /// <summary>
        /// Begins the scope.
        /// </summary>
        /// <typeparam name="TState">The type of the state.</typeparam>
        /// <param name="state">The state.</param>
        public IDisposable BeginScope<TState>(TState state) => this;

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
        }
    }
}
