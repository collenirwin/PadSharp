using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace PadSharp
{
    /// <summary>
    /// Contains logic for loading and searching dictionary.json.
    /// GitHub: https://github.com/adambom/dictionary
    /// </summary>
    public static class LocalDictionary
    {
        public const string FILE_NAME = "dictionary.json";
        public const string FILE_URL = "https://raw.githubusercontent.com/collenirwin/PadSharp/master/dictionary/dictionary.json";

        // c:/users/<user>/appdata/roaming/pad#/dictionary.json
        public static readonly string FULL_PATH = Path.Combine(Global.DATA_PATH, FILE_NAME);

        /// <summary>
        /// Dictionary containing the contents of dictionary.json
        /// </summary>
        public static Dictionary<string, string> dictionary { get; private set; }

        /// <summary>
        /// Has <see cref="dictionary"/> been initialized?
        /// </summary>
        public static bool loaded
        {
            get { return dictionary != null; }
        }

        public static bool loading { get; private set; }

        /// <summary>
        /// Is there a file at <see cref="FULL_PATH"/>?
        /// </summary>
        public static bool downloaded
        {
            get { return File.Exists(FULL_PATH); }
        }

        /// <summary>
        /// Are we in the middle of downloading the file?
        /// </summary>
        public static bool downloading { get; private set; }

        /// <summary>
        /// Attempts to load <see cref="FULL_PATH"/> into <see cref="dictionary"/>
        /// </summary>
        /// <param name="success">Action to run when finished (successful)</param>
        /// <param name="error">Action to run if an error was encountered</param>
        public static async void load(Action success = null, Action<Exception> error = null)
        {
            try
            {
                // avoid collisions
                if (loading)
                {
                    return;
                }

                loading = true;

                // read from FULL_PATH
                string json = File.ReadAllText(FULL_PATH);

                // deserialize json into dictionary
                dictionary = await Task<Dictionary<string, string>>.Run(() => 
                    JsonConvert.DeserializeObject<Dictionary<string, string>>(json));

                loading = false;

                // run success callback
                if (success != null)
                {
                    success();
                }
            }
            catch (Exception ex)
            {
                loading = false;

                // failed - run error callback
                if (error != null)
                {
                    error(ex);
                }
            }
        }

        /// <summary>
        /// Attempts to download dictionary.json from <see cref="FILE_URL"/>
        /// </summary>
        /// <param name="success">Action to run when finished (successful)</param>
        /// <param name="error">Action to run if an error was encountered</param>
        public static async void download(Action success = null, Action<Exception> error = null)
        {
            try
            {
                // avoid collisions
                if (downloading)
                {
                    return;
                }

                downloading = true;

                // make sure we have a folder to write to
                if (!Directory.Exists(Global.DATA_PATH))
                {
                    Directory.CreateDirectory(Global.DATA_PATH);
                }

                using (var client = new WebClient())
                {
                    // fetch file from url
                    await Task.Run(() => client.DownloadFile(new Uri(FILE_URL), FULL_PATH));
                }

                downloading = false;

                // run success callback
                if (success != null)
                {
                    success();
                }
            }
            catch (Exception ex)
            {
                downloading = false;

                // failed - run error callback
                if (error != null)
                {
                    error(ex);
                }
            }
        }

        /// <summary>
        /// Fetch definition for <see cref="word"/> from <see cref="dictionary"/>
        /// </summary>
        /// <param name="word">Word to define</param>
        /// <returns>
        /// definition or null if !<see cref="loaded"/> or if word is not in <see cref="dictionary"/>
        /// </returns>
        public static string define(string word)
        {
            // dictionary not in memory
            if (!loaded)
            {
                return null;
            }

            word = word.ToUpper();

            // see if it's in there
            if (dictionary.ContainsKey(word))
            {
                return dictionary[word]; // found!
            }

            var endings = new string[] { "S", "ED", "ES", "ING", "IES" };

            // it wasn't. let's try taking off common word endings and looking for those variations
            foreach (string ending in endings)
            {
                if (word.EndsWith(ending))
                {
                    // lop that ending off
                    string endingless = word.Substring(0, word.Length - ending.Length);
                    if (dictionary.ContainsKey(endingless))
                    {
                        return dictionary[endingless]; // aha!
                    }
                }
            }

            // no such luck!
            return null;
        }

        /// <summary>
        /// Set <see cref="dictionary"/> to null
        /// </summary>
        public static void flush()
        {
            dictionary = null;
        }
    }
}
