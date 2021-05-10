using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using Salix.Dapper.Cqrs.Abstractions;

namespace Salix.Dapper.Cqrs.MsSql.Testing.XUnit
{
    /// <summary>
    /// Test helpers to work with Command and Query classes (CQRS).
    /// </summary>
    [ExcludeFromCodeCoverage]
    public static class HelperQueryCommandClasses
    {
        /// <summary>
        /// Creates Dummy parameter array for class instantiation through reflection.
        /// </summary>
        /// <param name="classType">Type of IQuery/ICommand (Validatable) class to be instantiated.</param>
        /// <returns>Dummy general parameter array for First() class constructor</returns>
        public static object[] CreateDummyParametersForType(Type classType)
        {
            // To supply specific parameters - add this method to your query class:
            // private static object[] SupplyTestParameters()
            // and return object array with all parameters with necessary values to be able to successfully test your query class.
            MethodInfo parameterSettingMethod = classType.GetMethod(
                "SupplyTestParameters",
                BindingFlags.NonPublic | BindingFlags.Static);
            if (parameterSettingMethod != null)
            {
                return parameterSettingMethod.Invoke(null, null) as object[];
            }

            // For majority of Query/Command classes...
            // no specific parameters needed - compose necessary dummy parameters for Class instantiation (parameter values does not matter)
            ParameterInfo[] parameterInfo = classType.GetConstructors()[0].GetParameters();
            object[] parameters = new object[parameterInfo.Length];
            for (int index = 0; index < parameterInfo.Length; index++)
            {
                if (parameterInfo[index].ParameterType.Name == "String")
                {
                    // Handling cases when SQL Select may come in as parameter
                    parameters[index] =
                        parameterInfo[index].Name.IndexOf("QUERY", StringComparison.InvariantCultureIgnoreCase) > 0
                            ? "SELECT Id FROM FormData"
                            : "123-ABC-456";

                    continue;
                }

                // Checking whether it is collection
                bool collectionTypeFound = false;
                foreach (Type interfaceType in parameterInfo[index].ParameterType.GetInterfaces())
                {
                    if (interfaceType.IsGenericType
                        && interfaceType.GetGenericTypeDefinition() == typeof(IList<>))
                    {
                        Type itemType = parameterInfo[index].ParameterType.GetGenericArguments()[0];
                        Type listType = typeof(List<>).MakeGenericType(itemType);
                        parameters[index] = Activator.CreateInstance(listType);
                        collectionTypeFound = true;
                        break;
                    }
                }

                if (collectionTypeFound)
                {
                    continue;
                }

                // Nullable class can easily be null
                if (Nullable.GetUnderlyingType(parameterInfo[index].ParameterType) != null
                        || parameterInfo[index].ParameterType.IsClass)
                {
                    parameters[index] = null;
                    continue;
                }

                // Normal class - just create default instance of it.
                if (parameterInfo[index].ParameterType.IsClass)
                {
                    parameters[index] = Activator.CreateInstance(parameterInfo[index].ParameterType);
                    continue;
                }

                parameters[index] = parameterInfo[index].ParameterType.Name switch
                {
                    "DateTime" or "DateTimeOffset" => DateTime.Now,
                    "TimeSpan" => new TimeSpan(0, 0, 9, 59),
                    "Guid" => Guid.NewGuid(),
                    "Byte" or "SByte" or "Int32" or "Int16" or "Int64" or "UInt32" or "UInt16" or "UInt64" => 1,
                    "Boolean" => false,
                    "Char" => 'c',
                    "Decimal" => 1M,
                    "Double" => 1D,
                    "float" or "Single" => 1F,
                    _ => null,
                };
            }

            return parameters;
        }

        /// <summary>
        /// Attribute based data supply for SqlValidation generic MEGA-test.
        /// This loads all class names, which are implementing <seealso cref="IQuery{T}"/> interface
        /// and are also implementing IQueryValidator.
        /// </summary>
        public static IEnumerable<object[]> QueryValidatorClassesForAssemblyType(Type typeFromQueryAssembly)
        {
            // Get Queries project assembly, then get all IQueryValidator implementations from it
            IEnumerable<Type> allQueries = typeFromQueryAssembly.Assembly
                .GetTypes()
                .Where(t => t.IsPublic
                            && t.IsClass
                            && !t.ContainsGenericParameters
                            && t.GetInterfaces().Contains(typeof(IQueryValidator)));
            foreach (Type assemblyClass in allQueries)
            {
                yield return new object[] { assemblyClass };
            }
        }

        /// <summary>
        /// Attribute based data supply for SqlValidation generic MEGA-test.
        /// This loads all class names, which are implementing <seealso cref="ICommand{T}"/> interface
        /// and are also implementing <seealso cref="ICommandValidator"/>/
        /// </summary>
        public static IEnumerable<object[]> CommandValidatorClassesForAssemblyType(Type typeFromCommandAssembly)
        {
            // Get Commands project assembly, then get all ICommandValidator implementations from it
            IEnumerable<Type> allCommands = typeFromCommandAssembly.Assembly
                .GetTypes()
                .Where(t => t.IsPublic
                            && t.IsClass
                            && t.GetInterfaces().Contains(typeof(ICommandValidator)));
            foreach (Type assemblyClass in allCommands)
            {
                yield return new object[] { assemblyClass };
            }
        }
    }
}
