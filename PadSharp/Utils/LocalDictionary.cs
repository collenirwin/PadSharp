using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace PadSharp.Utils
{
    /// <summary>
    /// Contains logic for loading and searching dictionary.json.
    /// GitHub: https://github.com/adambom/dictionary
    /// </summary>
    public static class LocalDictionary
    {
        private const string FileName = "dictionary.json";
        private const string FileUrl = "https://raw.githubusercontent.com/collenirwin/PadSharp/master/dictionary/dictionary.json";

        // c:/users/<user>/appdata/roaming/pad#/dictionary.json
        private static readonly string FilePath = Path.Combine(Global.DataPath, FileName);

        /// <summary>
        /// Dictionary containing the contents of dictionary.json
        /// </summary>
        public static Dictionary<string, string> Dictionary { get; private set; }

        /// <summary>
        /// Has <see cref="Dictionary"/> been initialized?
        /// </summary>
        public static bool Loaded => Dictionary != null;

        /// <summary>
        /// Is <see cref="TryLoadAsync"/> running?
        /// </summary>
        public static bool Loading { get; private set; }

        /// <summary>
        /// Is there a file at <see cref="FilePath"/>?
        /// </summary>
        public static bool Downloaded => File.Exists(FilePath);

        /// <summary>
        /// Are we in the middle of downloading the file?
        /// </summary>
        public static bool Downloading { get; private set; }

        /// <summary>
        /// Attempts to load <see cref="FilePath"/> into <see cref="Dictionary"/>
        /// </summary>
        /// <returns>false if already loading, or if an error occured; true if successful</returns>
        public static async Task<bool> TryLoadAsync()
        {
            try
            {
                if (Loading)
                {
                    return false;
                }

                Loading = true;

                // read from local file
                string json = File.ReadAllText(FilePath);

                // deserialize json into dictionary
                Dictionary = await Task.Run(() => 
                    JsonConvert.DeserializeObject<Dictionary<string, string>>(json));

                return true;
            }
            catch (Exception ex)
            {
                Logger.Log(typeof(LocalDictionary), ex, "Loading dictionary");
                return false;
            }
            finally
            {
                Loading = false;
            }
        }

        /// <summary>
        /// Attempts to download dictionary.json from <see cref="FileUrl"/>
        /// </summary>
        public static async Task<bool> TryDownloadAsync()
        {
            try
            {
                if (Downloading)
                {
                    return false;
                }

                Downloading = true;

                // make sure we have a folder to write to
                if (!Directory.Exists(Global.DataPath))
                {
                    Directory.CreateDirectory(Global.DataPath);
                }

                using (var client = new WebClient())
                {
                    // fetch file from url
                    await client.DownloadFileTaskAsync(FileUrl, FilePath);
                }

                return true;
            }
            catch (Exception ex)
            {
                Logger.Log(typeof(LocalDictionary), ex, "Downloading dictionary");
                return false;
            }
            finally
            {
                Downloading = false;
            }
        }

        /// <summary>
        /// Fetch definition for <see cref="word"/> from <see cref="Dictionary"/>
        /// </summary>
        /// <param name="word">Word to define</param>
        /// <returns>
        /// definition or null if !<see cref="Loaded"/> or if word is not in <see cref="Dictionary"/>
        /// </returns>
        public static string Define(string word)
        {
            // dictionary not in memory
            if (!Loaded)
            {
                return null;
            }

            word = word.ToUpper();

            // see if it's in there
            if (Dictionary.ContainsKey(word))
            {
                return Dictionary[word]; // found!
            }

            var endings = new[] { "S", "ED", "ES", "ING", "IES" };

            // it wasn't. let's try taking off common word endings and looking for those variations
            foreach (string ending in endings)
            {
                if (word.EndsWith(ending))
                {
                    // lop that ending off
                    string endingless = word.Substring(0, word.Length - ending.Length);
                    if (Dictionary.ContainsKey(endingless))
                    {
                        return Dictionary[endingless]; // aha!
                    }
                }
            }

            // no such luck!
            return null;
        }

        /// <summary>
        /// Set <see cref="Dictionary"/> to null
        /// </summary>
        public static void Flush()
        {
            Dictionary = null;
        }
    }
}
