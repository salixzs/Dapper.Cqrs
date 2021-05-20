using System;
using System.Diagnostics;

namespace $rootnamespace$
{
    /// <summary>
    /// Holds data from SQL database table record.
    /// </summary>
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public sealed class $safeitemname$
    {
        /// <summary>
        /// Unique identifier of a record in database.
        /// Type: INT NOT NULL, AUTOINCREMENT.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Name for the object.
        /// Type: NVARCHAR(50) NULL.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Displays DB object main properties in Debug screen. (Only for development purposes).
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string DebuggerDisplay => $"{this.Name} (ID:{this.Id})";

    }
}
