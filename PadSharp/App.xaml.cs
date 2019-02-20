using System;
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
