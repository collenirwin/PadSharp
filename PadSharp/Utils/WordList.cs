using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace PadSharp.Utils
{
    /// <summary>
    /// Provides functionality for loading and searching the compiled resource 'words.txt' (ENABLE word list)
    /// </summary>
    public static class WordList
    {
        private static readonly HashSet<string> _words = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Case insensitive HashSet of all (most) English words.
        /// If not in memory, it is read in from the compiled resource file.
        /// </summary>
        public static HashSet<string> Words
        {
            get
            {
                if (_words.Count == 0)
                {
                    // read the word list in from words.txt resource
                    using var file = Assembly.GetExecutingAssembly().GetManifestResourceStream("PadSharp.words.txt");
                    using var reader = new StreamReader(file);

                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        _words.Add(line);
                    }
                }

                return _words;
            }
        }

        /// <summary>
        /// Searches for the given word in <see cref="Words"/> (case insensitive)
        /// </summary>
        /// <param name="word">word to look for</param>
        /// <returns>true if found</returns>
        public static bool ContainsWord(string word)
        {
            return Words.Contains(word);
        }
    }
}
