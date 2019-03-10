using PadSharp.Utils;
using System.ComponentModel;
using System.Windows;
using System.Windows.Documents;

namespace PadSharp
{
    /// <summary>
    /// Interaction logic for AboutWindow.xaml
    /// </summary>
    public partial class AboutWindow : Window
    {
        private static AboutWindow _instance;

        /// <summary>
        /// Singleton instance of <see cref="AboutWindow"/>
        /// </summary>
        public static AboutWindow Instance
        {
            get => _instance ?? (_instance = new AboutWindow());
        }

        private AboutWindow()
        {
            InitializeComponent();

            VersionDisplay.Text = $"Version {VersionChecker.Version}";

            // show the new version button if there's a newer version available on GitHub
            if (VersionChecker.NewVersion != null && VersionChecker.NewVersion != VersionChecker.Version)
            {
                NewVersionButton.Visibility = Visibility.Visible;
            }
        }

        private void Hyperlink_Click(object sender, RoutedEventArgs e)
        {
            Global.Launch((sender as Hyperlink).NavigateUri.ToString());
        }

        private void NewVersionButton_Click(object sender, RoutedEventArgs e)
        {
            Global.Launch("https://github.com/collenirwin/PadSharp/blob/master/setup/pad_sharp_setup.exe");
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            // make sure our singleton instance is null so that this window is re-instantiated next time
            _instance = null;
        }
    }
}
