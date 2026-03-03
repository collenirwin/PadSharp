using System.Security.Cryptography;
using System.Text;

namespace PadSharp.Utils;

/// <summary>
/// Contains methods for hashing strings.
/// </summary>
public static class Crypto
{
    /// <summary>
    /// Uses SHA256 to hash a string (password) with the specified salt added.
    /// </summary>
    /// <param name="password">Password to hash.</param>
    /// <param name="salt">Salt string to add to the password hash.</param>
    /// <returns>The hashed password.</returns>
    public static string Hash(string password, string salt)
    {
        var hashString = new StringBuilder();
        byte[] bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password + salt));

        foreach (byte b in bytes)
        {
            // append bytes in base 16
            hashString.Append(b.ToString("x2"));
        }

        return hashString.ToString();
    }
}
