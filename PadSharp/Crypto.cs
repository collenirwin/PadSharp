using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace BoinVault
{
    public static class Crypto
    {

        const int KEY_SIZE_BYTES = 32;
        const int PBKDF2_ITERATIONS = 4000;

        /// <summary>
        /// Uses SHA256 to hash a string (password) with the specified salt added
        /// </summary>
        /// <param name="password">password to hash</param>
        /// <param name="salt">salt string to add to the password hash</param>
        /// <returns>the hashed password</returns>
        public static string hash(string password, string salt)
        {
            var hashString = new StringBuilder();

            // sha256 hash
            using (var sha256 = new SHA256Managed())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password + salt));

                foreach (byte b in bytes)
                {
                    // append bytes in base 16
                    hashString.Append(b.ToString("x2"));
                }
            }

            return hashString.ToString();
        }

        /// <summary>
        /// Checks to see if a plaintext password with salt matches a hashed password
        /// </summary>
        /// <param name="password">plaintext password</param>
        /// <param name="salt">salt string to add</param>
        /// <param name="hashString">hashed password to check against</param>
        /// <returns>true if the password matches</returns>
        public static bool hashMatch(string password, string salt, string hashString)
        {
            return hash(password, salt) == hashString;
        }

        /// <summary>
        /// Uses Rijndael to encrypt the specified text with the specified password
        /// </summary>
        /// <param name="text">text to encrypt</param>
        /// <param name="password">password to use</param>
        /// <returns>the encrypted string</returns>
        public static string encrypt(string text, string password)
        {
            // generate random salt and initialization vector, we will prepend to string later
            byte[] saltBytes = genRandomByteArray(KEY_SIZE_BYTES);
            byte[] ivBytes = genRandomByteArray(KEY_SIZE_BYTES);
            byte[] textBytes = Encoding.UTF8.GetBytes(text);

            // use PBKDF2 to generate a key
            using (var key = new Rfc2898DeriveBytes(password, saltBytes, PBKDF2_ITERATIONS))
            {
                byte[] keyBytes = key.GetBytes(KEY_SIZE_BYTES);

                // not technically AES because our block size is not 128,
                // but it's an easier name to remember
                using (var aes = new RijndaelManaged())
                {
                    setAesSettings(aes);

                    using (var encryptor = aes.CreateEncryptor(keyBytes, ivBytes))
                    using (var memoryStream = new MemoryStream())
                    using (var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                    {
                        // encrypt all text
                        cryptoStream.Write(textBytes, 0, textBytes.Length);

                        // clear the buffer
                        cryptoStream.FlushFinalBlock();

                        // prepend the salt and iv to the encrypted bytes
                        byte[] finalBytes = saltBytes; // salt
                        finalBytes = finalBytes.Concat(ivBytes).ToArray(); // IV
                        finalBytes = finalBytes.Concat(memoryStream.ToArray()).ToArray(); // the rest

                        // convert our byte array to string and return
                        return Convert.ToBase64String(finalBytes);
                    }
                }
            }
        }

        /// <summary>
        /// Decrypts text that was previously encrypted with Crypto.encrypt
        /// </summary>
        /// <param name="encryptedText">previously encrypted text</param>
        /// <param name="password">password to encrypt decrypt with</param>
        /// <returns>the decrypted string</returns>
        public static string decrypt(string encryptedText, string password)
        {
            // convert our encrypted string back to a byte array
            byte[] allBytes = Convert.FromBase64String(encryptedText);

            // get the salt, which is the first KEY_SIZE_BYTES of the allBytes array
            byte[] saltBytes = allBytes.Take(KEY_SIZE_BYTES).ToArray();

            // get the initialization vector, which is right after the salt (and the same size)
            byte[] ivBytes = allBytes.Skip(KEY_SIZE_BYTES).Take(KEY_SIZE_BYTES).ToArray();

            // get the encrypted text, which comes after the salt and the bytes
            byte[] encryptedBytes = allBytes.Skip(KEY_SIZE_BYTES * 2).ToArray();

            using (var key = new Rfc2898DeriveBytes(password, saltBytes, PBKDF2_ITERATIONS))
            {
                byte[] keyBytes = key.GetBytes(KEY_SIZE_BYTES);

                using (var aes = new RijndaelManaged())
                {
                    setAesSettings(aes);

                    using (var decryptor = aes.CreateDecryptor(keyBytes, ivBytes))
                    using (var memoryStream = new MemoryStream(encryptedBytes))
                    using (var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
                    {
                        // a byte array to hold the unencrypted text
                        byte[] textBytes = new byte[encryptedBytes.Length];

                        // decrypt all text, throw it into textBytes, get the length of the actual decrpyted bytes
                        int count = cryptoStream.Read(textBytes, 0, textBytes.Length);

                        // convert to string using the length we found (otherise there could be unneeded null characters)
                        return Encoding.UTF8.GetString(textBytes, 0, count);
                    }
                }
            }
        }

        /// <summary>
        /// sets BlockSize to 256,
        /// sets Mode to CBC,
        /// sets Padding to PKCS7
        /// </summary>
        private static void setAesSettings(RijndaelManaged aes)
        {
            aes.BlockSize = 256;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
        }

        /// <summary>
        /// Generates an array of random bytes of a specified size
        /// </summary>
        /// <param name="numberOfBytes">size of the array in bytes</param>
        /// <returns>an array of random bytes</returns>
        public static byte[] genRandomByteArray(int numberOfBytes)
        {
            byte[] bytes = new byte[numberOfBytes];

            using (var ran = new RNGCryptoServiceProvider())
            {
                ran.GetBytes(bytes);
            }

            return bytes;
        }
    }
}
