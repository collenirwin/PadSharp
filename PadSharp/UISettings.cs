using BoinWPF.Themes;
using Newtonsoft.Json;
using PadSharp.Utils;
using System;
using System.IO;
using System.Windows;
using System.Windows.Media;

namespace PadSharp
{
    /// <summary>
    /// Contains properties for user settings and json serialization methods
    /// </summary>
    public class UISettings
    {
        /// <summary>
        /// Name of the settings file
        /// </summary>
        public const string FileName = "settings.json";

        /// <summary>
        /// Path to the settings file:
        /// c:/users/<user>/appdata/roaming/pad#/<see cref="FileName"/>
        /// </summary>
        public static readonly string FilePath = Path.Combine(Global.DataPath, FileName);

        /// <summary>
        /// The default UI settings
        /// </summary>
        public static UISettings Default { get; } = new UISettings();

        #region JSON Properties

        #region Colors and fonts

        [JsonProperty("theme")]
        public Theme Theme { get; set; } = Theme.Light;

        [JsonProperty("fontFamily")]
        public FontFamily FontFamily { get; set; } = new FontFamily("Segoe UI");

        [JsonProperty("fontSize")]
        public double FontSize { get; set; } = 16;

        #endregion

        #region Window positioning

        [JsonProperty("top")]
        public double Top { get; set; } = 300;

        [JsonProperty("left")]
        public double Left { get; set; } = 300;

        [JsonProperty("height")]
        public double Height { get; set; } = 350;

        [JsonProperty("width")]
        public double Width { get; set; } = 525;

        [JsonProperty("windowState")]
        public WindowState WindowState { get; set; } = WindowState.Normal;

        #endregion

        #region Date and time format

        [JsonProperty("dateFormat")]
        public string DateFormat { get; set; } = "MMMM d, yyyy";

        [JsonProperty("timeFormat")]
        public string TimeFormat { get; set; } = "h:mm tt";

        #endregion

        #region Toggles

        [JsonProperty("showLineNumbers")]
        public bool ShowLineNumbers { get; set; } = true;

        [JsonProperty("showStatusBar")]
        public bool ShowStatusBar { get; set; } = true;

        [JsonProperty("wordWrap")]
        public bool WordWrap { get; set; } = true;

        [JsonProperty("topmost")]
        public bool Topmost { get; set; } = false;

        #region Status bar visibilities

        [JsonProperty("lineNumberVisible")]
        public bool LineNumberVisible { get; set; } = true;

        [JsonProperty("columnNumberVisible")]
        public bool ColumnNumberVisible { get; set; } = true;

        [JsonProperty("wordCountVisible")]
        public bool WordCountVisible { get; set; } = true;

        [JsonProperty("charCountVisible")]
        public bool CharCountVisible { get; set; } = false;

        #endregion

        #endregion

        #endregion

        /// <summary>
        /// Creates a UISettings object based on the JSON file at <see cref="FilePath"/>
        /// </summary>
        /// <returns>null if an exception was thrown</returns>
        public static UISettings Load()
        {
            try
            {
                // if our file isn't there, make it
                if (!File.Exists(FilePath))
                {
                    Global.CreateDirectoryAndFile(FilePath);
                }

                // load json object in from FILE_PATH
                string json = File.ReadAllText(FilePath);

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
                Logger.Log(typeof(UISettings), ex, "loading");
            }

            return null;
        }

        /// <summary>
        /// Serialize UISettings object to JSON and write it to <see cref="FilePath"/>
        /// </summary>
        /// <returns>false if an exception was thrown</returns>
        public bool Save()
        {
            // don't save in the minimized state
            if (WindowState == WindowState.Minimized)
            {
                WindowState = WindowState.Normal;
            }

            try
            {
                // if our file isn't there, make it
                if (!File.Exists(FilePath))
                {
                    Global.CreateDirectoryAndFile(FilePath);
                }

                // serialize to JSON, write to FULL_PATH
                using var writer = File.CreateText(FilePath);
                writer.Write(JsonConvert.SerializeObject(this));
            }
            catch (Exception ex)
            {
                Logger.Log(typeof(UISettings), ex, "saving");
                return false;
            }

            return true;
        }
    }
}
