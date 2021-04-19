using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Dapper;
using Salix.Dapper.Cqrs.Abstractions;

namespace Salix.Dapper.Cqrs.MsSql
{
    /// <summary>
    /// Allows to assign SQL command parameters dynamically.
    /// </summary>
    /// <seealso cref="SqlMapper.IDynamicParameters" />
    public class DynamicParameters : IDynamicParameters, SqlMapper.IDynamicParameters
    {
        private readonly Dictionary<string, DynamicParameterInfo> _parameters = new();

        /// <summary>
        /// All the names of the parameters in the bag, use Get to yank them out.
        /// </summary>
        public IEnumerable<string> ParameterNames => _parameters.Select(p => p.Key);

        /// <summary>
        /// Add a parameter to this dynamic parameter list.
        /// </summary>
        /// <param name="name">Name of parameter.</param>
        /// <param name="fieldType">Database type of parameter.</param>
        /// <param name="value">Value for the parameter.</param>
        /// <param name="direction">In or Out parameter.</param>
        /// <param name="size">Database field size.</param>
        public void Add(string name, SqlDbType? fieldType = null, object value = null, ParameterDirection direction = ParameterDirection.Input, int? size = null)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("Cannot create Dynamic parameter with empty name.", nameof(name));
            }

            _parameters[CleanParameterName(name)] = new DynamicParameterInfo() { Name = name, Value = value, ParameterDirection = direction, SqlDbType = fieldType, Size = size };
        }

        /// <summary>
        /// Add all the parameters needed to the command just before it executes (Dapper).
        /// </summary>
        /// <param name="command">The raw command prior to execution.</param>
        /// <param name="identity">Information about the query.</param>
        void SqlMapper.IDynamicParameters.AddParameters(IDbCommand command, SqlMapper.Identity identity) => this.AddParameters(command);

        /// <summary>
        /// Add all the parameters needed to the command just before it executes.
        /// </summary>
        /// <param name="command">The raw command prior to execution.</param>
        /// <exception cref="ArgumentException">Assigning Parameter fails.</exception>
        public void AddParameters(IDbCommand command)
        {
            if (command == null)
            {
                throw new ArgumentNullException(nameof(command));
            }

            foreach (DynamicParameterInfo param in _parameters.Values)
            {
                string name = CleanParameterName(param.Name);
                var sqlCommand = (SqlCommand)command;
                bool doesParameterAlreadyExist = sqlCommand.Parameters.Contains(name);
                SqlParameter sqlParameter;
                if (doesParameterAlreadyExist)
                {
                    sqlParameter = sqlCommand.Parameters[name];
                }
                else
                {
                    sqlParameter = sqlCommand.CreateParameter();
                    sqlParameter.ParameterName = name;
                    if (param.SqlDbType.HasValue)
                    {
                        sqlParameter.SqlDbType = param.SqlDbType.Value;
                    }
                }

                object val = param.Value;
                try
                {
                    sqlParameter.Value = val ?? DBNull.Value;
                }
                catch (ArgumentException ex)
                {
                    throw new ArgumentException($"Assigning value to SqlDynamicParameter {name} (DbType: {sqlParameter.DbType}) with value {val?.ToString() ?? "NULL"} failed", ex);
                }

                sqlParameter.Direction = param.ParameterDirection;
                string valueAsString = val as string;
                if (valueAsString?.Length <= 4000)
                {
                    sqlParameter.Size = 4000;
                }

                if (param.Size != null)
                {
                    sqlParameter.Size = param.Size.Value;
                }

                if (!doesParameterAlreadyExist)
                {
                    command.Parameters.Add(sqlParameter);
                }

                param.AttachedParam = sqlParameter;
            }
        }

        /// <summary>
        /// Removes parameter syntax character, if added by developer.
        /// </summary>
        /// <param name="name">The name of parameter.</param>
        /// <returns>Cleared parameter.</returns>
        private static string CleanParameterName(string name) => !string.IsNullOrEmpty(name) && name[0] == '@' ? name.Substring(1) : name;
    }
}
