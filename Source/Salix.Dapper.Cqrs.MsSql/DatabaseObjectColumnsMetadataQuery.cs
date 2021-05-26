using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Salix.Dapper.Cqrs.Abstractions;

namespace Salix.Dapper.Cqrs.MsSql
{
    /// <summary>
    /// Retrieves a list of columns and its properties in specified database object (table, view).
    /// </summary>
    [ExcludeFromCodeCoverage]

    public sealed class DatabaseObjectColumnsMetadataQuery : MsSqlQueryMultipleBase<DatabaseObjectColumnMetadata>
    {
        private readonly string _objectName;

        /// <summary>
        /// Retrieves a list of columns and its properties in specified database object (table, view).
        /// </summary>
        /// <param name="objectName">The database object (Table, View) name.</param>
        public DatabaseObjectColumnsMetadataQuery(string objectName) => _objectName = objectName;

        /// <summary>
        /// Actual SQL Statement to execute against MS SQL database.
        /// </summary>
        public override string SqlStatement => @"
SELECT COLUMN_NAME as ColumnName,
       UPPER(DATA_TYPE) as DataType,
       CONVERT(bit, (CASE IS_NULLABLE WHEN 'YES' THEN 1 ELSE 0 END)) as IsNullable,
       CHARACTER_MAXIMUM_LENGTH AS CharLength,
       NUMERIC_PRECISION AS NumericPrecision,
       NUMERIC_SCALE AS NumericScale,
       DATETIME_PRECISION AS DateTimePrecision,
       CONVERT(bit, CASE WHEN COLUMN_DEFAULT IS NULL THEN 0 ELSE 1 END) as HasDefaultValue
FROM   INFORMATION_SCHEMA.COLUMNS
WHERE  TABLE_NAME = @ObjectName
";

        /// <summary>
        /// Anonymous object of Table Name.
        /// </summary>
        public override object Parameters => new { ObjectName = _objectName };
    }

    /// <summary>
    /// Data object holding database object column metadata.
    /// </summary>
    [ExcludeFromCodeCoverage]
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class DatabaseObjectColumnMetadata
    {
        /// <summary>
        /// The name of the column.
        /// </summary>
        public string ColumnName { get; set; }

        /// <summary>
        /// The type of the data in column (SQL types, like nvarchar).
        /// </summary>
        public string DataType { get; set; }

        /// <summary>
        /// A value indicating whether this column is nullable.
        /// </summary>
        public bool IsNullable { get; set; }

        /// <summary>
        /// The length of the string in character-type columns (char, varchar, nchar, nvarchar)
        /// </summary>
        public int CharLength { get; set; }

        /// <summary>
        /// The numeric precision for number-type columns (int, bit, float...).
        /// </summary>
        public int NumericPrecision { get; set; }

        /// <summary>
        /// The numeric scale (digits after dot/comma) for floating point columns (for whole numbers (int) it contains 0).
        /// </summary>
        public int NumericScale { get; set; }

        /// <summary>
        /// The date-time column precision (datetime = 3).
        /// </summary>
        public int DateTimePrecision { get; set; }

        /// <summary>
        /// True, when column has constraint for default value when nothing is passed.
        /// </summary>
        public bool HasDefaultValue { get; set; }

        /// <summary>
        /// Displays DB object column definition in Debug screen. (Only for development purposes).
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string DebuggerDisplay
        {
            get
            {
                var dbgView = new StringBuilder($"{this.ColumnName} {this.DataType}");
                if (this.DataType.Contains("CHAR"))
                {
                    dbgView.Append(this.CharLength == -1 ? "(MAX)" : $"({this.CharLength:D})");
                }

                if (this.DataType.Contains("BINARY"))
                {
                    dbgView.Append(this.CharLength == -1 ? "(MAX)" : $"({this.CharLength:D})");
                }

                if (this.DataType.Contains("DECIMAL"))
                {
                    dbgView.Append($"({this.NumericPrecision:D}, {this.NumericScale:D})");
                }

                if (this.DataType.Contains("NUMERIC"))
                {
                    dbgView.Append($"({this.NumericPrecision:D}, {this.NumericScale:D})");
                }

                if (this.DataType.Contains("FLOAT") || this.DataType.Contains("REAL"))
                {
                    dbgView.Append($"({this.NumericPrecision:D}, {this.NumericScale:D})");
                }

                if (this.DataType.Contains("DATETIME2"))
                {
                    dbgView.Append($"({this.DateTimePrecision:D})");
                }

                dbgView.Append(this.IsNullable ? "  NULL" : "  NOT NULL");
                dbgView.Append(this.HasDefaultValue ? " (+DEFAULT)" : string.Empty);
                return dbgView.ToString();
            }
        }
    }
}
