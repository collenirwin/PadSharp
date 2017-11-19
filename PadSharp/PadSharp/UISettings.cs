using BoinWPF.Themes;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Windows;
using System.Windows.Media;

namespace PadSharp
{
    /// <summary>
    /// Contains fields for user settings and json serialization methods
    /// </summary>
    public class UISettings
    {
        public const string FILE_PATH = "settings.json";

        // colors and fonts
        public Theme theme = Theme.light;
        public FontFamily fontFamily = new FontFamily("Segoe UI");
        public double fontSize = 16;

        // window positioning
        public double top = 300;
        public double left = 300;
        public double height = 350;
        public double width = 525;
        public WindowState windowState = WindowState.Normal;

        // date and time format
        public string dateFormat = "MMMM d yyyy";
        public string timeFormat = "h:mm tt";

        // toggles
        public bool showLineNumbers = true;
        public bool showStatusBar = true;
        public bool wordWrap = true;

        /// <summary>
        /// Creates a UISettings object based on the JSON file at FILE_PATH
        /// </summary>
        /// <returns>null if an exception was thrown</returns>
        public static UISettings load()
        {
            try
            {
                // if our file isn't there, make it
                if (!File.Exists(FILE_PATH))
                {
                    using (var writer = File.CreateText(FILE_PATH))
                    {
                        writer.Write("");
                    }
                }

                // load json object in from FILE_PATH
                string json = File.ReadAllText(FILE_PATH);

                if (json != "")
                {
                    return JsonConvert.DeserializeObject<UISettings>(json);
                }
                else
                {
                    return new UISettings();
                }
            }
            catch (Exception ex)
            {
                Logger.log(typeof(UISettings), ex);
            }

            return null;
        }

        /// <summary>
        /// Serialize UISettings object to JSON and write it to FILE_PATH
        /// </summary>
        /// <returns>false if an exception was thrown</returns>
        public bool save()
        {
            // don't save in the minimized state
            if (windowState == WindowState.Minimized)
            {
                windowState = WindowState.Normal;
            }

            try
            {
                // serialize to JSON, write to FILE_PATH
                using (var writer = File.CreateText(FILE_PATH))
                {
                    writer.Write(JsonConvert.SerializeObject(this));
                }
            }
            catch (Exception ex)
            {
                Logger.log(typeof(UISettings), ex);
                return false;
            }

            return true;
        }
    }
}
