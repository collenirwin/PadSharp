using PadSharp.Utils;
using System.Linq;
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
    }
}
