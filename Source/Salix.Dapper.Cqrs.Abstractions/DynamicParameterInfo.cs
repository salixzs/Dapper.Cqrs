using System.Data;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Salix.Dapper.Cqrs.Abstractions
{
    /// <summary>
    /// Class to abstract parameter parameters for adding to SQL Command.
    /// </summary>
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class DynamicParameterInfo
    {
        /// <summary>
        /// Name of the parameter.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Value for parameter.
        /// </summary>
        public object Value { get; set; }

        /// <summary>
        /// Direction - IN[put] or OUT[put] parameter.
        /// </summary>
        public ParameterDirection ParameterDirection { get; set; }

        /// <summary>
        /// MS SQL Database Type for parameter.
        /// </summary>
        public SqlDbType? SqlDbType { get; set; }

        /// <summary>
        /// Size of parameter (string length, number size).
        /// </summary>
        public int? Size { get; set; }

        /// <summary>
        /// Fully setup SqlParameter from given values.
        /// </summary>
        public IDbDataParameter AttachedParam { get; set; }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        [ExcludeFromCodeCoverage]
        private string DebuggerDisplay
        {
            get
            {
                var dbg = new StringBuilder();
                switch (this.ParameterDirection)
                {
                    case ParameterDirection.Input:
                        dbg.Append("IN ");
                        break;
                    case ParameterDirection.InputOutput:
                        dbg.Append("INOUT ");
                        break;
                    case ParameterDirection.Output:
                        dbg.Append("OUT ");
                        break;
                    case ParameterDirection.ReturnValue:
                        dbg.Append("RETURN ");
                        break;
                    default:
                        break;
                }

                dbg.Append($"{this.Name}={this.Value}");
                return dbg.ToString();
            }
        }
    }
}
