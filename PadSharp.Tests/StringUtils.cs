using PadSharp.Utils;
using System.Threading.Tasks;
using Xunit;

namespace PadSharp.Tests
{
    public class StringUtils
    {
        #region SplitIntoLines

        [Fact]
        public void SplitIntoLines_OneLine_ReturnsSingle()
        {
            // arrange
            string line = "Hello.";

            // act
            var actual = line.SplitIntoLines();

            // assert
            Assert.Single(actual, line);
        }

        [Theory]
        [InlineData("Line 1\nLine 2", 2)]
        [InlineData("Line 1\nLine 2\n", 3)]
        [InlineData("\n\n\n", 4)]
        public void SplitIntoLines_MultiLineLF_ReturnsMultiple(string input, int expected)
        {
            // arrange, act
            var actual = input.SplitIntoLines();

            // assert
            Assert.Equal(expected, actual.Length);
        }

        [Theory]
        [InlineData("Line 1\r\nLine 2", 2)]
        [InlineData("Line 1\r\nLine 2\r\n", 3)]
        [InlineData("\r\n\r\n\r\n", 4)]
        public void SplitIntoLines_MultiLineCRLF_ReturnsMultiple(string input, int expected)
        {
            // arrange, act
            var actual = input.SplitIntoLines();

            // assert
            Assert.Equal(expected, actual.Length);
        }

        [Theory]
        [InlineData("Line 1\r\nLine 2\n", 3)]
        [InlineData("Line 1\nLine 2\r\n", 3)]
        [InlineData("\r\n\n\r\n", 4)]
        public void SplitIntoLines_MultiLineMixedLineEndings_ReturnsMultiple(string input, int expected)
        {
            // arrange, act
            var actual = input.SplitIntoLines();

            // assert
            Assert.Equal(expected, actual.Length);
        }

        #endregion

        #region ReverseLines

        [Fact]
        public void ReverseLines_OneLine_ReturnsSameString()
        {
            // arrange
            string expected = "Hello.";

            // act
            string actual = expected.ReverseLines();

            // assert
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("Line 1\nLine 2", "Line 2\r\nLine 1")]
        [InlineData("Line 1\nLine 2\n", "\r\nLine 2\r\nLine 1")]
        [InlineData("\n\n\n", "\r\n\r\n\r\n")]
        public void ReverseLines_MultiLineLFEndings_ReturnsReverseCRLF(string input, string expected)
        {
            // arrange, act
            string actual = input.ReverseLines();

            // assert
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("Line 1\r\nLine 2", "Line 2\r\nLine 1")]
        [InlineData("Line 1\r\nLine 2\r\n", "\r\nLine 2\r\nLine 1")]
        [InlineData("\r\n\r\n\r\n", "\r\n\r\n\r\n")]
        public void ReverseLines_MultiLineCRLFEndings_ReturnsReverseCRLF(string input, string expected)
        {
            // arrange, act
            string actual = input.ReverseLines();

            // assert
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("Line 1\r\nLine 2\n", "\r\nLine 2\r\nLine 1")]
        [InlineData("Line 1\nLine 2\r\n", "\r\nLine 2\r\nLine 1")]
        [InlineData("\r\n\n\r\n", "\r\n\r\n\r\n")]
        public void ReverseLines_MultiLineMixedLineEndings_ReturnsReverseCRLF(string input, string expected)
        {
            // arrange, act
            string actual = input.ReverseLines();

            // assert
            Assert.Equal(expected, actual);
        }

        #endregion

        #region SortLines

