using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Logging;
using Salix.Dapper.Cqrs.Abstractions;

namespace Salix.Dapper.Cqrs.MsSql
{
    /// <inheritdoc/>
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class SqlDatabaseSession : IDatabaseSession
    {
        private readonly ILogger<SqlDatabaseSession> _logger;
        private readonly IMsSqlContext _context;

        /// <summary>
        /// Database session object, used by <see cref="CommandQueryContext"/> which is passed as parameter into "Execute" method of
        /// <see cref="IQuery{T}"/> or <see cref="ICommand"/> implementations and used there to use methods in this class for actual data manipulation.
        /// </summary>
        /// <param name="context">The SQL database context, containing the connection itself and all its manipulation methods.</param>
        /// <param name="logger">
        /// The logger (here just for Trace level to add method call trace into log).
        /// Log level is checked before using LogTrace to avoid SQL shortening procedure if it is not required.
        /// </param>
        public SqlDatabaseSession(IMsSqlContext context, ILogger<SqlDatabaseSession> logger)
        {
            _logger = logger;
            _context = context;
        }

        /// <inheritdoc/>
        public TimeSpan ExecutionTime { get; private set; }

        /// <inheritdoc/>
        public virtual IEnumerable<T> Query<T>(string sqlQuery, object parameter = null)
        {
            if (_logger.IsEnabled(LogLevel.Trace))
            {
                _logger.LogTrace("Query<T>({Query}) called with expected return of type IEnumerable<{TypeParam}>.", sqlQuery.ToShortSql(), typeof(T).Name);
            }

            IEnumerable<T> result = _context.ExecuteSql(transaction => _context.Connection.Query<T>(sqlQuery, parameter, transaction));
            this.ExecutionTime = _context.ExecutionTime;
            return result;
        }

        /// <inheritdoc/>
        public virtual async Task<IEnumerable<T>> QueryAsync<T>(string sqlQuery, object parameter = null)
        {
            if (_logger.IsEnabled(LogLevel.Trace))
            {
                _logger.LogTrace("QueryAsync<T>({Query}) called with expected return of type IEnumerable<{TypeParam}>.", sqlQuery.ToShortSql(), typeof(T).Name);
            }

            IEnumerable<T> result = await _context.ExecuteSql(transaction => _context.Connection.QueryAsync<T>(sqlQuery, parameter, transaction));
            this.ExecutionTime = _context.ExecutionTime;
            return result;
        }

        /// <inheritdoc/>
        public virtual T QueryFirstOrDefault<T>(string sqlQuery, object parameter = null)
        {
            if (_logger.IsEnabled(LogLevel.Trace))
            {
                _logger.LogTrace("QueryFirstOrDefault<T>({Query}) called with expected return of type {TypeParam}.", sqlQuery.ToShortSql(), typeof(T).Name);
            }

            T result = _context.ExecuteSql(transaction => _context.Connection.QueryFirstOrDefault<T>(sqlQuery, parameter, transaction));
            this.ExecutionTime = _context.ExecutionTime;
            return result;
        }

        /// <inheritdoc/>
        public virtual async Task<T> QueryFirstOrDefaultAsync<T>(string sqlQuery, object parameter = null)
        {
            if (_logger.IsEnabled(LogLevel.Trace))
            {
                _logger.LogTrace("QueryFirstOrDefaultAsync<T>({Query}) called with expected return of type {TypeParam}.", sqlQuery.ToShortSql(), typeof(T).Name);
            }

            T result = await _context.ExecuteSql(transaction => _context.Connection.QueryFirstOrDefaultAsync<T>(sqlQuery, parameter, transaction));
            this.ExecutionTime = _context.ExecutionTime;
            return result;
        }

        /// <inheritdoc/>
        public virtual void Execute(string sql, object parameter = null)
        {
            if (_logger.IsEnabled(LogLevel.Trace))
            {
                _logger.LogTrace("Execute({Sql}) issued for SqlDatabaseSession.", sql.ToShortSql());
            }

            _context.ExecuteSql(transaction => _context.Connection.Execute(sql, parameter, transaction));
            this.ExecutionTime = _context.ExecutionTime;
        }

        /// <inheritdoc/>
        public virtual T Execute<T>(string sql, object parameter)
        {
            if (_logger.IsEnabled(LogLevel.Trace))
            {
                _logger.LogTrace("Execute<T>({Sql}) called with expected return of type {TypeParam}.", sql.ToShortSql(), typeof(T).Name);
            }

            T result = _context.ExecuteSql(transaction => _context.Connection.QueryFirstOrDefault<T>(sql, parameter, transaction));
            this.ExecutionTime = _context.ExecutionTime;
            return result;
        }

        /// <inheritdoc/>
        public virtual async Task ExecuteAsync(string sql, object parameter = null)
        {
            if (_logger.IsEnabled(LogLevel.Trace))
            {
                _logger.LogTrace("ExecuteAsync({Sql}) issued for SqlDatabaseSession.", sql.ToShortSql());
            }

            await _context.ExecuteSql(transaction => _context.Connection.ExecuteAsync(sql, parameter, transaction));
            this.ExecutionTime = _context.ExecutionTime;
        }

        /// <inheritdoc/>
        public virtual async Task<T> ExecuteAsync<T>(string sql, object parameter)
        {
            if (_logger.IsEnabled(LogLevel.Trace))
            {
                _logger.LogTrace("Execute<T>({Sql}) called with expected return of type {TypeParam}.", sql.ToShortSql(), typeof(T).Name);
            }

            T result = await _context.ExecuteSql(transaction => _context.Connection.QueryFirstOrDefaultAsync<T>(sql, parameter, transaction));
            this.ExecutionTime = _context.ExecutionTime;
            return result;
        }

        /// <inheritdoc/>
        public void RollbackTransaction() => _context.RollbackTransaction();

        /// <inheritdoc/>
        public void CommitTransaction() => _context.CommitTransaction();

        /// <inheritdoc/>
        public IDynamicParameters CreateDynamicParameters() => new DynamicParameters();

        /// <summary>
        /// String which represents important values of this class instance during debugging session.
        /// </summary>
        public override string ToString() => $"MsSqlDB Session:{this.GetHashCode().ToString("D", CultureInfo.InvariantCulture)}, {_context}";

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        [ExcludeFromCodeCoverage]
        private string DebuggerDisplay => this.ToString();
    }
}
