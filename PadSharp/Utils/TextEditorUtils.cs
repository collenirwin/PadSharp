using ICSharpCode.AvalonEdit;
using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace PadSharp.Utils
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
        public static bool FindNext(this TextEditor textbox, string regex, 
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
        public static bool ReplaceNext(this TextEditor textbox, string regex, string replacement,
            int start, bool matchCase, bool lookback = false)
        {
            try
            {
                if (regex == "")
                {
                    return false;
                }

                if (FindNext(textbox, regex, start, matchCase, lookback))
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
            catch
            {
                // invalid regular expression
                return false;
            }
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
        public static bool ReplaceAll(this TextEditor textbox, string regex, string replacement,
            bool matchCase, Predicate<int> predicate = null)
        {
            try
            {
                if (regex == "")
                {
                    return false;
                }

                var options = matchCase ? RegexOptions.None : RegexOptions.IgnoreCase;
                options |= RegexOptions.Multiline;

                var _regex = new Regex(regex, options);
                var matches = _regex.Matches(textbox.Text);

                if (predicate == null || predicate(matches.Count))
                {
                    textbox.Document.Text = _regex.Replace(textbox.Document.Text, replacement);
                    return true;
                }
            }
            catch { /* invalid regex - continue */ }
            return false;
        }

        #endregion

        /// <summary>
        /// Inserts the specified text into the <see cref="TextEditor"/> provided,
        /// then moves the caret to the end of the inserted text
        /// </summary>
        /// <param name="textbox"><see cref="TextEditor"/> to insert text into</param>
        /// <param name="text">Text to insert</param>
        public static void Insert(this TextEditor textbox, string text)
        {
            // grab position we're going to before we reset textbox.Text
            int position = textbox.CaretOffset + text.Length;

            // insert the text
            textbox.Document.Text = textbox.Document.Text.Insert(textbox.CaretOffset, text);

            // go to the previously calculated position
            textbox.CaretOffset = position;

            // we should not have a selection at this point
            textbox.SelectionLength = 0;
        }

        /// <summary>
        /// Replaces the selected text within the <see cref="TextEditor"/> provided.
        /// Does nothing is textbox.SelectionLength < 0.
        /// </summary>
        /// <param name="textbox"><see cref="TextEditor"/> to replace selection in</param>
        /// <param name="replacement">Text to replace selection with</param>
        public static void ReplaceSelectedText(this TextEditor textbox, string replacement)
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
        /// Toggles the given lineStart at the start of each select selected line (regardless of start/caret position).
        /// Selects the line the caret is on if there is no current selection.
        /// </summary>
        /// <param name="textbox"><see cref="TextEditor"/> we're working with</param>
        /// <param name="lineStart">Text to toggle at the start of each selected line</param>
        public static void ToggleSelectionLineStart(this TextEditor textbox, string lineStart)
        {
            var startLine = textbox.Document.GetLineByOffset(textbox.SelectionStart);
            textbox.SelectionStart = startLine.Offset;

            if (!textbox.SelectedText.Contains('\n'))
            {
                textbox.SelectionLength = startLine.Length;
            }

            textbox.ReplaceSelectedText(textbox.SelectedText.ToggleLineStart(lineStart));
        }

        /// <summary>
        /// Set all line endings to \r\n (windows = true) or \n (windows = false)
        /// </summary>
        /// <param name="textbox"><see cref="TextEditor"/> we're working with</param>
        /// <param name="windows">Use Windows line endings?</param>
        public static void NormalizeLineEndings(this TextEditor textbox, bool windows)
        {
            textbox.Document.Text = textbox.Document.Text.Replace("\r", "");

            if (windows)
            {
                textbox.Document.Text = textbox.Document.Text.Replace("\n", "\r\n");
            }
        }
    }
}
