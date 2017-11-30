using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ICSharpCode.AvalonEdit;
using System.Text.RegularExpressions;
using System.Windows.Controls;

namespace PadSharp
{
    public static class TextEditorUtils
    {
        public static bool findNext(TextEditor textbox, string text, 
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

                var regex = new Regex(text, options);
                var match = regex.Match(allText, start);

                if (!match.Success)
                {
                    // loop around and start from the opposite end
                    match = regex.Match(textbox.Text, lookback ? textbox.Text.Length : 0);
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

        //public static bool replaceNext(TextEditor textbox, string text, 
        //    int start, bool matchCase, bool lookback = false)
        //{

        //}
    }
}
