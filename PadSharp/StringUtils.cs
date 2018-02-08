using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PadSharp
{
    public static class StringUtils
    {
        /// <summary>
        /// Reverses the order of the specified lines
        /// </summary>
        /// <param name="linesToReverse">Lines to reverse</param>
        /// <returns>linesToReverse in reverse order</returns>
        public static string reverseLines(string linesToReverse)
        {
            return string.Join("\r\n", linesToReverse
                .Replace("\r", "")
                .Split('\n')
                .Reverse());
        }

        /// <summary>
        /// Sorts the specified lines provided
        /// </summary>
        /// <param name="linesToSort">Lines to sort</param>
        /// <param name="descending">Sort in descending order?</param>
        /// <returns>Sorted linesToSort</returns>
        public static string sortLines(string linesToSort, bool descending = false)
        {
            // get a collection of the lines
            var lines = linesToSort.Replace("\r", "").Split('\n');

            // sort 'em (ascending or descending based on passed param)
            var sortedlines = descending ? lines.OrderByDescending(x => x) : lines.OrderBy(x => x);

            return string.Join("\r\n", sortedlines);
        }

        /// <summary>
        /// Converts text specified text to Titlecase
        /// </summary>
        /// <param name="textToConvert">Text you want to convert to Titlecase</param>
        /// <returns>textToConvert in title case</returns>
        public static string titleCase(string textToConvert)
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
        public static string toggleCase(string textToConvert)
        {
            string toggledText = "";

            foreach (char c in textToConvert)
            {
                toggledText += char.IsUpper(c) ? c.ToString().ToLower() : c.ToString().ToUpper();
            }

            return toggledText;
        }

        /// <summary>
        /// Prepends linesToPrepend with textToPrepend
        /// </summary>
        /// <param name="linesToPrepend">Lines to prepend with textToPrepend</param>
        /// <param name="textToPrepend">String to prepend to the passed lines</param>
        /// <returns>linesToPrepend with textToPrepend prepended to each line</returns>
        public static string prependLines(string linesToPrepend, string textToPrepend)
        {
            return string.Join("\r\n", linesToPrepend
                .Replace("\r", "")
                .Split('\n')
                .Select(x => textToPrepend + x));
        }
    }
}
