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
        public const string Version = "1.4.8";
        public const string VersionUrl = "https://raw.githubusercontent.com/collenirwin/PadSharp/master/setup/version.txt";

        private static Random _random = new Random();

        /// <summary>
        /// Fetches a version number from a file at <see cref="VersionUrl"/>
        /// and compares it with <see cref="Version"/>.
        /// </summary>
        /// <param name="newVersion">Called if the fetched version is not the same as <see cref="Version"/> (if provided)</param>
        /// <param name="error">Called if an exception was thrown (if provided)</param>
        public static async void CheckVersion(Action<string> newVersion = null, Action<Exception> error = null)
        {
            try
            {
                string version = Version;

                using (var client = new WebClient())
                {
                    // fetch version from repo
                    version = await Task.Run(() => client.DownloadString(VersionUrl + 
                        "?r=" + _random.Next(100000).ToString())); // add a random number to the end to avoid caching
                }

                // if we have a new version and a callback
                if (newVersion != null && version != Version)
                {
                    newVersion(version);
                }
            }
            catch (Exception ex)
            {
                if (error != null)
                {
                    error(ex);
                }
            }
        }
    }
}
