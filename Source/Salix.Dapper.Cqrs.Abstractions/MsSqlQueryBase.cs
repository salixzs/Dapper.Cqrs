using System;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Salix.Dapper.Cqrs.Abstractions
{
    /// <summary>
    /// Base class for IQuery implementors to cover for common functionalities of common (mostly CRUD) classes with MS SQL Server.
    /// </summary>
    [System.Diagnostics.DebuggerDisplay("{DebuggerDisplay,nq}")]
    public abstract class MsSqlQueryBase<T>
    {
        /// <summary>
        /// Property to hold SQL Statement used for Query class.
        /// Should be overridden in derived class.
        /// </summary>
        public virtual string SqlStatement => "SQL Statement is not overridden in inheriting class";

        /// <summary>
        /// Anonymous object of SqlQuery parameters.
        /// Should be overridden in derived class if needed.
        /// </summary>
        public virtual object Parameters => null;

        /// <summary>
        /// Use during Code Debugging (by developer) to get statements, which can be copy-pasted into SQL Management Studio for checking against database.
        /// </summary>
        public string RealSqlStatement => $@"{this.PropertyStatements}
{this.SqlStatement}";

        /// <summary>
        /// Validates the Query SQL statement against database engine for its syntax and structural correctness.
        /// Validates only statement in <see cref="SqlStatement"/>. If derived class contains more than this one
        /// SQL statement - override this implementation and add all statement validations to it.
        /// </summary>
        /// <param name="databaseSession">The database session object.</param>
        /// <exception cref="DatabaseStatementSyntaxException">Throws when <see cref="SqlStatement"/> is incorrect.</exception>
        public virtual void Validate(IDatabaseSession databaseSession)
        {
            string result = databaseSession.QueryFirstOrDefault<string>("SELECT dbo.CheckSql(@tsql, @parameterTypes)", new { tsql = this.SqlStatement, parameterTypes = this.ParameterTypes });
            if (result != "OK")
            {
                throw new DatabaseStatementSyntaxException(result, this.SqlStatement);
            }
        }

        /// <inheritdoc/>
        public virtual T Execute(IDatabaseSession session) => throw new NotImplementedException("For MS SQL Server best use ExecuteAsync() or implement this method (overdrive) in IQuery class.");

        /// <summary>
        /// Builds SQL Syntax for input parameter setup.
        /// </summary>
        private string PropertyStatements
        {
            get
            {
                Type anonType = this.Parameters.GetType();
                PropertyInfo[] objectProperties = anonType.GetProperties();
                if (objectProperties.Length == 0)
                {
                    return string.Empty;
                }

                var propStatments = new StringBuilder();
                foreach (PropertyInfo propInfo in objectProperties)
                {
                    propStatments.Append("DECLARE @").Append(propInfo.Name).Append(' ');

                    var propertyTypeName = propInfo.PropertyType.Name;

                    // In case parameter is of nullable type - we have to get underlying type name
                    if (Nullable.GetUnderlyingType(propInfo.PropertyType) != null)
                    {
                        propertyTypeName = Nullable.GetUnderlyingType(propInfo.PropertyType).Name;
                    }

                    // First - get the type
                    switch (propertyTypeName)
                    {
                        case "String":
                            propStatments.Append("NVARCHAR(100) = ");
                            break;
                        case "Char":
                            propStatments.Append("NCHAR(1) = ");
                            break;
                        case "Char[]":
                            propStatments.Append("NVARCHAR(1000) = ");
                            break;
                        case "Int16":
                        case "SByte":
                            propStatments.Append("SMALLINT = ");
                            break;
                        case "Int32":
                        case "UInt16":
                            propStatments.Append("INT = ");
                            break;
                        case "Int64":
                        case "UInt32":
                            propStatments.Append("BIGINT = ");
                            break;
                        case "Boolean":
                            propStatments.Append("BIT = ");
                            break;
                        case "Byte":
                            propStatments.Append("TINYINT = ");
                            break;
                        case "UInt64":
                            propStatments.Append("DECIMAL(20) = ");
                            break;
                        case "Decimal":
                            propStatments.Append("DECIMAL(29,4) = ");
                            break;
                        case "Single":
                            propStatments.Append("REAL = ");
                            break;
                        case "Double":
                            propStatments.Append("FLOAT = ");
                            break;
                        case "Guid":
                            propStatments.Append("UNIQUEIDENTIFIER = ");
                            break;
                        case "DateTime":
                        case "DateTimeOffset":
                            propStatments.Append("DATETIME = ");
                            break;
                        case "TimeSpan":
                            propStatments.Append("BIGINT = ");
                            break;
                        case "Int32[]":
                            propStatments.Append("INT[] = (");
                            break;
                        default:
                            propStatments.Append(propertyTypeName).Append(" = ");
                            break;
                    }

                    // Now assign value
                    if (propInfo.GetValue(this.Parameters) == null)
                    {
                        propStatments.AppendLine("NULL;");
                        continue;
                    }

                    switch (propertyTypeName)
                    {
                        case "String":
                            string strval = propInfo.GetValue(this.Parameters).ToString();
                            if (strval.Length > 75)
                            {
                                strval = strval.Substring(0, 72) + "...";
                            }

                            propStatments.Append('\'').Append(strval).AppendLine("';");
                            break;
                        case "Char":
                            propStatments.Append('\'').Append(propInfo.GetValue(this.Parameters)).AppendLine("';");
                            break;
                        case "Char[]":
                            propStatments.Append('\'').Append(propInfo.GetValue(this.Parameters)).AppendLine("';");
                            break;
                        case "Int16":
                        case "SByte":
                            propStatments.Append(propInfo.GetValue(this.Parameters)).AppendLine(";");
                            break;
                        case "Int32":
                        case "UInt16":
                            propStatments.Append(propInfo.GetValue(this.Parameters)).AppendLine(";");
                            break;
                        case "Int64":
                        case "UInt32":
                            propStatments.Append(propInfo.GetValue(this.Parameters)).AppendLine(";");
                            break;
                        case "Boolean":
                            bool boolval = (bool)propInfo.GetValue(this.Parameters);
                            propStatments.AppendLine(boolval ? "1;" : "0;");
                            break;
                        case "Byte":
                            propStatments.Append(propInfo.GetValue(this.Parameters)).AppendLine(";");
                            break;
                        case "UInt64":
                            propStatments.Append(propInfo.GetValue(this.Parameters)).AppendLine(";");
                            break;
                        case "Decimal":
                            propStatments.Append(((decimal)propInfo.GetValue(this.Parameters)).ToString(CultureInfo.InvariantCulture)).AppendLine(";");
                            break;
                        case "Single":
                            propStatments.Append(((float)propInfo.GetValue(this.Parameters)).ToString(CultureInfo.InvariantCulture)).AppendLine(";");
                            break;
                        case "Double":
                            propStatments.Append(((double)propInfo.GetValue(this.Parameters)).ToString(CultureInfo.InvariantCulture)).AppendLine(";");
                            break;
                        case "Guid":
                            propStatments.Append('\'').Append(propInfo.GetValue(this.Parameters)).AppendLine("';");
                            break;
                        case "DateTime":
                            var dtval = (DateTime)propInfo.GetValue(this.Parameters);
                            propStatments.Append('\'').Append(dtval.ToString("u")).AppendLine("';");
                            break;
                        case "DateTimeOffset":
                            var dtoval = (DateTimeOffset)propInfo.GetValue(this.Parameters);
                            propStatments.Append('\'').Append(dtoval.ToString("u")).AppendLine("';");
                            break;
                        case "TimeSpan":
                            var tsval = ((TimeSpan)propInfo.GetValue(this.Parameters)).Ticks;
                            propStatments.Append(tsval).AppendLine(";");
                            break;
                        case "Int32[]":
                            propStatments.Append('(').Append(string.Join(",", (int[])propInfo.GetValue(this.Parameters))).AppendLine(");");
                            break;
                        default:
                            propStatments.Append('\'').Append(propInfo.GetValue(this.Parameters)).AppendLine("';");
                            break;
                    }
                }

                return propStatments.ToString();
            }
        }

        private string ParameterTypes
        {
            get
            {
                if (this.Parameters == null)
                {
                    return null;
                }

                Type anonType = this.Parameters.GetType();
                PropertyInfo[] parameterInfo = anonType.GetProperties();
                if (parameterInfo.Length == 0)
                {
                    return null;
                }

                string[] parameterDefinitions = new string[parameterInfo.Length];
                for (int index = 0; index < parameterInfo.Length; index++)
                {
                    string parameterName = parameterInfo[index].Name;
                    string parameterType = parameterInfo[index].PropertyType.Name;

                    // In case parameter is of nullable type - we have to get underlying type name
                    if (Nullable.GetUnderlyingType(parameterInfo[index].PropertyType) != null)
                    {
                        parameterType = Nullable.GetUnderlyingType(parameterInfo[index].PropertyType).Name;
                    }

                    parameterDefinitions[index] = parameterType switch
                    {
                        "String" => $"@{parameterName} NVARCHAR(4000)",
                        "Char" => $"@{parameterName} NCHAR(1)",
                        "Char[]" => $"@{parameterName} NVARCHAR(4000)",
                        "DateTime" => $"@{parameterName} DATETIME",
                        "DateTimeOffset" => $"@{parameterName} DATETIME",
                        "TimeSpan" => $"@{parameterName} BIGINT",
                        "Guid" => $"@{parameterName} UNIQUEIDENTIFIER",
                        "Byte" => $"@{parameterName} TINYINT",
                        "SByte" => $"@{parameterName} SMALLINT",
                        "Int16" => $"@{parameterName} SMALLINT",
                        "Int32" => $"@{parameterName} INT",
                        "Int64" => $"@{parameterName} BIGINT",
                        "UInt16" => $"@{parameterName} INT",
                        "UInt32" => $"@{parameterName} BIGINT",
                        "UInt64" => $"@{parameterName} DECIMAL(20)",
                        "Boolean" => $"@{parameterName} BIT",
                        "Decimal" => $"@{parameterName} DECIMAL(29,4)",
                        "Double" => $"@{parameterName} FLOAT",
                        "Single" => $"@{parameterName} REAL",
                        _ => null,
                    };
                }

                return string.Join(",", parameterDefinitions.Where(parameter => !string.IsNullOrEmpty(parameter)));
            }
        }

        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        private string DebuggerDisplay => this.SqlStatement.ToShortSql();
    }
}
