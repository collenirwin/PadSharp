using PadSharp.Utils;
using Xunit;

namespace PadSharp.Tests
{
    public class StringUtils
    {
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
    }
}
