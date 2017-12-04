using BoinWPF;
using System.Windows;

namespace PadSharp
{
    public partial class App : Application
    {
        public App()
        {
            // check for a new version
            VersionChecker.checkVersion(version =>
            {
                var result = Alert.showDialog(
                    string.Format("A new version of {0} is available (version {1}). Would you like to download it?",
                    Global.APP_NAME, Global.VERSION), "Pad#", "Yes", "No");

                // go to the link to the setup file in the repo
                if (result == AlertResult.button1Clicked)
                {
                    Global.launch("https://github.com/collenirwin/PadSharp/blob/master/setup/pad_sharp_setup.exe");
                }
            });
        }
    }
}
