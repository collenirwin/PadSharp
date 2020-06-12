using RegularExtensions;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PadSharp.Utils
{
    /// <summary>
    /// Contains extension methods for manipulating string.
    /// </summary>
    public static class StringUtils
    {
        /// <summary>
        /// Splits a string into lines, regardless of line ending style.
        /// </summary>
        /// <param name="lines">Lines to split.</param>
        /// <returns>Lines separated into an array.</returns>
        public static string[] SplitIntoLines(this string lines) => lines.RegexSplit(@"\r?\n");

        /// <summary>
        /// Reverses the order of the lines in the given string.
        /// </summary>
        /// <param name="linesToReverse">String containing lines to reverse.</param>
        /// <returns>The given string with its lines reversed.</returns>
        public static string ReverseLines(this string linesToReverse) =>
            string.Join("\r\n", linesToReverse.SplitIntoLines().Reverse());

        /// <summary>
        /// Sorts the order of the lines in the given string.
        /// </summary>
        /// <param name="linesToSort">String containing lines to sort.</param>
        /// <param name="descending">Sort in descending order?</param>
        /// <returns>The given string with its lines sorted.</returns>
        public static string SortLines(this string linesToSort, bool descending = false)
        {
            var lines = linesToSort.SplitIntoLines();
            var sortedlines = descending ? lines.OrderByDescending(x => x) : lines.OrderBy(x => x);
            return string.Join("\r\n", sortedlines);
        }

        /// <summary>
        /// Converts the specified text to Titlecase.
        /// </summary>
        /// <param name="textToConvert">Text to convert to Titlecase.</param>
        /// <returns>The given text, converted to Titlecase.</returns>
        public static string ToTitleCase(this string textToConvert)
        {
            string newText = textToConvert;
            var matches = textToConvert.Matches(@"\w+");

            foreach (Match match in matches)
            {
                // we go through the .Remove.Insert ordeal here in order to preserve the whitespace between the words
                newText = newText
                    .Remove(match.Index, match.Length)
                    .Insert(match.Index, match.Value.Length == 1 // 1-length word:
                        ? match.Value.ToUpper()                  // don't try to do anything with chars after first
                        : match.Value.Substring(0, 1).ToUpper() + match.Value.Substring(1).ToLower());
            }

            return newText;
        }

        /// <summary>
        /// Converts the specified text to tOGGLECASE.
        /// </summary>
        /// <param name="textToConvert">Text to convert to tOGGLECASE.</param>
        /// <returns>The given text, converted to tOGGLECASE.</returns>
        public static string ToggleCase(this string textToConvert) =>
            string.Join("", textToConvert
                .Select(c => char.IsUpper(c) ? char.ToLower(c) : char.ToUpper(c)));

        /// <summary>
        /// Prepends each line in linesToPrepend with textToPrepend.
        /// </summary>
        /// <param name="linesToPrepend">Lines to prepend with textToPrepend.</param>
        /// <param name="textToPrepend">String to prepend to the passed lines.</param>
        /// <returns>linesToPrepend with textToPrepend prepended to each line.</returns>
        public static string PrependLines(this string linesToPrepend, string textToPrepend) =>
            string.Join("\r\n", linesToPrepend
                .SplitIntoLines()
                .Select(line => textToPrepend + line));

        /// <summary>
        /// Toggles the passed start of the lines.
        /// If the start exists, remove it. If not, add it.
        /// </summary>
        /// <param name="lines">Lines to toggle the start of.</param>
        /// <param name="lineStart">Start of the lines to toggle.</param>
        /// <returns>lines with lineStart toggled on each line.</returns>
        public static string ToggleLineStart(this string lines, string lineStart) =>
            string.Join("\r\n", lines
                .SplitIntoLines()
                .Select(line => line.StartsWith(lineStart) ? line.Substring(lineStart.Length) : lineStart + line));

        /// <summary>
        /// Toggles the passed start and end of the string.
        /// If the start and end exist, remove them. If not, add them.
        /// </summary>
        /// <param name="text">Text to toggle the start and end of.</param>
        /// <param name="start">Start of the string to toggle.</param>
        /// <param name="end">End of the string to toggle.</param>
        /// <returns>text with start and end added or removed.</returns>
        public static string ToggleStartAndEnd(this string text, string start, string end)
        {
            bool haveStart = text.StartsWith(start);
            bool haveEnd = text.EndsWith(end);

            // start and end already there
            if (haveStart && haveEnd)
            {
                // return the string without the start and end
                return text.Substring(start.Length, text.Length - end.Length - start.Length);
            }
            else if (haveStart)
            {
                return text + end;
            }
            else if (haveEnd)
            {
                return start + text;
            }

            return start + text + end;
        }

        /// <summary>
        /// Attempts to count the number of matches of the specified
        /// Regular Expression within the string asyncronously.
        /// </summary>
        /// <param name="text">Text to search within.</param>
        /// <param name="regex">Regular Expression to apply to text.</param>
        /// <param name="matchCase">Use IgnoreCase flag?</param>
        /// <returns>The number of matches, or null if an Exception was thrown.</returns>
        public static async Task<int?> TryCountMatchesAsync(this string text, string regex, bool matchCase)
        {
            try
            {
                var options = RegexOptions.Multiline;

                if (!matchCase)
                {
                    options |= RegexOptions.IgnoreCase;
                }

                return await Task.Run(() => text.Matches(regex, options).Count);
            }
            catch
            {
                return null;
            }
        }
    }
}
