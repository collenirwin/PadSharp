using Microsoft.VisualStudio.TestTools.UnitTesting;
using PadSharp;
using PadSharp.Utils;
using System.Threading.Tasks;

namespace PadSharpTest
{
    [TestClass]
    public class StringUtilsTest
    {
        [TestMethod]
        public void TestSplitLines()
        {
            string[] expected = { "hello,", "world ", "How are you?" };
            string input = "hello,\r\nworld \nHow are you?";

            CollectionAssert.AreEqual(expected, input.SplitIntoLines());
        }

        [TestMethod]
        public void TestReverseLines()
        {
            string expected = "5\r\n4\r\n3\r\n2\r\n1";
            string input = "1\n2\r\n3\r\n4\n5";

            Assert.AreEqual(expected, input.ReverseLines());
        }

        [TestMethod]
        public void TestSortLines()
        {
            string expected = "_\r\n1\r\n2\r\na\r\nb\r\nc";
            string input = "a\r\n_\r\n2\nc\r\nb\n1";

            Assert.AreEqual(expected, input.SortLines());
        }

        [TestMethod]
        public void TestTitleCase()
        {
            string expected = "The  Quick Brown   Fox\r\n Jumped Over  The Lazy   \n    Dog";
            string input = "THe  qUiCK BROWN   fox\r\n JuMped oVEr  the Lazy   \n    Dog";

            Assert.AreEqual(expected, input.ToTitleCase());
        }

        [TestMethod]
        public void TestToggleCase()
        {
            string expected = "thE  QuIck brown   FOX\r\n jUmPED OveR  THE lAZY   \n    dOG";
            string input = "THe  qUiCK BROWN   fox\r\n JuMped oVEr  the Lazy   \n    Dog";

            Assert.AreEqual(expected, input.ToggleCase());
        }

        [TestMethod]
        public void TestPrependLines()
        {
            string expected = "***Hello\r\n***World\r\n***2748";
            string input = "Hello\nWorld\r\n2748";

            Assert.AreEqual(expected, input.PrependLines("***"));
        }

        [TestMethod]
        public void TestToggleLineStart()
        {
            string expected = "**Hello\r\n**World\r\n**2748";
            string input = "Hello\nWorld\r\n2748";

            Assert.AreEqual(expected, input.ToggleLineStart("**"));

            expected = "*Hello\r\n*World\r\n*2748";
            input = "**Hello\r\n**World\r\n**2748";

            Assert.AreEqual(expected, input.ToggleLineStart("*"));

            expected = "Hello\r\nWorld\r\n2748";
            input = "*Hello\r\n*World\r\n*2748";

            Assert.AreEqual(expected, input.ToggleLineStart("*"));

            expected = "* *Hello\r\n*World\r\n*_***2748";
            input = " *Hello\r\n**World\r\n_***2748";

            Assert.AreEqual(expected, input.ToggleLineStart("*"));

            expected = "";
            input = "*";

            Assert.AreEqual(expected, input.ToggleLineStart("*"));

            expected = "*";
            input = "";

            Assert.AreEqual(expected, input.ToggleLineStart("*"));

            expected = "";
            input = "";

            Assert.AreEqual(expected, input.ToggleLineStart(""));
        }

        [TestMethod]
        public void TestToggleStartAndEnd()
        {
            string expected = "Hello 1\r\n  2  \n3 World";
            string input = "1\r\n  2  \n3";

            Assert.AreEqual(expected, input.ToggleStartAndEnd("Hello ", " World"));
        }

        [TestMethod]
        public async Task TestTryCountMatchesAsync()
        {
            string text = "82634728sfbdgysyHelloeg b6fTD^&FSDGHGStsfWorldbbacs &%^%$A^*7)G";

            // test invalid regex
            Assert.IsNull(await text.TryCountMatchesAsync(@"\", true));
            Assert.IsNull(await text.TryCountMatchesAsync(@"\", false));

            // should be 10 digits
            Assert.AreEqual(10, await text.TryCountMatchesAsync(@"\d", true));
            Assert.AreEqual(10, await text.TryCountMatchesAsync(@"\d", false));

            // test match case
            Assert.AreEqual(0, await text.TryCountMatchesAsync("hello", true));
            Assert.AreEqual(1, await text.TryCountMatchesAsync("hello", false));
        }
    }
}
