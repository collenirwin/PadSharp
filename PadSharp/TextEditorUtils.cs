using ICSharpCode.AvalonEdit;
using System.Text.RegularExpressions;

namespace PadSharp
{
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
        /// within textbox.Text and replace it with replacement.
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
    }
}
