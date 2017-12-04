using System;
using System.Net;
using System.Threading.Tasks;

namespace PadSharp
{
    public static class VersionChecker
    {
        public const string VERSION = "1.2.0";
        public const string VERSION_URL = "https://github.com/collenirwin/PadSharp/blob/master/setup/version.txt";

        /// <summary>
        /// Fetches a version number from a file at <see cref="VERSION_URL"/>
        /// and compares it with <see cref="VERSION"/>.
        /// </summary>
        /// <param name="newVersion">Called if the fetched version is not the same as <see cref="VERSION"/> (if provided)</param>
        /// <param name="error">Called if an exception was thrown (if provided)</param>
        public static async void checkVersion(Action<string> newVersion = null, Action<Exception> error = null)
        {
            try
            {
                string version = VERSION;

                using (var client = new WebClient())
                {
                    // fetch version from repo
                    version = await Task.Run(() => client.DownloadString(VERSION_URL));
                }

                // if we have a new version and a callback
                if (newVersion != null && version != VERSION)
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
