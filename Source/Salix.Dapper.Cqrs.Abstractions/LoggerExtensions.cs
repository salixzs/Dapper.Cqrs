using System;

namespace Salix.Dapper.Cqrs.Abstractions
{
    public static class LoggerExtensions
    {
        /// <summary>
        /// Returns TimeSpan string representation in its shortest possible form.
        /// Not recommended to use for times more than few minutes!
        /// Examples: 304ms, 2.34s, 1 min 28.4323 sec.
        /// </summary>
        /// <param name="elapsedTime">The TimeSpan, usually elapsed time from Stopwatch.Elapsed.</param>
        public static string ToHumanReadableString(this TimeSpan elapsedTime)
        {
            if (elapsedTime.Ticks == 0)
            {
                return "0";
            }

            if (elapsedTime.TotalMilliseconds < 10)
            {
                return $"{elapsedTime.TotalMilliseconds:0.###} ms";
            }

            if (elapsedTime.TotalMilliseconds < 1000)
            {
                return $"{elapsedTime.TotalMilliseconds:####} ms";
            }

            if (elapsedTime.Minutes > 0)
            {
                return $"{elapsedTime.Minutes:D} min {elapsedTime.Seconds:D} sec";
            }

            if (elapsedTime.Seconds < 10)
            {
                return $"{elapsedTime.Seconds:D} sec {elapsedTime.Milliseconds:0} ms";
            }

            return $"{elapsedTime.Seconds:D} sec";
        }

        /// <summary>
        /// ONLY FOR SQL QUERY STRINGS!
        /// This shortens SQL Query string for outputting in logging. Does not work on any string (only SQL).
        /// Returned string is not suitable for executing against database, its just "smartly" shortened for viewing purposes.
        /// </summary>
        /// <param name="query">The SQL query string.</param>
        public static string ToShortSql(this string query)
        {
            if (string.IsNullOrEmpty(query))
            {
                return "---";
            }

            string trimmed = query
                .Replace("\n", string.Empty)
                .Replace("\r", string.Empty)
                .Replace("[", string.Empty)
                .Replace("]", string.Empty);
            trimmed = string.Join(" ", trimmed.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));

            const int maxTotalLength = 75;
            const int maxSelectLength = (int)(maxTotalLength * 0.3);
            const int maxFromLength = (int)(maxTotalLength * 0.4);

            var selectPosition = trimmed.IndexOf("SELECT", 0, StringComparison.InvariantCultureIgnoreCase);
            var fromPosition = trimmed.IndexOf("FROM", 0, StringComparison.InvariantCultureIgnoreCase);
            if (selectPosition >= 0)
            {
                // SUBSELECTS in SELECT clause
                int nextSelectPosition = selectPosition;
                do
                {
                    nextSelectPosition = trimmed.IndexOf("SELECT", nextSelectPosition + 1, StringComparison.InvariantCultureIgnoreCase);
                    if (nextSelectPosition > 0 && nextSelectPosition < fromPosition)
                    {
                        fromPosition = trimmed.IndexOf("FROM", fromPosition + 1, StringComparison.InvariantCultureIgnoreCase);
                    }
                }
                while (nextSelectPosition > 0);
            }

            if (selectPosition == -1 || selectPosition > 5)
            {
                return trimmed.Length > maxTotalLength ? trimmed.Substring(0, maxTotalLength - 1) + "\u2026" : trimmed;
            }

            string selectPart = trimmed.Substring(selectPosition, fromPosition);
            int actualSelectPartLength = selectPart.Length;
            if (selectPart.Length > maxSelectLength)
            {
                selectPart = selectPart.Substring(0, maxSelectLength - 2) + "\u2026 ";
                actualSelectPartLength = maxSelectLength;
            }

            var wherePosition = trimmed.IndexOf("WHERE", fromPosition == -1 ? 0 : fromPosition, StringComparison.InvariantCultureIgnoreCase);

            string fromPart = trimmed.Substring(fromPosition, wherePosition == -1 ? trimmed.Length - fromPosition : trimmed.Length - fromPosition - (trimmed.Length - wherePosition));
            int actualFromPartLength = fromPart.Length;
            if (fromPart.Length > maxFromLength)
            {
                fromPart = fromPart.Substring(0, maxFromLength - 2) + "\u2026 ";
                actualFromPartLength = maxFromLength;
            }

            string wherePart = wherePosition == -1 ? string.Empty : trimmed.Substring(wherePosition, trimmed.Length - wherePosition);
            int maxWhereLength = maxTotalLength - actualSelectPartLength - actualFromPartLength;
            if (wherePart.Length > maxWhereLength)
            {
                wherePart = wherePart.Substring(0, maxWhereLength - 1) + "\u2026";
            }

            return string.Concat(selectPart, fromPart, wherePart);
        }
    }
}
