using System;
using System.Net;
using System.Threading.Tasks;

namespace PadSharp.Utils
{
    /// <summary>
    /// Provides the version number, and a method for checking that the current version is the latest
    /// </summary>
    public static class VersionChecker
    {
        /// <summary>
        /// Version of the currently running application
        /// </summary>
        public const string Version = "2.1.0";

        /// <summary>
        /// URL to the version file on GitHub
        /// </summary>
        public const string VersionUrl = "https://raw.githubusercontent.com/collenirwin/PadSharp/master/setup/version.txt";

        /// <summary>
        /// Version grabbed from <see cref="VersionUrl"/>
        /// </summary>
        public static string NewVersion { get; private set; }

        private static readonly Random _random = new Random();

        /// <summary>
        /// Fetches a version number from a file at <see cref="VersionUrl"/>
        /// </summary>
        /// <returns>The version number, or null if unsuccessful</returns>
        public static async Task<string> TryGetNewVersionAsync()
        {
            try
            {
                using var client = new WebClient();

                // fetch new version from the repo
                // add a random number to the end to avoid caching
                var newVersion = await client.DownloadStringTaskAsync($"{VersionUrl}?r={_random.Next(100000)}");
                NewVersion = newVersion;

                return newVersion;
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
