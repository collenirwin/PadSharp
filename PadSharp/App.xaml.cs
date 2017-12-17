using BoinWPF;
using System.Diagnostics;
using System.Windows;

namespace PadSharp
{
    public partial class App : Application
    {
        public App()
        {
            // only check for new version if we're not debugging
            if (!Debugger.IsAttached)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    // check for a new version
                    VersionChecker.checkVersion(version =>
                    {
                        var result = Alert.showDialog(
                            string.Format("A new version of {0} is available (version {1}). Would you like to download it?",
                            Global.APP_NAME, version), "Pad#", "Yes", "No");

                        // go to the link to the setup file in the repo
                        if (result == AlertResult.button1Clicked)
                        {
                            Global.launch("https://github.com/collenirwin/PadSharp/blob/master/setup/pad_sharp_setup.exe");
                        }
                    });
                });
            }
        }
    }
}
