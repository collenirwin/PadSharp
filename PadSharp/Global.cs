using BoinWPF;
using System;
using System.IO;

namespace PadSharp
{
    public static class Global
    {
        /// <summary>
        /// Name of the app
        /// </summary>
        public const string APP_NAME = "Pad#";

        /// <summary>
        /// Current version
        /// </summary>
        public const string VERSION = "1.1.2";

        /// <summary>
        /// c:/users/[user]/appdata/roaming/<see cref="APP_NAME"/>
        /// </summary>
        public static readonly string DATA_PATH = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            APP_NAME);

        /// <summary>
        /// Show an Alert dialog with the following format:
        /// [action]. Here are some additional details: [details]
        /// </summary>
        /// <param name="action">What happened?</param>
        /// <param name="details">What more is there to say about this?</param>
        public static void actionMessage(string action, string details)
        {
            Alert.showDialog(action + ". Here are some additional details: " + details, APP_NAME);
        }

        /// <summary>
        /// Creates the file's parent directory if it hasn't been created,
        /// then creates the specified (empty) file within it
        /// </summary>
        /// <param name="path">Full path to file</param>
        public static void createDirectoryAndFile(string path)
        {
            string dirPath = Path.Combine(path, "../");
            if (!Directory.Exists(dirPath))
            {
                Directory.CreateDirectory(dirPath);
            }

            using (var writer = File.CreateText(path))
            {
                writer.Write("");
            }
        }
    }
}
