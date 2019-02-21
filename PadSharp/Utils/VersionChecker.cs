using System;
using System.Net;
using System.Threading.Tasks;

namespace PadSharp
{
    /// <summary>
    /// Provides the version number, and a method for checking that the current version is the latest
    /// </summary>
    public static class VersionChecker
    {
        public const string Version = "1.5.0";
        public const string VersionUrl = "https://raw.githubusercontent.com/collenirwin/PadSharp/master/setup/version.txt";

        private static Random _random = new Random();

        /// <summary>
        /// Fetches a version number from a file at <see cref="VersionUrl"/>
        /// </summary>
        /// <returns>The version number, or null if unsuccessful</returns>
        public static async Task<string> TryGetNewVersionAsync()
        {
            try
            {
                using (var client = new WebClient())
                {
                    // fetch version from repo
                    // add a random number to the end to avoid caching
                    return await client.DownloadStringTaskAsync($"{VersionUrl}?r={_random.Next(100000)}");
                }
            }
            catch (Exception ex)
            {
                Logger.Log(typeof(VersionChecker), ex, "getting new version from github");
                return null;
            }
        }

        /// <summary>
        /// Checks if the version number in the file at <see cref="VersionUrl"/>
        /// is different from <see cref="Version"/>
        /// </summary>
        /// <returns>
        /// true if successful and the repo's version is not the same as the current version
        /// </returns>
        public static async Task<bool> NewVersionAvailableAsync()
        {
            return await TryGetNewVersionAsync() == Version;
        }
    }
}
