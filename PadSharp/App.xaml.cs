﻿using BoinWPF;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Threading;

namespace PadSharp
{
    public partial class App : Application
    {
        public App()
        {
            // register uncaught exception handler
            DispatcherUnhandledException += App_DispatcherUnhandledException;

            // only check for new version if we're not debugging
            if (!Debugger.IsAttached)
            {
                Current.Dispatcher.Invoke(() =>
                {
                    // check for a new version
                    VersionChecker.CheckVersion((version) =>
                    {
                        var result = Alert
                            .showDialog($"A new version of {Global.AppName} is available (version {version}). Would you like to download it?",
                            title: "Pad#", button1Text: "Yes", button2Text: "No");

                        // go to the link to the setup file in the repo
                        if (result == AlertResult.button1Clicked)
                        {
                            Global.Launch("https://github.com/collenirwin/PadSharp/blob/master/setup/pad_sharp_setup.exe");
                        }
                    },
                    (ex) =>
                    {
                        Logger.Log(typeof(App), ex, "Checking Version");
                    });
                });
            }
        }

        private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            // log the exception, noting it was unhandled
            Logger.Log(typeof(App), e.Exception, "Unhandled Exception");
            
            // attempt to recover the open file
            try
            {
                string fileText = (MainWindow as MainView).textbox.Text;

                // get a unique file name by hashing the file text and the current time
                string fileName = Crypto.Hash(fileText, DateTime.Now.Ticks.ToString()) + ".txt";

                // %appdata%\Pad#\recovery\<file_name>
                string path = Path.Combine(Path.Combine(Global.DataPath, "recovery"), fileName);

                // create and write to the file
                Global.CreateDirectoryAndFile(path);
                File.WriteAllText(path, fileText);

                // let the user know where to find the file
                Global.ActionMessage("Pad# has experienced a fatal error. There has been an attempt to recover your file. " +
                    "Please click More for the path to the recovered file.", path);
            }
            catch (Exception ex)
            {
                Logger.Log(typeof(App), ex, "Recovering file");
            }
        }
    }
}
