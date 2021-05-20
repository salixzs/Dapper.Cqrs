using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Salix.Dapper.Cqrs.MsSql.Testing.XUnit
{
    /// <summary>
    /// Test helpers to work with Database related tests.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public static class HelperDatabaseTests
    {
        /// <summary>
        /// Retrieves all Public properties names from database data object (DB DTO).
        /// Pass in DB POCO class to get its property descriptions via reflection.
        /// </summary>
        /// <typeparam name="T">DB POCO class.</typeparam>
        public static List<PocoPropertyMetadata> GetPocoObjectProperties<T>()
        {
            var propertyNames = new List<PocoPropertyMetadata>();
            foreach (PropertyInfo props in typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                Type nullableType = Nullable.GetUnderlyingType(props.PropertyType);
                propertyNames.Add(new PocoPropertyMetadata
                {
                    Name = props.Name,
                    TypeName = nullableType == null ? props.PropertyType.Name : nullableType.Name,
                    IsNullable = nullableType != null
                });
            }

            return propertyNames;
        }

        /// <summary>
        /// Compares the database column descriptions with data contract properties
        /// and reports any misalignment (missing columns, missing properties, incompatible types used, null-ability for both).
        /// </summary>
        /// <typeparam name="T">Type of data contract (DB POCO class).</typeparam>
        /// <param name="fixture">The database tests fixture for getting database object columns metadata.</param>
        /// <param name="tableName">Name of the table. Default (if not used) = data contract name.</param>
        /// <param name="exceptPocoProperties">The exceptions to be used for data contract properties (extra properties, which should not be considered). When not specified - uses all found properties.</param>
        /// <param name="exceptDatabaseFields">The exceptions to database column names (extra fields not used to map data to data contract). When not specified - uses all found columns in database object.</param>
        public static async Task<List<string>> CompareDatabaseWithContract<T>(
            SqlDatabaseFixture fixture,
            string tableName = null,
            HashSet<string> exceptPocoProperties = null,
            HashSet<string> exceptDatabaseFields = null)
        {
            tableName ??= typeof(T).Name;
            exceptPocoProperties ??= new HashSet<string>();
            exceptDatabaseFields ??= new HashSet<string>();
            var validationResults = new List<string>();

            List<PocoPropertyMetadata> pocoProperties = GetPocoObjectProperties<T>();
            var databaseFields = (await fixture.Db.QueryAsync(new DatabaseObjectColumnsMetadataQuery(tableName))).ToList();

            // Validating that all DTO POCO properties has counterpart in database object fields
            foreach (PocoPropertyMetadata pocoProperty in pocoProperties)
            {
                if (exceptPocoProperties.Contains(pocoProperty.Name))
                {
                    continue;
                }

                DatabaseObjectColumnMetadata? dbField = databaseFields.FirstOrDefault(dbf =>
                    dbf.ColumnName.Equals(pocoProperty.Name, StringComparison.OrdinalIgnoreCase));
                if (dbField == null)
                {
                    validationResults.Add($"DB column with name \"{pocoProperty.Name}\" was not found (but property exists). If property is extra (non-database loadable), then use \"exceptPocoProperties\" parameter to exclude it from test.");
                    continue;
                }

                if (pocoProperty.TypeName.Equals("string", StringComparison.OrdinalIgnoreCase) && !(dbField.DataType.Contains("CHAR") || dbField.DataType.Contains("XML")))
                {
                    validationResults.Add($"Data contract property \"{pocoProperty.Name}\" (STRING) has non-matching type \"{dbField.DataType}\" in database column.");
                    continue;
                }

                if (!new HashSet<string> { "BYTE[]", "STRING" }.Contains(pocoProperty.TypeName.ToUpper(CultureInfo.InvariantCulture)) && pocoProperty.IsNullable != dbField.IsNullable && pocoProperty.IsNullable && !dbField.HasDefaultValue)
                {
                    validationResults.Add($"Data contract property \"{pocoProperty.Name}\" ({pocoProperty.TypeName}{(pocoProperty.IsNullable ? "?" : string.Empty)}) has non-matching NULL-ability in database field \"{dbField.DataType} {(dbField.IsNullable ? "NULL" : "NOT NULL")}\".");
                }
            }

            // Validating that all Database Fields has counterpart in POCO properties
            foreach (DatabaseObjectColumnMetadata dbField in databaseFields)
            {
                if (exceptDatabaseFields.Contains(dbField.ColumnName))
                {
                    continue;
                }

                PocoPropertyMetadata? pocoProperty = pocoProperties.FirstOrDefault(pp =>
                    pp.Name.Equals(dbField.ColumnName, StringComparison.OrdinalIgnoreCase));
                if (pocoProperty == null)
                {
                    validationResults.Add($"Data contract property with name \"{dbField.ColumnName}\" was not found (but DB column exists). If DB field is not loadable, then use \"exceptDatabaseFields\" parameter to exclude it from test.");
                    continue;
                }

                if (dbField.DataType.Contains("CHAR") && pocoProperty.TypeName.ToUpper(CultureInfo.InvariantCulture) != "STRING")
                {
                    validationResults.Add($"Database column \"{dbField.ColumnName}\" ({dbField.DataType}) has non-matching type \"{pocoProperty.TypeName}\" in data contract property.");
                    continue;
                }

                if (!new HashSet<string> { "BYTE[]", "STRING" }.Contains(pocoProperty.TypeName.ToUpper(CultureInfo.InvariantCulture)) && !dbField.DataType.ToUpper(CultureInfo.InvariantCulture).Contains("XML") && pocoProperty.IsNullable != dbField.IsNullable && pocoProperty.IsNullable && !dbField.HasDefaultValue)
                {
                    validationResults.Add($"Database column \"{dbField.ColumnName}\" ({dbField.DataType} {(dbField.IsNullable ? "NULL" : "NOT NULL")}) has non-matching NULL-ability in data contract property ({pocoProperty.TypeName}{(pocoProperty.IsNullable ? "?" : string.Empty)}).");
                }
            }

            return validationResults;
        }
    }

    /// <summary>
    /// Metadata about POCO class property.
    /// </summary>
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class PocoPropertyMetadata
    {
        /// <summary>
        /// The name of the property.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The type name of the property.
        /// For nullable types stores underlying (actual) type.
        /// </summary>
        public string TypeName { get; set; }

        /// <summary>
        /// When true - property is nullable.
        /// </summary>
        public bool IsNullable { get; set; }

        /// <summary>
        /// Displays DB object main properties in Debug screen. (Only for development purposes).
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string DebuggerDisplay
        {
            get
            {
                var dbgView = new StringBuilder($"{this.Name} ({this.TypeName}");
                dbgView.Append(this.IsNullable ? "?)" : ")");
                return dbgView.ToString();
            }
        }
    }
}
