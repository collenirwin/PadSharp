using RegularExtensions;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PadSharp.Utils
{
    /// <summary>
    /// Contains extension methods for manipulating string
    /// </summary>
    public static class StringUtils
    {
        /// <summary>
        /// Splits a string into lines, regardless of line ending style
        /// </summary>
        /// <param name="lines">lines to split</param>
        /// <returns>lines separated into an array</returns>
        public static string[] SplitIntoLines(this string lines)
        {
            return lines.RegexSplit(@"\r?\n");
        }

        /// <summary>
        /// Reverses the order of the specified lines
        /// </summary>
        /// <param name="linesToReverse">Lines to reverse</param>
        /// <returns>linesToReverse in reverse order</returns>
        public static string ReverseLines(this string linesToReverse)
        {
            return string.Join("\r\n", linesToReverse.SplitIntoLines().Reverse());
        }

        /// <summary>
        /// Sorts the specified lines alphabetically
        /// </summary>
        /// <param name="linesToSort">Lines to sort</param>
        /// <param name="descending">Sort in descending order?</param>
        /// <returns>Sorted linesToSort</returns>
        public static string SortLines(this string linesToSort, bool descending = false)
        {
            // get a collection of the lines
            var lines = linesToSort.SplitIntoLines();

            // sort 'em (ascending or descending based on passed param)
            var sortedlines = descending ? lines.OrderByDescending(x => x) : lines.OrderBy(x => x);
            return string.Join("\r\n", sortedlines);
        }

        /// <summary>
        /// Converts the specified text to Titlecase
        /// </summary>
        /// <param name="textToConvert">Text you want to convert to Titlecase</param>
        /// <returns>textToConvert in title case</returns>
        public static string ToTitleCase(this string textToConvert)
        {
            string newText = textToConvert;
            var matches = textToConvert.Matches(@"\w+");

            foreach (Match match in matches)
            {
                // we go through the .remove.insert ordeal here in order to preserve the whitespace between the words
                newText = newText
                    .Remove(match.Index, match.Length)
                    .Insert(match.Index, match.Value.Length == 1 // 1-length word:
                        ? match.Value.ToUpper()                  // don't try to do anything with chars after first
                        : match.Value.Substring(0, 1).ToUpper() + match.Value.Substring(1).ToLower());
            }

            return newText;
        }

        /// <summary>
        /// Converts the specified text to tOGGLECASE
        /// </summary>
        /// <param name="textToConvert">Text you feel needs a case toggle</param>
        /// <returns>textToConvert in with each letter's case toggled</returns>
        public static string ToggleCase(this string textToConvert)
        {
            return string.Join("", textToConvert
                .Select(c => char.IsUpper(c) ? char.ToLower(c) : char.ToUpper(c)));
        }

        /// <summary>
        /// Prepends linesToPrepend with textToPrepend
        /// </summary>
        /// <param name="linesToPrepend">Lines to prepend with textToPrepend</param>
        /// <param name="textToPrepend">String to prepend to the passed lines</param>
        /// <returns>linesToPrepend with textToPrepend prepended to each line</returns>
        public static string PrependLines(this string linesToPrepend, string textToPrepend)
        {
            return string.Join("\r\n", linesToPrepend
                .SplitIntoLines()
                .Select(x => textToPrepend + x));
        }

        /// <summary>
        /// Toggles the passed start of the lines.
        /// If the start exists, remove it. If not, add it.
        /// </summary>
        /// <param name="lines">Lines to toggle</param>
        /// <param name="lineStart">Start of the lines to toggle</param>
        /// <returns></returns>
        public static string ToggleLineStart(this string lines, string lineStart)
        {
            return string.Join("\r\n", lines
                .SplitIntoLines()
                .Select(x => x.StartsWith(lineStart) ? x.Substring(lineStart.Length) : lineStart + x));
        }

        /// <summary>
        /// Toggles the passed start and end of the string.
        /// If the start and end exist, remove them. If not, add them.
        /// </summary>
        /// <param name="text">Text to toggle</param>
        /// <param name="lineStart">Start of the string to toggle</param>
        /// <param name="lineEnd">End of the string to toggle</param>
        /// <returns>text with start and end added or removed</returns>
        public static string ToggleStartAndEnd(this string text, string lineStart, string lineEnd)
        {
            bool haveStart = text.StartsWith(lineStart);
            bool haveEnd = text.EndsWith(lineEnd);

            // start and end already there
            if (haveStart && haveEnd)
            {
                // return the string without the start and end
                return text.Substring(lineStart.Length, text.Length - lineEnd.Length - lineStart.Length);
            }
            else if (haveStart)
            {
                return text + lineEnd;
            }
            else if (haveEnd)
            {
                return lineStart + text;
            }

            return lineStart + text + lineEnd;
        }

        /// <summary>
        /// Attempts to count the number of matches of the specified Regular Expression within the string asyncronously.
        /// </summary>
        /// <param name="text">Text to search in</param>
        /// <param name="regex">Regular Expression to apply to text</param>
        /// <param name="matchCase">Use IgnoreCase flag?</param>
        /// <returns>The number of matches, or null if an Exception was thrown</returns>
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
