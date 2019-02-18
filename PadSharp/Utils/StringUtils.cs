using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PadSharp
{
    /// <summary>
    /// Contains extension methods for manipulating string
    /// </summary>
    public static class StringUtils
    {
        /// <summary>
        /// Removes \r before splitting at \n
        /// </summary>
        /// <param name="lines">lines to split</param>
        /// <returns>lines separated into an array</returns>
        public static string[] SplitLines(this string lines)
        {
            return lines.Replace("\r", "").Split('\n');
        }

        /// <summary>
        /// Reverses the order of the specified lines
        /// </summary>
        /// <param name="linesToReverse">Lines to reverse</param>
        /// <returns>linesToReverse in reverse order</returns>
        public static string ReverseLines(this string linesToReverse)
        {
            return string.Join("\r\n", linesToReverse.SplitLines().Reverse());
        }

        /// <summary>
        /// Sorts the specified lines provided
        /// </summary>
        /// <param name="linesToSort">Lines to sort</param>
        /// <param name="descending">Sort in descending order?</param>
        /// <returns>Sorted linesToSort</returns>
        public static string SortLines(this string linesToSort, bool descending = false)
        {
            // get a collection of the lines
            var lines = linesToSort.SplitLines();

            // sort 'em (ascending or descending based on passed param)
            var sortedlines = descending ? lines.OrderByDescending(x => x) : lines.OrderBy(x => x);

            return string.Join("\r\n", sortedlines);
        }

        /// <summary>
        /// Converts text specified text to Titlecase
        /// </summary>
        /// <param name="textToConvert">Text you want to convert to Titlecase</param>
        /// <returns>textToConvert in title case</returns>
        public static string ToTitleCase(this string textToConvert)
        {
            string newText = textToConvert;
            var matches = Regex.Matches(textToConvert, @"\w+");

            foreach (Match match in matches)
            {
                // we go through the .remove.insert ordeal here in order to preserve the whitespace between the words
                newText = newText
                    .Remove(match.Index, match.Length)
                    .Insert(match.Index, match.Value.Length == 1 // 1-length word: don't try to do anything with chars after first
                        ? match.Value.ToUpper()
                        : match.Value.Substring(0, 1).ToUpper() + match.Value.Substring(1).ToLower());
            }

            return newText;
        }

        /// <summary>
        /// Converts text specified text to tOGGLECASE
        /// </summary>
        /// <param name="textToConvert">Text you feel needs a case toggle</param>
        /// <returns>textToConvert in with each letter's case toggled</returns>
        public static string ToggleCase(this string textToConvert)
        {
            string toggledText = "";

            foreach (char c in textToConvert)
            {
                toggledText += char.IsUpper(c) ? char.ToLower(c) : char.ToUpper(c);
            }

            return toggledText;
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
                .SplitLines()
                .Select(x => textToPrepend + x));
        }

        /// <summary>
        /// Toggles the passed start of the lines.
        /// If the start exists, remove it. If not, add it.
        /// </summary>
        /// <param name="lines">Lines to toggle</param>
        /// <param name="start">Start of the lines to toggle</param>
        /// <returns></returns>
        public static string ToggleLineStart(this string lines, string start)
        {
            return string.Join("\r\n", lines
                .SplitLines()
                .Select(x => x.StartsWith(start) ? x.Substring(start.Length) : start + x));
        }

        /// <summary>
        /// Toggles the passed start and end of the string.
        /// If the start and end exist, remove them. If not, add them.
        /// </summary>
        /// <param name="text">Text to toggle</param>
        /// <param name="start">Start of the string to toggle</param>
        /// <param name="end">End of the string to toggle</param>
        /// <returns>text with start and end added or removed</returns>
        public static string ToggleStartAndEnd(this string text, string start, string end)
        {
            // start and end aalready there
            if (text.StartsWith(start) && text.EndsWith(end))
            {
                // return the string without the start and end
                return text.Substring(start.Length, text.Length - end.Length - start.Length);
            }

            return start + text + end;
        }

        /// <summary>
        /// Attempts to count the number of matches of the specified Regular Expression within the string asyncronously.
        /// </summary>
        /// <param name="text">Text to search in</param>
        /// <param name="regex">Regular Expression to apply to text</param>
        /// <param name="matchCase">Use IgnoreCase flag?</param>
        /// <param name="action">Action to call when count is complete (takes count as only parameter)</param>
        /// <param name="error">Action to call when an Exception is thrown</param>
        public static async void CountMatchesAsync(this string text, string regex, bool matchCase,
            Action<int> action, Action<Exception> error = null)
        {
            try
            {
                int result = await Task<int>.Run(() =>
                    Regex.Matches(text, regex, RegexOptions.Multiline | (matchCase ? RegexOptions.IgnoreCase : 0)).Count);

                if (action != null)
                {
                    action(result);
                }
            }
            catch (Exception ex)
            {
                if (error != null)
                {
                    error(ex);
                }
            }
        }
    }
}
