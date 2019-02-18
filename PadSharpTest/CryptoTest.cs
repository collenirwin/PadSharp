using Microsoft.VisualStudio.TestTools.UnitTesting;
using PadSharp;
using System;
using System.Text;

namespace PadSharpTest
{
    [TestClass]
    public class CryptoTest
    {
        Random ran = new Random();

        [TestMethod]
        public void TestEncrypt()
        {
            string data = "Hello world!\nIt's a wonderful  day";
            string password = "shgs7^nhsAufgGh983jmf";

            string encrpyted = Crypto.Encrypt(data, password);

            Console.WriteLine(encrpyted);
            Assert.AreNotEqual(data, encrpyted);
        }

        [TestMethod]
        public void TestDecrypt()
        {
            string data = "Hello world!\nIt's a wonderful  day ✔";
            string password = "shgs7^nhsA✔ufgGh983jmf";

            string encrpyted = Crypto.Encrypt(data, password);
            string decrypted = Crypto.Decrypt(encrpyted, password);

            Assert.AreEqual(data, decrypted);
        }

        [TestMethod]
        public void TestDecryptRandom()
        {
            // takes around 3 minutes on average on my machine
            for (int x = 0; x < 1000; x++)
            {
                var data = new StringBuilder();
                var password = new StringBuilder();

                for (int y = 0; y < ran.Next(1, 10000); y++)
                {
                    data.Append(Convert.ToChar(ran.Next(32, 127)));
                }

                for (int y = 0; y < ran.Next(1, 150); y++)
                {
                    password.Append(Convert.ToChar(ran.Next(32, 127)));
                }

                string encrypted = Crypto.Encrypt(data.ToString(), password.ToString());
                string decrypted = Crypto.Decrypt(encrypted, password.ToString());

                Assert.AreEqual(data.ToString(), decrypted);
            }
        }

        [TestMethod]
        public void TestALotOfData()
        {
            var data = new StringBuilder();
            var password = "huhf7sYf7ey 7tyn  HBSDYAGfhnhsndf32";

            for (int y = 0; y < 5000000; y++)
            {
                data.Append(Convert.ToChar(ran.Next(32, 127)));
            }

            string encrypted = Crypto.Encrypt(data.ToString(), password);
            string decrypted = Crypto.Decrypt(encrypted, password);

            Assert.AreEqual(data.ToString(), decrypted);
        }

        [TestMethod]
        public void TestLongPassword()
        {
            var data = "huhf7sYf7ey 7tyn  HBSDYAGfhnhsndf32";
            var password = new StringBuilder();

            for (int y = 0; y < 5000000; y++)
            {
                password.Append(Convert.ToChar(ran.Next(32, 127)));
            }

            string encrypted = Crypto.Encrypt(data, password.ToString());
            string decrypted = Crypto.Decrypt(encrypted, password.ToString());

            Assert.AreEqual(data.ToString(), decrypted);
        }

        [TestMethod]
        public void TestLongPasswordAndData()
        {
            var data = new StringBuilder();
            var password = new StringBuilder();

            for (int y = 0; y < 5000000; y++)
            {
                data.Append(Convert.ToChar(ran.Next(32, 127)));
            }

            for (int y = 0; y < 5000000; y++)
            {
                password.Append(Convert.ToChar(ran.Next(32, 127)));
            }

            string encrypted = Crypto.Encrypt(data.ToString(), password.ToString());
            string decrypted = Crypto.Decrypt(encrypted, password.ToString());

            Assert.AreEqual(data.ToString(), decrypted);
        }
    }
}
