using BoinWPF;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace PadSharp
{
    /// <summary>
    /// Container for common constants and methods
    /// </summary>
    public static class Global
    {
        /// <summary>
        /// Name of the app
        /// </summary>
        public const string APP_NAME = "Pad#";

        /// <summary>
        /// Current version
        /// </summary>
        public const string VERSION = VersionChecker.VERSION;

        /// <summary>
        /// c:/users/[user]/appdata/roaming/<see cref="APP_NAME"/>
        /// </summary>
        public static readonly string DATA_PATH = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            APP_NAME);

        /// <summary>
        /// Show an Alert dialog with the following format:
        /// [action]. (hidden in toggleable textbox): [message]
        /// </summary>
        /// <param name="action">What happened?</param>
        /// <param name="details">What more is there to say about this?</param>
        public static void actionMessage(string action, string details)
        {
            Alert.showMoreInfoDialog(action, details, APP_NAME);
        }

        /// <summary>
        /// Attempts to run the specified path/url
        /// </summary>
        /// <param name="path">Path to file to run</param>
        public static void launch(string path)
        {
            try
            {
                Process.Start(path);
            }
            catch (Exception ex)
            {
                string message = "Failed to launch '" + path + "'";
                Global.actionMessage(message, ex.Message);
                Logger.log(typeof(Global), ex, message);
            }
        }

        /// <summary>
        /// Creates the file's parent directory if it hasn't been created,
        /// then creates the specified (empty) file within it if it hasn't been created
        /// </summary>
        /// <param name="path">Full path to file</param>
        public static void createDirectoryAndFile(string path)
        {
            string dirPath = Path.Combine(path, "../");
            if (!Directory.Exists(dirPath))
            {
                Directory.CreateDirectory(dirPath);
            }

            if (!File.Exists(path))
            {
                using (var writer = File.CreateText(path))
                {
                    writer.Write("");
                }
            }
        }
    }
}
