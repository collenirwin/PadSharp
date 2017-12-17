using ICSharpCode.AvalonEdit;
using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace PadSharp
{
    /// <summary>
    /// Contains various utility functions for working with text within an AvalonEdit TextEditor
    /// </summary>
    public static class TextEditorUtils
    {
        #region Find + Replace

        /// <summary>
        /// Attempts to find the next match of the specified Regular Expression
        /// within textbox.Text and highlight it.
        /// </summary>
        /// <param name="textbox">TextEditor to search in</param>
        /// <param name="regex">Regular Expression to apply to textbox.Text</param>
        /// <param name="start">Starting point in textbox.Text</param>
        /// <param name="matchCase">use IgnoreCase flag?</param>
        /// <param name="lookback">search back from this point?</param>
        /// <returns>true if found, false if regex is incorrect or if not found</returns>
        public static bool findNext(TextEditor textbox, string regex, 
            int start, bool matchCase, bool lookback = false)
        {
            try
            {
                var options = matchCase
                    ? RegexOptions.None
                    : RegexOptions.IgnoreCase;

                string allText = textbox.Text;

                if (lookback)
                {
                    options |= RegexOptions.RightToLeft;
                }

                var _regex = new Regex(regex, options);
                var match = _regex.Match(allText, start);

                if (!match.Success)
                {
                    // loop around and start from the opposite end
                    match = _regex.Match(textbox.Text, lookback ? textbox.Text.Length : 0);
                }

                if (match.Success)
                {
                    // select the matched text
                    textbox.Select(match.Index, match.Length);

                    // scroll to the matched text
                    var location = textbox.Document.GetLocation(match.Index);
                    textbox.ScrollTo(location.Line, location.Column);
                }

                return match.Success;
            }
            catch
            {
                // invalid regular expression
                return false;
            }
        }

        /// <summary>
        /// Attempts to find the next match of the specified Regular Expression
        /// within textbox.Text and replace it with the replacement.
        /// </summary>
        /// <param name="textbox">TextEditor to search in</param>
        /// <param name="regex">Regular Expression to apply to textbox.Text</param>
        /// <param name="start">Starting point in textbox.Text</param>
        /// <param name="matchCase">use IgnoreCase flag?</param>
        /// <param name="lookback">search back from this point?</param>
        /// <returns>true if found, false if regex is incorrect or if not found</returns>
        public static bool replaceNext(TextEditor textbox, string regex, string replacement,
            int start, bool matchCase, bool lookback = false)
        {
            if (regex == "")
            {
                return false;
            }

            if (findNext(textbox, regex, start, matchCase, lookback))
            {
                int oldStart = textbox.SelectionStart;

                // replace the selection
                textbox.Text = textbox.Text
                    .Remove(oldStart, textbox.SelectionLength)
                    .Insert(oldStart, replacement);

                // place our caret after the inserted replacement
                textbox.CaretOffset = oldStart + replacement.Length;
                return true;
            }

            // nothing to replace
            return false;
        }

        /// <summary>
        /// Attempts to find the matches of the specified Regular Expression
        /// within textbox.Text and replace them with the replacement.
        /// </summary>
        /// <param name="textbox">TextEditor to search in</param>
        /// <param name="regex">Regular Expression to apply to textbox.Text</param>
        /// <param name="start">Starting point in textbox.Text</param>
        /// <param name="matchCase">use IgnoreCase flag?</param>
        /// <param name="predicate">
        /// Number of replacements will be passed to this.
        /// It will decide if the replacement will run.
        /// </param>
        /// <returns>
        /// true if found, false if regex is incorrect or if not found or if predicate returns false
        /// </returns>
        public static bool replaceAll(TextEditor textbox, string regex, string replacement,
            bool matchCase, Predicate<int> predicate = null)
        {
            if (regex == "")
            {
                return false;
            }

            var options = matchCase
                ? RegexOptions.None
                : RegexOptions.IgnoreCase;

            options |= RegexOptions.Multiline;

            var _regex = new Regex(regex, options);
            var matches = _regex.Matches(textbox.Text);

            if (predicate == null || predicate(matches.Count))
            {
                textbox.Text = _regex.Replace(textbox.Text, replacement);
                return true;
            }

            return false;
        }

        #endregion

        #region Specific Text Manipulation

        /// <summary>
        /// Replaces originalText with newText if it exists within the <see cref="TextEditor"/> provided.
        /// Does nothing otherwise.
        /// </summary>
        /// <param name="textbox"><see cref="TextEditor"/> to replace text in</param>
        /// <param name="originalText">text to replace</param>
        /// <param name="newText">text to replace originalText with</param>
        static void replaceIfExists(TextEditor textbox, string originalText, string newText)
        {
            int index = textbox.Text.IndexOf(originalText);

            if (index != -1)
            {
                // use textbox.Document.Text so that the undo stack is updated
                textbox.Document.Text = textbox.Document.Text
                    .Remove(index, originalText.Length)
                    .Insert(index, newText);
            }
        }
            

        /// <summary>
        /// Reverses the order of the specified lines in the <see cref="TextEditor"/> provided.
        /// Does nothing if textbox fors not contain linesToReverse.
        /// </summary>
        /// <param name="textbox"><see cref="TextEditor"/> to reverse lines in</param>
        /// <param name="linesToReverse">Lines to reverse</param>
        public static void reverseLines(TextEditor textbox, string linesToReverse)
        {
            replaceIfExists(textbox, linesToReverse,
                string.Join("\r\n", linesToReverse
                    .Replace("\r", "")
                    .Split('\n')
                    .Reverse()));
        }

        /// <summary>
        /// Sorts the specified lines in the <see cref="TextEditor"/> provided.
        /// Does nothing if textbox does not conain linesToSort.
        /// </summary>
        /// <param name="textbox"><see cref="TextEditor"/> to sort lines in</param>
        /// <param name="linesToSort">Lines to sort</param>
        /// <param name="descending">Sort in descending order?</param>
        public static void sortLines(TextEditor textbox, string linesToSort, bool descending = false)
        {
            // get a collection of the lines
            var lines = linesToSort.Replace("\r", "").Split('\n');

            // sort 'em (ascending or descending based on passed param)
            var sortedlines = descending ? lines.OrderByDescending(x => x) : lines.OrderBy(x => x);

            replaceIfExists(textbox, linesToSort, string.Join("\r\n", sortedlines));
        }

        /// <summary>
        /// Converts text specified text to lowercase if it's contained in the <see cref="TextEditor"/> provided
        /// </summary>
        /// <param name="textbox"><see cref="TextEditor"/> to convert text in</param>
        /// <param name="textToConvert">Text you want to convert to lowercase</param>
        public static void lowerCase(TextEditor textbox, string textToConvert)
        {
            replaceIfExists(textbox, textToConvert, textToConvert.ToLower());
        }

        /// <summary>
        /// Converts text specified text to UPPERCASE if it's contained in the <see cref="TextEditor"/> provided
        /// </summary>
        /// <param name="textbox"><see cref="TextEditor"/> to convert text in</param>
        /// <param name="textToConvert">Text you want to convert to UPPERCASE</param>
        public static void upperCase(TextEditor textbox, string textToConvert)
        {
            replaceIfExists(textbox, textToConvert, textToConvert.ToUpper());
        }

        /// <summary>
        /// Converts text specified text to Titlecase if it's contained in the <see cref="TextEditor"/> provided
        /// </summary>
        /// <param name="textbox"><see cref="TextEditor"/> to convert text in</param>
        /// <param name="textToConvert">Text you want to convert to Titlecase</param>
        public static void titleCase(TextEditor textbox, string textToConvert)
        {
            string newText = textToConvert;
            var matches = Regex.Matches(textToConvert, @"\w+");

            foreach (Match match in matches)
            {
                newText = newText
                    .Remove(match.Index, match.Length)
                    .Insert(match.Index, match.Value.Length == 1 // 1-length word: don't try to do anything with chars after first
                        ? match.Value.ToUpper()
                        : match.Value.Substring(0, 1).ToUpper() + match.Value.Substring(1).ToLower());
            }

            replaceIfExists(textbox, textToConvert, newText);
        }

        /// <summary>
        /// Converts text specified text to tOGGLECASE if it's contained in the <see cref="TextEditor"/> provided
        /// </summary>
        /// <param name="textbox"><see cref="TextEditor"/> to convert text in</param>
        /// <param name="textToConvert">Text you feel needs a case toggle</param>
        public static void toggleCase(TextEditor textbox, string textToConvert)
        {
            string toggledText = "";

            foreach (char c in textToConvert)
            {
                toggledText += char.IsUpper(c) ? c.ToString().ToLower() : c.ToString().ToUpper();
            }

            replaceIfExists(textbox, textToConvert, toggledText);
        }

        #endregion

        /// <summary>
        /// Set all line endings to \r\n (windows = true) or \n (windows = false)
        /// </summary>
        /// <param name="textbox"><see cref="TextEditor"/> we're working with</param>
        /// <param name="windows">Use Windows line endings?</param>
        public static void normalizeLineEndings(TextEditor textbox, bool windows)
        {
            textbox.Text = textbox.Text.Replace("\r", "");

            if (windows)
            {
                textbox.Text = textbox.Text.Replace("\n", "\r\n");
            }
        }
    }
}
