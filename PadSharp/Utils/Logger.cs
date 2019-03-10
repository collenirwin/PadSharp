using System;
using System.IO;

namespace PadSharp.Utils
{
    /// <summary>
    /// Contains methods for logging information to a file
    /// </summary>
    public static class Logger
    {
        /// <summary>
        /// Path to the file we're logging to
        /// </summary>
        public static readonly string FilePath = Path.Combine(Global.DataPath, "log.txt");

        /// <summary>
        /// Log the specified text to <see cref="FilePath"/> with the current date/time prepended
        /// </summary>
        /// <param name="text">text to log</param>
        public static void Log(string text)
        {
            try
            {
                Global.CreateDirectoryAndFile(FilePath);

                using (var writer = new StreamWriter(FilePath, true))
                {
                    writer.WriteLine(DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss tt") + ": " + text);
                }
            }
            catch { }
        }

        /// <summary>
        /// Log the specified Exception and sender's type to <see cref="FilePath"/> with the current date/time prepended
        /// </summary>
        /// <param name="sender">Type from which the Exception was thrown</param>
        /// <param name="ex">Exception that was thrown</param>
        /// <param name="message">Optional additional information</param>
        public static void Log(Type sender, Exception ex, string message = "")
        {
            Log($"[{sender.Name} {ex.GetType()} (site: {ex.TargetSite})]: {ex.Message} ({message}) Call stack:\r\n{ex.StackTrace}");
        }
    }
}
