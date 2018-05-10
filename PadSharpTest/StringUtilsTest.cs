using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PadSharp;

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

            CollectionAssert.AreEqual(expected, input.splitLines());
        }

        [TestMethod]
        public void TestReverseLines()
        {
            string expected = "5\r\n4\r\n3\r\n2\r\n1";
            string input = "1\n2\r\n3\r\n4\n5";

            Assert.AreEqual(expected, input.reverseLines());
        }

        [TestMethod]
        public void TestSortLines()
        {
            string expected = "_\r\n1\r\n2\r\na\r\nb\r\nc";
            string input = "a\r\n_\r\n2\nc\r\nb\n1";

            Assert.AreEqual(expected, input.sortLines());
        }

        [TestMethod]
        public void TestTitleCase()
        {
            string expected = "The  Quick Brown   Fox\r\n Jumped Over  The Lazy   \n    Dog";
            string input = "THe  qUiCK BROWN   fox\r\n JuMped oVEr  the Lazy   \n    Dog";

            Assert.AreEqual(expected, input.titleCase());
        }

        [TestMethod]
        public void TestToggleCase()
        {
            string expected = "thE  QuIck brown   FOX\r\n jUmPED OveR  THE lAZY   \n    dOG";
            string input = "THe  qUiCK BROWN   fox\r\n JuMped oVEr  the Lazy   \n    Dog";

            Assert.AreEqual(expected, input.toggleCase());
        }

        [TestMethod]
        public void TestPrependLines()
        {
            string expected = "***Hello\r\n***World\r\n***2748";
            string input = "Hello\nWorld\r\n2748";

            Assert.AreEqual(expected, input.prependLines("***"));
        }

        [TestMethod]
        public void TestToggleLineStart()
        {
            string expected = "**Hello\r\n**World\r\n**2748";
            string input = "Hello\nWorld\r\n2748";

            Assert.AreEqual(expected, input.toggleLineStart("**"));

            expected = "*Hello\r\n*World\r\n*2748";
            input = "**Hello\r\n**World\r\n**2748";

            Assert.AreEqual(expected, input.toggleLineStart("*"));

            expected = "Hello\r\nWorld\r\n2748";
            input = "*Hello\r\n*World\r\n*2748";

            Assert.AreEqual(expected, input.toggleLineStart("*"));

            expected = "* *Hello\r\n*World\r\n*_***2748";
            input = " *Hello\r\n**World\r\n_***2748";

            Assert.AreEqual(expected, input.toggleLineStart("*"));
        }

        [TestMethod]
        public void TestToggleStartAndEnd()
        {
            string expected = "Hello 1\r\n  2  \n3 World";
            string input = "1\r\n  2  \n3";

            Assert.AreEqual(expected, input.toggleStartAndEnd("Hello ", " World"));
        }
    }
}
