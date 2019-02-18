using BoinWPF;
using System;
using System.Diagnostics;
using System.IO;

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
        public const string AppName = "Pad#";

        /// <summary>
        /// Current version
        /// </summary>
        public const string Version = VersionChecker.Version;

        /// <summary>
        /// c:/users/[user]/appdata/roaming/<see cref="AppName"/>
        /// </summary>
        public static readonly string DataPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            AppName);

        /// <summary>
        /// Show an Alert dialog with the following format:
        /// [action]. (hidden in toggleable textbox): [message]
        /// </summary>
        /// <param name="action">What happened?</param>
        /// <param name="details">What more is there to say about this?</param>
        public static void ActionMessage(string action, string details)
        {
            Alert.showMoreInfoDialog(action, details, AppName);
        }

        /// <summary>
        /// Attempts to run the specified path/url
        /// </summary>
        /// <param name="path">Path to file to run</param>
        public static void Launch(string path)
        {
            try
            {
                Process.Start(path);
            }
            catch (Exception ex)
            {
                string message = "Failed to launch '" + path + "'";
                ActionMessage(message, ex.Message);
                Logger.Log(typeof(Global), ex, message);
            }
        }

        /// <summary>
        /// Creates the file's parent directory if it hasn't been created,
        /// then creates the specified (empty) file within it if it hasn't been created
        /// </summary>
        /// <param name="path">Full path to file</param>
        public static void CreateDirectoryAndFile(string path)
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
