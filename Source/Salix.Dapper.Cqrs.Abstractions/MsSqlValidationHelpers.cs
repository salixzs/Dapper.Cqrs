using System;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Salix.Dapper.Cqrs.Abstractions
{
    internal static class MsSqlValidationHelpers
    {
        /// <summary>
        /// Retrieves SQL Statement parameter types as string of comma-separated MS SQL types with their parameter names.
        /// Example: "@Name NVARCHAR(4000), Id INT"
        /// </summary>
        /// <param name="statementParameters">The SQL statement parameters.</param>
        internal static string GetParameterTypes(object statementParameters)
        {
            if (statementParameters == null)
            {
                return null;
            }

            Type anonType = statementParameters.GetType();
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
                    parameterType = Nullable.GetUnderlyingType(parameterInfo[index].PropertyType)?.Name;
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


        /// <summary>
        /// Gets the parameter names, types and values in copy/pastable format for SQL Server Management Studio (developer productivity).
        /// Returns all SQL Statement 
        /// </summary>
        /// <param name="statementParameters">The statement parameters.</param>
        internal static string GetParameterStatements(object statementParameters)
        {
            Type anonType = statementParameters.GetType();
            PropertyInfo[] objectProperties = anonType.GetProperties();
            if (objectProperties.Length == 0)
            {
                return string.Empty;
            }

            var propStatements = new StringBuilder();
            foreach (PropertyInfo propInfo in objectProperties)
            {
                propStatements.Append("DECLARE @").Append(propInfo.Name).Append(' ');

                string propertyTypeName = propInfo.PropertyType.Name;

                // In case parameter is of nullable type - we have to get underlying type name
                if (Nullable.GetUnderlyingType(propInfo.PropertyType) != null)
                {
                    propertyTypeName = Nullable.GetUnderlyingType(propInfo.PropertyType)?.Name;
                }

                // First - get the type
                switch (propertyTypeName)
                {
                    case "String":
                        propStatements.Append("NVARCHAR(100) = ");
                        break;
                    case "Char":
                        propStatements.Append("NCHAR(1) = ");
                        break;
                    case "Char[]":
                        propStatements.Append("NVARCHAR(1000) = ");
                        break;
                    case "Int16":
                    case "SByte":
                        propStatements.Append("SMALLINT = ");
                        break;
                    case "Int32":
                    case "UInt16":
                        propStatements.Append("INT = ");
                        break;
                    case "Int64":
                    case "UInt32":
                        propStatements.Append("BIGINT = ");
                        break;
                    case "Boolean":
                        propStatements.Append("BIT = ");
                        break;
                    case "Byte":
                        propStatements.Append("TINYINT = ");
                        break;
                    case "UInt64":
                        propStatements.Append("DECIMAL(20) = ");
                        break;
                    case "Decimal":
                        propStatements.Append("DECIMAL(29,4) = ");
                        break;
                    case "Single":
                        propStatements.Append("REAL = ");
                        break;
                    case "Double":
                        propStatements.Append("FLOAT = ");
                        break;
                    case "Guid":
                        propStatements.Append("UNIQUEIDENTIFIER = ");
                        break;
                    case "DateTime":
                    case "DateTimeOffset":
                        propStatements.Append("DATETIME = ");
                        break;
                    case "TimeSpan":
                        propStatements.Append("BIGINT = ");
                        break;
                    case "Int32[]":
                        propStatements.Append("INT[] = (");
                        break;
                    default:
                        propStatements.Append(propertyTypeName).Append(" = ");
                        break;
                }

                // Now assign value
                if (propInfo.GetValue(statementParameters) == null)
                {
                    propStatements.AppendLine("NULL;");
                    continue;
                }

                switch (propertyTypeName)
                {
                    case "String":
                        string strval = propInfo.GetValue(statementParameters).ToString();
                        if (strval.Length > 75)
                        {
                            strval = strval.Substring(0, 72) + "...";
                        }

                        propStatements.Append('\'').Append(strval).AppendLine("';");
                        break;
                    case "Char":
                    case "Char[]":
                    case "Guid":
                        propStatements.Append('\'').Append(propInfo.GetValue(statementParameters)).AppendLine("';");
                        break;
                    case "Int16":
                    case "SByte":
                    case "Int32":
                    case "UInt16":
                    case "Int64":
                    case "UInt32":
                    case "Byte":
                    case "UInt64":
                        propStatements.Append(propInfo.GetValue(statementParameters)).AppendLine(";");
                        break;
                    case "Boolean":
                        bool boolval = (bool)propInfo.GetValue(statementParameters);
                        propStatements.AppendLine(boolval ? "1;" : "0;");
                        break;
                    case "Decimal":
                        propStatements.Append(((decimal)propInfo.GetValue(statementParameters)).ToString(CultureInfo.InvariantCulture)).AppendLine(";");
                        break;
                    case "Single":
                        propStatements.Append(((float)propInfo.GetValue(statementParameters)).ToString(CultureInfo.InvariantCulture)).AppendLine(";");
                        break;
                    case "Double":
                        propStatements.Append(((double)propInfo.GetValue(statementParameters)).ToString(CultureInfo.InvariantCulture)).AppendLine(";");
                        break;
                    case "DateTime":
                        var dtval = (DateTime)propInfo.GetValue(statementParameters);
                        propStatements.Append('\'').Append(dtval.ToString("u")).AppendLine("';");
                        break;
                    case "DateTimeOffset":
                        var dtoval = (DateTimeOffset)propInfo.GetValue(statementParameters);
                        propStatements.Append('\'').Append(dtoval.ToString("u")).AppendLine("';");
                        break;
                    case "TimeSpan":
                        long tsval = ((TimeSpan)propInfo.GetValue(statementParameters)).Ticks;
                        propStatements.Append(tsval).AppendLine(";");
                        break;
                    case "Int32[]":
                        propStatements.Append('(').Append(string.Join(",", (int[])propInfo.GetValue(statementParameters))).AppendLine(");");
                        break;
                    default:
                        propStatements.Append('\'').Append(propInfo.GetValue(statementParameters)).AppendLine("';");
                        break;
                }
            }

            return propStatements.ToString();
        }
    }
}
