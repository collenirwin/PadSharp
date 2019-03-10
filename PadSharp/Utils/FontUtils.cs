using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Media;

namespace PadSharp.Utils
{
    public static class FontUtils
    {
        /// <summary>
        /// Gets all of the sytem's fonts and adds them to the passed <see cref="ObservableCollection"/>
        /// </summary>
        /// <param name="fontFamilies">Collection of font families</param>
        public static void PopulateFontCollection(ObservableCollection<FontFamily> fontFamilies)
        {
            // grab and sort system fonts
            var fonts = Fonts.SystemFontFamilies.OrderBy(x => x.Source);

            // add 'em all to the observable collection
            foreach (var font in fonts)
            {
                fontFamilies.Add(font);
            }
        }
    }
}
