using ICSharpCode.AvalonEdit;
using System;
using System.Text.RegularExpressions;

namespace PadSharp
{
    /// <summary>
    /// Contains various extension methods for working with text within an AvalonEdit TextEditor
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
        /// <param name="matchCase">Use IgnoreCase flag?</param>
        /// <param name="lookback">Search back from this point?</param>
        /// <returns>true if found, false if regex is incorrect or if not found</returns>
        public static bool findNext(this TextEditor textbox, string regex, 
            int start, bool matchCase, bool lookback = false)
        {
            try
            {
                var options = matchCase
                    ? RegexOptions.None
                    : RegexOptions.IgnoreCase;

                options |= RegexOptions.Multiline;

                if (lookback)
                {
                    options |= RegexOptions.RightToLeft;
                }

                var _regex = new Regex(regex, options);
                var match = _regex.Match(textbox.Text, start);

                if (!match.Success)
                {
                    // loop around and start from the opposite end
                    match = _regex.Match(textbox.Document.Text, lookback ? textbox.Document.Text.Length : 0);
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
        /// <param name="matchCase">Use IgnoreCase flag?</param>
        /// <param name="lookback">Search back from this point?</param>
        /// <returns>true if found, false if regex is incorrect or if not found</returns>
        public static bool replaceNext(this TextEditor textbox, string regex, string replacement,
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
                textbox.Document.Text = textbox.Document.Text
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
        /// <param name="matchCase">Use IgnoreCase flag?</param>
        /// <param name="predicate">
        /// Number of replacements will be passed to this.
        /// It will decide if the replacement will run.
        /// </param>
        /// <returns>
        /// true if found, false if regex is incorrect or if not found or if predicate returns false
        /// </returns>
        public static bool replaceAll(this TextEditor textbox, string regex, string replacement,
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
                textbox.Document.Text = _regex.Replace(textbox.Document.Text, replacement);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Replaces the first instance of originalText with newText if it exists within the <see cref="TextEditor"/> provided.
        /// Does nothing otherwise.
        /// </summary>
        /// <param name="textbox"><see cref="TextEditor"/> to replace text in</param>
        /// <param name="originalText">Text to replace</param>
        /// <param name="newText">Text to replace originalText with</param>
        private static void replaceIfExists(this TextEditor textbox, string originalText, string newText)
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

        #endregion

        /// <summary>
        /// Replaces the selected text within the <see cref="TextEditor"/> provided.
        /// Does nothing is textbox.SelectionLength < 0.
        /// </summary>
        /// <param name="textbox"><see cref="TextEditor"/> to replace selection in</param>
        /// <param name="replacement">Text to replace selection with</param>
        public static void replaceSelectedText(this TextEditor textbox, string replacement)
        {
            if (textbox.SelectionLength > 0)
            {
                int start = textbox.SelectionStart;

                // replace selected text
                textbox.Document.Text = textbox.Document.Text
                    .Remove(start, textbox.SelectionLength)
                    .Insert(start, replacement);

                // select the new text
                textbox.Select(start, replacement.Length);
            }
        }

        /// <summary>
        /// Set all line endings to \r\n (windows = true) or \n (windows = false)
        /// </summary>
        /// <param name="textbox"><see cref="TextEditor"/> we're working with</param>
        /// <param name="windows">Use Windows line endings?</param>
        public static void normalizeLineEndings(this TextEditor textbox, bool windows)
        {
            textbox.Text = textbox.Text.Replace("\r", "");

            if (windows)
            {
                textbox.Text = textbox.Text.Replace("\n", "\r\n");
            }
        }
    }
}
