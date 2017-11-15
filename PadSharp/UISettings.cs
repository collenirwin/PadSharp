using Newtonsoft.Json;
using System;
using System.IO;
using System.Windows;
using System.Windows.Media;

namespace PadSharp
{
    public enum Theme
    {
        dark,
        light
    }

    public class UISettings
    {
        public const string FILE_PATH = "settings.json";

        // colors and fonts
        public Theme theme = Theme.light;
        public FontFamily fontFamily = new FontFamily("Segoe UI");
        public double fontSize = 16;
        public Color editorFontColor = Colors.Black;

        // window positioning
        public int top = 300;
        public int left = 300;
        public int height = 350;
        public int width = 525;
        public WindowState windowState = WindowState.Normal;

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
