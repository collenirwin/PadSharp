using BoinWPF;
using BoinWPF.Themes;
using ICSharpCode.AvalonEdit.Search;
using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace PadSharp
{
    /// <summary>
    /// Interaction logic for MainView.xaml
    /// </summary>
    public partial class MainView : Window, INotifyPropertyChanged
    {
        #region Vars

        public event PropertyChangedEventHandler PropertyChanged = (sender, e) => { };

        const int FILE_SIZE_LIMIT = 100000;
        const double MAX_FONT_SIZE = 500;

        readonly UISettings settings;
        Theme theme;
        string savedText = "";

        #region Properties

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

        #region Insert

        public UICommand todaysDateCommand { get; private set; }
        public UICommand currentTimeCommand { get; private set; }
        public UICommand dateAndTimeCommand { get; private set; }

        #endregion

        #endregion

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

        int _lineNumber = 1;
        public int lineNumber
        {
            get { return _lineNumber; }
            private set
            {
                if (_lineNumber != value)
                {
                    _lineNumber = value;
                    PropertyChanged(this, new PropertyChangedEventArgs("lineNumber"));
                }
            }
        }

        int _columnNumber = 1;
        public int columnNumber
        {
            get { return _columnNumber; }
            private set
            {
                if (_columnNumber != value)
                {
                    _columnNumber = value;
                    PropertyChanged(this, new PropertyChangedEventArgs("columnNumber"));
                }
            }
        }

        int _wordCount = 0;
        public int wordCount
        {
            get { return _wordCount; }
            private set
            {
                if (_wordCount != value)
                {
                    _wordCount = value;
                    PropertyChanged(this, new PropertyChangedEventArgs("wordCount"));
                }
            }
        }

        public double fontSize
        {
            get { return textbox.FontSize; }
            set
            {
                // make sure fontSize is within acceptable range
                if (value > MAX_FONT_SIZE)
                {
                    textbox.FontSize = MAX_FONT_SIZE;
                }
                else if (value < 1)
                {
                    textbox.FontSize = 1;
                }
                else
                {
                    textbox.FontSize = value;
                }
            }
        }

        #endregion

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

            // insert
            todaysDateCommand = new UICommand(TodaysDate_Command);
            currentTimeCommand = new UICommand(CurrentTime_Command);
            dateAndTimeCommand = new UICommand(DateAndTime_Command);

            #endregion

            InitializeComponent();

            SearchPanel.Install(textbox);
            textbox.TextArea.Caret.PositionChanged += textbox_PositionChanged;

            settings = UISettings.load();

            // if settings couldn't load
            if (settings == null)
            {
                Global.actionMessage("Failed to load settings.json",
                    "settings.json is contained within " +
                    Global.APP_NAME + "'s root directory. A potential fix may be to run " +
                    Global.APP_NAME + " as an administrator.");

                // call constructor for default settings
                settings = new UISettings();
            }

            applySettings(settings);

            populateFontDropdown(fontDropdown);
            selectFont(fontDropdown, textbox.FontFamily);
        }

        #region Settings

        /// <summary>
        /// Set all window properties to match settings fields
        /// </summary>
        private void applySettings(UISettings settings)
        {
            // set theme
            this.theme = settings.theme;
            Application.Current.Resources.
                MergedDictionaries[0].Source = ThemeManager.themeUri(this.theme);

            checkIfSameValue(themeMenu, this.theme == Theme.light ? "Light" : "Dark");

            textbox.FontFamily = settings.fontFamily;
            this.fontSize = settings.fontSize;

            this.WindowState = settings.windowState;

            // only apply size/location settings if not maximized
            if (this.WindowState == WindowState.Normal)
            {
                this.Top = settings.top;
                this.Left = settings.left;
                this.Height = settings.height;
                this.Width = settings.width;
            }

            // check the selected date/time formats
            checkIfSameValue(dateFormatMenu, settings.dateFormat);
            checkIfSameValue(timeFormatMenu, settings.timeFormat);

            // set toggles
            showLineNumbersDropdown.IsChecked = settings.showLineNumbers;
            showStatusBarDropdown.IsChecked = settings.showStatusBar;
            wordWrapDropdown.IsChecked = settings.wordWrap;

            // call toggle events? ...
            showLineNumbers_Checked(null, null);
            showStatusBar_Checked(null, null);
            wordWrap_Checked(null, null);
        }

        /// <summary>
        /// Set all fields in settings object to match window properties
        /// </summary>
        private void setSettings(UISettings settings)
        {
            settings.theme = this.theme;
            settings.fontFamily = textbox.FontFamily;
            settings.fontSize = this.fontSize;
            settings.windowState = this.WindowState;
            settings.top = this.Top;
            settings.left = this.Left;
            settings.height = this.Height;
            settings.width = this.Width;
        }

        #region Font

        private void populateFontDropdown(ComboBox dropdown)
        {
            // sort system fonts
            var fonts = Fonts.SystemFontFamilies.OrderBy(x => x.Source);

            // add 'em all to the dropdown
            foreach (var font in fonts)
            {
                var item = new ComboBoxItem();
                item.FontFamily = font;
                item.Content = font.Source;

                dropdown.Items.Add(item);
            }
        }

        private void selectFont(ComboBox dropdown, FontFamily font)
        {
            foreach (ComboBoxItem item in dropdown.Items)
            {
                if (item.Content.ToString() == font.Source)
                {
                    dropdown.SelectedItem = item;
                    return;
                }
            }

            // default to 0 if we can't find given font
            if (dropdown.Items.Count != 0)
            {
                dropdown.SelectedIndex = 0;
            }
        }

        private void fontDropdown_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // set textbox and dropdown font to the new selected font
            var font = new FontFamily((fontDropdown.SelectedItem as ComboBoxItem).Content.ToString());
            textbox.FontFamily = font;
            fontDropdown.FontFamily = font;
        }

        #endregion

        #endregion

        #region Menu

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
                Alert.showDialog("No file is open.", Global.APP_NAME);
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

        #region Insert

        private void TodaysDate_Command()
        {
            insert(DateTime.Now.ToString(settings.dateFormat));
        }

        private void CurrentTime_Command()
        {
            insert(DateTime.Now.ToString(settings.timeFormat));
        }

        private void DateAndTime_Command()
        {
            insert(DateTime.Now.ToString(settings.dateFormat + " " + settings.timeFormat));
        }

        #endregion

        #region Settings

        private void theme_Checked(object sender, RoutedEventArgs e)
        {
            var item = sender as MenuItem;
            uncheckSiblings(item);

            // select theme
            this.theme = item.Header.ToString() == "Light" ? Theme.light : Theme.dark;

            // set theme for app
            Application.Current.Resources.
                    MergedDictionaries[0].Source = ThemeManager.themeUri(this.theme);
        }

        private void date_Checked(object sender, RoutedEventArgs e)
        {
            var item = sender as MenuItem;
            uncheckSiblings(item);
            settings.dateFormat = item.Header.ToString();
        }

        private void time_Checked(object sender, RoutedEventArgs e)
        {
            var item = sender as MenuItem;
            uncheckSiblings(item);
            settings.timeFormat = item.Header.ToString();
        }

        private void showLineNumbers_Checked(object sender, RoutedEventArgs e)
        {
            textbox.ShowLineNumbers = showLineNumbersDropdown.IsChecked;
            settings.showLineNumbers = showLineNumbersDropdown.IsChecked;
        }

        private void showStatusBar_Checked(object sender, RoutedEventArgs e)
        {
            statusBar.Visibility = showStatusBarDropdown.IsChecked ? Visibility.Visible : Visibility.Collapsed;
            settings.showStatusBar = showStatusBarDropdown.IsChecked;
        }

        private void wordWrap_Checked(object sender, RoutedEventArgs e)
        {
            textbox.WordWrap = wordWrapDropdown.IsChecked;
            settings.wordWrap = wordWrapDropdown.IsChecked;
        }

        #endregion

        #region Help

        private void help_Click(object sender, RoutedEventArgs e)
        {
            var item = sender as MenuItem;

            try
            {
                Process.Start(item.Tag.ToString());
            }
            catch (Exception ex)
            {
                Global.actionMessage("Failed to launch '" + item.Tag.ToString() + "'", ex.Message);
            }
        }

        #endregion

        private void insert(string text)
        {
            // grab position we're going to before we reset textbox.Text
            int position = textbox.CaretOffset + text.Length;

            // insert the text
            textbox.Text = textbox.Text.Insert(textbox.CaretOffset, text);

            // go to the previously calculated position
            textbox.CaretOffset = position;
        }

        private void uncheckSiblings(MenuItem item)
        {
            // if the item is checked, loop through all its parent's children and uncheck them
            if (item.IsChecked)
            {
                var parent = item.Parent as MenuItem;
                foreach (MenuItem child in parent.Items)
                {
                    if (child != item && child != null && child.IsChecked)
                    {
                        child.IsChecked = false;
                    }
                }
            }
        }

        private void checkIfSameValue(MenuItem parent, string value)
        {
            foreach (MenuItem child in parent.Items)
            {
                if (child != null && child.Header.ToString() == value)
                {
                    child.IsChecked = true;
                }
            }
        }

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
                // set all user settings
                setSettings(settings);

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

        private void textbox_PositionChanged(object sender, EventArgs e)
        {
            // set ln and col on caret position change
            lineNumber = textbox.TextArea.Caret.Line;
            columnNumber = textbox.TextArea.Caret.Column;
        }

        private void textbox_TextChanged(object sender, EventArgs e)
        {
            // count the words on textchanged
            wordCount = Regex.Matches(textbox.Text, @"\b\S+\b").Count;
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
            saveDialog.Title = "Save As";
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
