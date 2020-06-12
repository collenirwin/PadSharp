using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace PadSharp.Utils
{
    /// <summary>
    /// Contains methods for encrypting, decrypting, and hashing strings.
    /// </summary>
    public static class Crypto
    {
        private const int _keyByteCount = 32;
        private const int _pbkdf2Iterations = 10_000;

        /// <summary>
        /// Uses SHA256 to hash a string (password) with the specified salt added.
        /// </summary>
        /// <param name="password">Password to hash.</param>
        /// <param name="salt">Salt string to add to the password hash.</param>
        /// <returns>The hashed password.</returns>
        public static string Hash(string password, string salt)
        {
            var hashString = new StringBuilder();

            using var sha256 = new SHA256Managed();
            byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password + salt));

            foreach (byte b in bytes)
            {
                // append bytes in base 16
                hashString.Append(b.ToString("x2"));
            }

            return hashString.ToString();
        }

        /// <summary>
        /// Checks to see if a plaintext password with salt matches a hashed password.
        /// </summary>
        /// <param name="password">Plaintext password/</param>
        /// <param name="salt">Salt string to add.</param>
        /// <param name="hashString">Hashed password to check against.</param>
        /// <returns>true if the password matches.</returns>
        public static bool HashIsMatch(string password, string salt, string hashString) =>
            Hash(password, salt) == hashString;

        /// <summary>
        /// Uses Rijndael to encrypt the specified text with the specified password.
        /// </summary>
        /// <param name="text">Text to encrypt.</param>
        /// <param name="password">Password to use.</param>
        /// <returns>The encrypted text.</returns>
        public static string Encrypt(string text, string password)
        {
            // generate random salt and initialization vector
            var saltBytes = GenerateRandomByteArray(_keyByteCount);
            var ivBytes = GenerateRandomByteArray(_keyByteCount);
            var textBytes = Encoding.UTF8.GetBytes(text);

            // use PBKDF2 to generate a key
            using var key = new Rfc2898DeriveBytes(password, saltBytes, _pbkdf2Iterations);
            var keyBytes = key.GetBytes(_keyByteCount);

            using var aes = CreateAes();
            using var encryptor = aes.CreateEncryptor(keyBytes, ivBytes);
            using var memoryStream = new MemoryStream();
            using var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write);

            // encrypt all text
            cryptoStream.Write(textBytes, 0, textBytes.Length);

            // clear the buffer
            cryptoStream.FlushFinalBlock();

            // salt + iv + encrypted bytes
            var finalBytes = saltBytes
                .Concat(ivBytes)
                .Concat(memoryStream.ToArray())
                .ToArray();

            // convert our byte array to string and return
            return Convert.ToBase64String(finalBytes);
        }

        /// <summary>
        /// Decrypts text that was previously encrypted with <see cref="Encrypt(string, string)"/>.
        /// </summary>
        /// <param name="encryptedText">Encrypted text.</param>
        /// <param name="password">Password to use in decryption.</param>
        /// <returns>The decrypted string.</returns>
        public static string Decrypt(string encryptedText, string password)
        {
            // convert our encrypted string back to a byte array
            var allBytes = Convert.FromBase64String(encryptedText);

            // get the salt, which is the first KEY_SIZE_BYTES of the allBytes array
            var saltBytes = allBytes.Take(_keyByteCount).ToArray();

            // get the initialization vector, which is right after the salt (and the same size)
            var ivBytes = allBytes.Skip(_keyByteCount).Take(_keyByteCount).ToArray();

            // get the encrypted text, which comes after the salt and the bytes
            var encryptedBytes = allBytes.Skip(_keyByteCount * 2).ToArray();

            using var key = new Rfc2898DeriveBytes(password, saltBytes, _pbkdf2Iterations);
            byte[] keyBytes = key.GetBytes(_keyByteCount);

            using var aes = CreateAes();
            using var decryptor = aes.CreateDecryptor(keyBytes, ivBytes);
            using var memoryStream = new MemoryStream(encryptedBytes);
            using var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);

            // a byte array to hold the unencrypted text
            var textBytes = new byte[encryptedBytes.Length];

            // decrypt all text, throw it into textBytes, get the length of the actual decrpyted bytes
            int count = cryptoStream.Read(textBytes, 0, textBytes.Length);

            // convert to string using the length we found (otherise there could be unneeded null characters)
            return Encoding.UTF8.GetString(textBytes, 0, count);
        }

        /// <summary>
        /// Creates a <see cref="RijndaelManaged"/> object,
        /// sets BlockSize to 256,
        /// sets Mode to CBC,
        /// sets Padding to PKCS7
        /// </summary>
        private static RijndaelManaged CreateAes() =>
            new RijndaelManaged
            {
                BlockSize = 256,
                Mode = CipherMode.CBC,
                Padding = PaddingMode.PKCS7
            };

        /// <summary>
        /// Generates an array of random bytes of a specified size.
        /// </summary>
        /// <param name="numberOfBytes">Size of the array.</param>
        /// <returns>An array of random bytes.</returns>
        public static byte[] GenerateRandomByteArray(int numberOfBytes)
        {
            var bytes = new byte[numberOfBytes];

            using var random = new RNGCryptoServiceProvider();
            random.GetBytes(bytes);

            return bytes;
        }
    }
}
