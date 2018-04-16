using System;
using System.IO;

namespace PadSharp
{
    public static class Logger
    {
        public static readonly string FILE_PATH = Path.Combine(Global.DATA_PATH, "log.txt");

        /// <summary>
        /// Log the specified text to Logger.FILE_PATH with the current date/time prepended
        /// </summary>
        /// <param name="text">text to log</param>
        public static void log(string text)
        {
            try
            {
                Global.createDirectoryAndFile(FILE_PATH);

                using (var writer = new StreamWriter(FILE_PATH, true))
                {
                    writer.WriteLine(DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss tt") + ": " + text);
                }
            }
            catch { }
        }

        /// <summary>
        /// Log the specified Exception and sender's type to Logger.FILE_PATH with the current date/time prepended
        /// </summary>
        /// <param name="sender">Type from which the Exception was thrown</param>
        /// <param name="ex">Exception that was thrown</param>
        /// <param name="message">Optional additional information</param>
        public static void log(Type sender, Exception ex, string message = "")
        {
            log(string.Format("[{0} {1} (site: {2})]: {3} ({4})", 
                sender.Name, ex.GetType().ToString(), ex.TargetSite, ex.Message, message));
        }
    }
}
