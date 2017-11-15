using System;
using System.IO;

namespace PadSharp
{
    public static class Logger
    {
        public const string FILE_PATH = "log.txt";

        /// <summary>
        /// Log the specified text to Logger.FILE_PATH with the current date/time prepended
        /// </summary>
        /// <param name="text">text to log</param>
        public static void log(string text)
        {
            try
            {
                using (var writer = new StreamWriter(FILE_PATH, true))
                {
                    writer.WriteLine(DateTime.Now.ToLongTimeString() + ": " + text);
                }
            }
            catch { }
        }

        /// <summary>
        /// Log the specified Exception and sender's type to Logger.FILE_PATH with the current date/time prepended
        /// </summary>
        /// <param name="sender">Type from which the Exception was thrown</param>
        /// <param name="ex">Exception that was thrown</param>
        public static void log(Type sender, Exception ex)
        {
            log(string.Format("[{0} Exception]: {1}", sender.Name, ex.Message));
        }
    }
}
