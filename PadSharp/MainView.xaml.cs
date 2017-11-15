using BoinWPF;
using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Diagnostics;
using System.Text;

namespace PadSharp
{
    /// <summary>
    /// Interaction logic for View.xaml
    /// </summary>
    public partial class MainView : Window, INotifyPropertyChanged
    {
        #region Vars

        public event PropertyChangedEventHandler PropertyChanged = (sender, e) => { };

        const int FILE_SIZE_LIMIT = 100000;

        #region UICommands

        #region File

        public UICommand newCommand { get; private set; }
        public UICommand newWindowCommand { get; private set; }
        public UICommand openCommand { get; private set; }
        public UICommand openInExplorerCommand { get; private set; }
        public UICommand saveCommand { get; private set; }
        public UICommand saveAsCommand { get; private set; }
        public UICommand exitCommand { get; private set; }

        #endregion

        #region Edit

        public UICommand undoCommand { get; private set; }
        public UICommand redoCommand { get; private set; }
        public UICommand cutCommand { get; private set; }
        public UICommand copyCommand { get; private set; }
        public UICommand pasteCommand { get; private set; }
        public UICommand findCommand { get; private set; }
        public UICommand findAndReplaceCommand { get; private set; }

        #endregion

        #endregion

        readonly UISettings settings;
        string savedText = "";
        FileInfo _file;

        public FileInfo file
        {
            get { return _file; }
            private set
            {
                if (_file != value)
                {
                    _file = value;
                    PropertyChanged(this, new PropertyChangedEventArgs("file"));
                }
            }
        }

        #endregion

        #region Methods

        public MainView()
        {
            #region Assigning Commands

            // file
            newCommand = new UICommand(New_Command);
            newWindowCommand = new UICommand(NewWindow_Command);
            openCommand = new UICommand(Open_Command);
            openInExplorerCommand = new UICommand(OpenInExplorer_Command);
            saveCommand = new UICommand(Save_Command);
            saveAsCommand = new UICommand(SaveAs_Command);
            exitCommand = new UICommand(Exit_Command);

            // edit
            undoCommand = new UICommand(Undo_Command);
            redoCommand = new UICommand(Redo_Command);
            cutCommand = new UICommand(Cut_Command);
            copyCommand = new UICommand(Copy_Command);
            pasteCommand = new UICommand(Paste_Command);
            findCommand = new UICommand(Find_Command);
            findAndReplaceCommand = new UICommand(FindReplace_Command);

            #endregion

            InitializeComponent();

            settings = UISettings.load();

            // if settings couldn't load
            if (settings == null)
            {
                Global.actionMessage("Failed to load settings.json", 
                    "settings.json is contained within " + 
                    Global.APP_NAME + "'s root directory. A potential fix may be to run " + 
                    Global.APP_NAME + " as an administrator.");
            }

            
        }

        #region Menu Actions

        #region File

        private void New_Command()
        {
            // text not saved
            if (textbox.Text != savedText)
            {
                // are you sure?
                if (Alert.showDialog(
                    "Are you sure you want to make a new file? Your current changes will be lost.",
                    Global.APP_NAME, "Yes", "Cancel") == AlertResult.button1Clicked)
                {
                    // reset all values to null/""
                    file = null;
                    savedText = "";
                    textbox.Text = "";
                }
            }
        }

        private void NewWindow_Command()
        {
            try
            {
                // start another process from the executable
                Process.Start(Process.GetCurrentProcess().MainModule.FileName);
            }
            catch (Exception ex)
            {
                Global.actionMessage("Failed to open a new window", ex.Message);
            }
        }

        private void Open_Command()
        {
            var openDialog = new OpenFileDialog();
            openDialog.Title = "Open";
            openDialog.Multiselect = false;
            openDialog.ShowDialog();

            string path = openDialog.FileName;

            // if a file was selected, save it
            if (path != null && path != "")
            {
                open(path);
            }
        }

        private void OpenInExplorer_Command()
        {
            if (file != null && file.Exists)
            {
                try
                {
                    // open the current file in Windows Explorer
                    Process.Start("explorer.exe", file.FullName);
                }
                catch (Exception ex)
                {
                    Global.actionMessage(
                        "Failed to open the current file in Windows Explorer", ex.Message);
                }
            }
            else
            {
                Alert.showDialog("", Global.APP_NAME);
            }
        }

        private void Save_Command()
        {
            if (file == null)
            {
                saveAs();
            } 
            else
            {
                save(file.FullName);
            }
        }

        private void SaveAs_Command()
        {
            saveAs();
        }

        private void Exit_Command()
        {
            this.Close();
        }

        #endregion

        #region Edit

        private void Undo_Command()
        {
            textbox.Undo();
        }

        private void Redo_Command()
        {
            textbox.Redo();
        }

        private void Cut_Command()
        {
            textbox.Cut();
        }

        private void Copy_Command()
        {
            textbox.Copy();
        }

        private void Paste_Command()
        {
            textbox.Paste();
        }

        private void Find_Command()
        {
            
        }

        private void FindReplace_Command()
        {

        }

        #endregion

        #endregion

        private void window_Closing(object sender, CancelEventArgs e)
        {
            // if the user has not saved their changes, but they wish to
            if (textbox.Text != savedText &&
                Alert.showDialog("Close without saving?",
                Global.APP_NAME, "Close", "Cancel") == AlertResult.button2Clicked)
            {
                e.Cancel = true; // don't close
            }
            else
            {
                // save user settings, show a message if saving fails
                if (settings != null && !settings.save())
                {
                    Global.actionMessage("Failed to save settings.json",
                        "settings.json saves to " +
                        Global.APP_NAME + "'s root directory. A potential fix may be to run " +
                        Global.APP_NAME + " as an administrator.");
                }
            }
        }

        private void open(string path)
        {
            try
            {
                var _file = new FileInfo(path);

                if (_file.Exists)
                {
                    // under limit
                    if (_file.Length < FILE_SIZE_LIMIT)
                    {
                        // read text from path into textbox
                        textbox.Text = File.ReadAllText(path);
                        savedText = textbox.Text;
                        file = _file;
                    }
                    else
                    {
                        Global.actionMessage("Failed to open '" + path + "'", "File is too large.");
                    }
                }
                else
                {
                    Global.actionMessage("Failed to open '" + path + "'", "File does not exist.");
                }
            }
            catch (Exception ex)
            {
                Global.actionMessage("Failed to open '" + path + "'", ex.Message);
            }
        }

        private void save(string path)
        {
            try
            {
                // write textbox text to path
                File.WriteAllText(path, textbox.Text);
                savedText = textbox.Text;
                file = new FileInfo(path);
            }
            catch (Exception ex)
            {
                Global.actionMessage("Failed to save to '" + path + "'", ex.Message);
            }
        }

        private void saveAs()
        {
            var saveDialog = new SaveFileDialog();
            saveDialog.Title = "Save";
            saveDialog.ShowDialog();

            string path = saveDialog.FileName;

            // if a file was selected, save it
            if (path != null && path != "")
            {
                save(path);
            }
        }

        #endregion
    }
}
