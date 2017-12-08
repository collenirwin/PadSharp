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

        /// <summary>
        /// Sorts the specified lines in the <see cref="TextEditor"/> provided.
        /// Does nothing if textbox does not conain linesToSort.
        /// </summary>
        /// <param name="textbox"><see cref="TextEditor"/> to sort lines in</param>
        /// <param name="linesToSort">Lines to sort</param>
        public static void sortLines(TextEditor textbox, string linesToSort)
        {
            int index = textbox.Text.IndexOf(linesToSort);

            if (index != -1)
            {
                textbox.Text = textbox.Text
                    .Remove(index, linesToSort.Length)
                    .Insert(index, string.Join("\r\n", 
                        linesToSort.Replace("\r", "").Split('\n').OrderBy(x => x)));
            }
        }

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