        [Fact]
        public void SortLines_OneLine_ReturnsSameString()
        {
            // arrange
            string expected = "Hello.";

            // act
            string actual = expected.SortLines();

            // assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void SortLines_OneLineDescending_ReturnsSameString()
        {
            // arrange
            string expected = "Hello.";

            // act
            string actual = expected.SortLines(descending: true);

            // assert
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("b\na", "a\r\nb")]
        [InlineData("a\nb\n", "\r\na\r\nb")]
        [InlineData("\n\n\n", "\r\n\r\n\r\n")]
        public void SortLines_MultiLineLFEndings_ReturnsSortedCRLF(string input, string expected)
        {
            // arrange, act
            string actual = input.SortLines();

            // assert
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("b\r\na", "a\r\nb")]
        [InlineData("a\r\nb\r\n", "\r\na\r\nb")]
        [InlineData("\r\n\r\n\r\n", "\r\n\r\n\r\n")]
        public void SortLines_MultiLineCRLFEndings_ReturnsSortedCRLF(string input, string expected)
        {
            // arrange, act
            string actual = input.SortLines();

            // assert
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("b\na\r\n", "\r\na\r\nb")]
        [InlineData("a\r\nb\n", "\r\na\r\nb")]
        [InlineData("\r\n\n\r\n", "\r\n\r\n\r\n")]
        public void SortLines_MultiLineMixedLineEndings_ReturnsSortedCRLF(string input, string expected)
        {
            // arrange, act
            string actual = input.SortLines();

            // assert
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("a\nA", "a\r\nA")]
        [InlineData("A\na", "a\r\nA")]
        public void SortLines_MixedCase_ReturnsSortedLowercaseFirst(string input, string expected)
        {
            // arrange, act
            string actual = input.SortLines();

            // assert
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("a\nb", "a\r\nb")]
        [InlineData("b\na", "a\r\nb")]
        [InlineData("3\na\n1\n_", "_\r\n1\r\n3\r\na")]
        public void SortLines_General_ReturnsSortedDescending(string input, string expected)
        {
            // arrange, act
            string actual = input.SortLines();

            // assert
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("a\nb", "b\r\na")]
        [InlineData("b\na", "b\r\na")]
        [InlineData("3\na\n1\n_", "a\r\n3\r\n1\r\n_")]
        public void SortLines_GeneralDescending_ReturnsSortedDescending(string input, string expected)
        {
            // arrange, act
            string actual = input.SortLines(descending: true);

            // assert
            Assert.Equal(expected, actual);
        }

        #endregion

        #region ToTitleCase

        [Fact]
        public void ToTitleCase_EmptyString_ReturnsSameString()
        {
            // arrange
            string expected = "";

            // act
            string actual = expected.ToTitleCase();

            // assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void ToTitleCase_SingleWord_ReturnsWordInTitleCase()
        {
            // arrange
            string input = "hELlo";

            // act
            string actual = input.ToTitleCase();

            // assert
            Assert.Equal("Hello", actual);
        }

        [Theory]
        [InlineData("hello world", "Hello World")]
        [InlineData("i aM cool", "I Am Cool")]
        [InlineData("1 is not A LETTER", "1 Is Not A Letter")]
        public void ToTitleCase_MultipleWords_ReturnsWordsInTitleCase(string input, string expected)
        {
            // arrange, act
            string actual = input.ToTitleCase();

            // assert
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("'hello world!'", "'Hello World!'")]
        [InlineData("new.sentence", "New.Sentence")]
        [InlineData("split-word", "Split-Word")]
        public void ToTitleCase_WordsWithSymbols_ReturnsWordsInTitleCaseIgnoringSymbols(string input, string expected)
        {
            // arrange, act
            string actual = input.ToTitleCase();

            // assert
            Assert.Equal(expected, actual);
        }

        #endregion

        #region ToggleCase

        [Fact]
        public void ToggleCase_EmptyString_ReturnsSameString()
        {
            // arrange
            string expected = "";

            // act
            string actual = expected.ToggleCase();

            // assert
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("a", "A")]
        [InlineData("A", "a")]
        public void ToggleCase_SingleLetter_ReturnsOppositeCase(string input, string expected)
        {
            // arrange, act
            string actual = input.ToggleCase();

            // assert
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("hELLO wORLD", "Hello World")]
        [InlineData("a b c D", "A B C d")]
        public void ToggleCase_MixedCase_ReturnsOppositeCase(string input, string expected)
        {
            // arrange, act
            string actual = input.ToggleCase();

            // assert
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("0123456789")]
        [InlineData("_^&*/\\\n\r$@!(){}-=+~`,.<>?")]
        public void ToggleCase_NumbersAndSymbols_ReturnsSameString(string input)
        {
            // arrange, act
            string actual = input.ToggleCase();

            // assert
            Assert.Equal(input, actual);
        }

        #endregion

        #region PrependLines

        [Fact]
        public void PrependLines_EmptyString_ReturnsSameString()
        {
            // arrange
            string expected = "";

            // act
            string actual = expected.PrependLines("");

            // assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void PrependLines_EmptyStringWithText_ReturnsText()
        {
            // arrange
            string input = "";

            // act
            string actual = input.PrependLines("text");

            // assert
            Assert.Equal("text", actual);
        }

        [Fact]
        public void PrependLines_TextWithEmptyString_ReturnsText()
        {
            // arrange
            string expected = "text";

            // act
            string actual = expected.PrependLines("");

            // assert
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("a", "b", "ba")]
        [InlineData("input", "prepend ", "prepend input")]
        public void PrependLines_SingleLine_ReturnsLinePrepended(string input, string prepend, string expected)
        {
            // arrange, act
            string actual = input.PrependLines(prepend);

            // assert
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("A\nB", "pre", "preA\r\npreB")]
        [InlineData("A\n", "pre", "preA\r\npre")]
        [InlineData("1\r\n2\n3", "pre", "pre1\r\npre2\r\npre3")]
        public void PrependLines_MultiLine_ReturnsLinesPrepended(string input, string prepend, string expected)
        {
            // arrange, act
            string actual = input.PrependLines(prepend);

            // assert
            Assert.Equal(expected, actual);
        }

        #endregion

        #region ToggleLineStart

        [Theory]
        [InlineData("")]
        [InlineData("text")]
        public void ToggleLineStart_EmptyString_ReturnsSameString(string input)
        {
            // arrange, act
            string actual = input.ToggleLineStart("");

            // assert
            Assert.Equal(input, actual);
        }

        [Theory]
        [InlineData("", "a", "a")]
        [InlineData("a", "a", "")]
        [InlineData("aa", "a", "a")]
        public void ToggleLineStart_SingleLine_ReturnsStartToggled(string input, string toggle, string expected)
        {
            // arrange, act
            string actual = input.ToggleLineStart(toggle);

            // assert
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("a\nb", "a", "\r\nab")]
        [InlineData("a\nab", "a", "\r\nb")]
        [InlineData("a\na\r\nb", "a", "\r\n\r\nab")]
        public void ToggleLineStart_MultiLine_ReturnsLineStartToggled(string input, string toggle, string expected)
        {
            // arrange, act
            string actual = input.ToggleLineStart(toggle);

            // assert
            Assert.Equal(expected, actual);
        }

        #endregion

        #region ToggleStartAndEnd

        [Theory]
        [InlineData("")]
        [InlineData("text")]
        public void ToggleStartAndEnd_EmptyString_ReturnsSameString(string input)
        {
            // arrange, act
            string actual = input.ToggleStartAndEnd(start: "", end: "");

            // assert
            Assert.Equal(input, actual);
        }

        [Theory]
        [InlineData("", "a", "a")]
        [InlineData("a", "a", "")]
        [InlineData("aa", "a", "a")]
        [InlineData("abc", "a", "bc")]
        [InlineData("bc", "a", "abc")]
        public void ToggleStartAndEnd_StartOnly_ReturnsStartToggled(string input, string start, string expected)
        {
            // arrange, act
            string actual = input.ToggleStartAndEnd(start, end: "");

            // assert
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("", "a", "a")]
        [InlineData("a", "a", "")]
        [InlineData("aa", "a", "a")]
        [InlineData("abc", "c", "ab")]
        [InlineData("abc", "a", "abca")]
        public void ToggleStartAndEnd_EndOnly_ReturnsEndToggled(string input, string end, string expected)
        {
            // arrange, act
            string actual = input.ToggleStartAndEnd(start: "", end);

            // assert
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("abc", "a", "c", "b")]
        [InlineData("b", "a", "c", "abc")]
        [InlineData("bc", "a", "c", "abc")]
        [InlineData("ab", "a", "c", "abc")]
        public void ToggleStartAndEnd_StartAndEnd_ReturnsStartAndEndToggled(string input,
            string start, string end, string expected)
        {
            // arrange, act
            string actual = input.ToggleStartAndEnd(start, end);

            // assert
            Assert.Equal(expected, actual);
        }

        #endregion

        #region TryCountMatchesAsync

        [Fact]
        public async Task TryCountMatchesAsync_EmptyString_ReturnsNull()
        {
            // arrange
            string input = "";

            // act
            var actual = await input.TryCountMatchesAsync(@"\d", matchCase: false);

            // assert
            Assert.Null(actual);
        }

        [Fact]
        public async Task TryCountMatchesAsync_EmptyRegex_ReturnsNull()
        {
            // arrange
            string input = "test";

            // act
            var actual = await input.TryCountMatchesAsync("", matchCase: false);

            // assert
            Assert.Null(actual);
        }

        [Theory]
        [InlineData("1", @"\d", true, 1)]
        [InlineData("1", @"\d", false, 1)]
        [InlineData("123abc456", @"\d", true, 6)]
        [InlineData("123abc456", @"\d", false, 6)]
        [InlineData("123abc456", @"\w+", true, 1)]
        [InlineData("123abc456", @"\w+", false, 1)]
        [InlineData("aaaAA", "a", true, 3)]
        [InlineData("aaaAA", "a", false, 5)]
        public async Task TryCountMatchesAsync_GoodInput_ReturnsCorrectMatchCount(string input,
            string regex, bool matchCase, int? expected)
        {
            // arrange, act
            var actual = await input.TryCountMatchesAsync(regex, matchCase);

            // assert
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("1", true)]
        [InlineData("1", false)]
        public async Task TryCountMatchesAsync_InvalidRegex_ReturnsNull(string input, bool matchCase)
        {
            // arrange, act
            var actual = await input.TryCountMatchesAsync(@"\", matchCase);

            // assert
            Assert.Null(actual);
        }

        #endregion
    }
}
