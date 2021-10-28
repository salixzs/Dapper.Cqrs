using System.Data;

namespace Salix.Dapper.Cqrs.Abstractions
{
    /// <summary>
    /// Allows to assign SQL command parameters dynamically.
    /// </summary>
    public interface IDynamicParameters
    {
        /// <summary>
        /// Add a parameter to dynamic parameter list for SQL database functionality.
        /// </summary>
        /// <param name="name">Name of parameter.</param>
        /// <param name="fieldType">Database type of parameter.</param>
        /// <param name="value">Value for the parameter.</param>
        /// <param name="direction">In or Out parameter.</param>
        /// <param name="size">Database field size.</param>
        void Add(string name, SqlDbType? fieldType = null, object value = null, ParameterDirection direction = ParameterDirection.Input, int? size = null);
    }
}
