using System;

namespace BoinWPF.Themes
{
    public enum Theme
    {
        Light,
        Dark
    }

    public static class ThemeManager
    {
        /// <summary>
        /// Gets the full <see cref="Uri"/> for the specified <see cref="Theme"/>'s resource file
        /// </summary>
        /// <param name="theme">Theme to lookup</param>
        /// <returns>Uri to theme's resource file</returns>
        public static Uri GetThemeUri(Theme theme)
        {
            if (theme == Theme.Light)
            {
                return new Uri("pack://application:,,,/BoinWPF;component/Themes/Light.xaml");
            }

            return new Uri("pack://application:,,,/BoinWPF;component/Themes/Dark.xaml");
        }
    }
}
