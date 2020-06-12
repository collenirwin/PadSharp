using PadSharp.Utils;
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
    }
}
