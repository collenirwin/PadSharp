using BoinWPF;
using BoinWPF.Themes;
using ICSharpCode.AvalonEdit.Utils;
using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace PadSharp
{
    /// <summary>
    /// Interaction logic for MainView.xaml
    /// </summary>
    public partial class MainView : Window, INotifyPropertyChanged
    {
        #region Vars

        // required to implement (from INotifyPropertyChanged)
        public event PropertyChangedEventHandler PropertyChanged = (sender, e) => { };

        // 100k file size max
        const int FILE_SIZE_LIMIT = 100000;

        // 500px font size max
        const double MAX_FONT_SIZE = 500;

        // settings object (contains all user settings)
        readonly UISettings settings;

        // Theme we're using
        Theme theme;

        // text from currently open file
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
        public UICommand printCommand { get; private set; }
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
        public UICommand gotoCommand { get; private set; }
        public UICommand gotoGoCommand { get; private set; }
        public UICommand normalizeLineEndingdCommand { get; private set; }
        public UICommand defineCommand { get; private set; }
        public UICommand sortCommand { get; private set; }
        public UICommand selectAllCommand { get; private set; }

        #endregion

        #region Insert

        public UICommand todaysDateCommand { get; private set; }
        public UICommand currentTimeCommand { get; private set; }
        public UICommand dateAndTimeCommand { get; private set; }

        #endregion

        #endregion

        FileInfo _file;

        /// <summary>
        /// Currently open file
        /// </summary>
        public FileInfo file
        {
            get { return _file; }
            private set
            {
                if (_file != value)
                {
                    _file = value;
                    PropertyChanged(this, new PropertyChangedEventArgs("file"));
                    fileSaved = true;
                }
            }
        }

        /// <summary>
        /// Is the file saved?
        /// </summary>
        public bool fileSaved
        {
            get
            {
                return file != null && textbox.Text == savedText;
            }
            set
            {
                // hack? update the UI while keeping this essentially readonly
                PropertyChanged(this, new PropertyChangedEventArgs("fileSaved"));
            }
        }

        int _lineNumber = 1;

        /// <summary>
        /// Line number the caret is on
        /// </summary>
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

        /// <summary>
        /// Column number the caret is on
        /// </summary>
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

        /// <summary>
        /// Number of words in textbox (found using \b\S+\b)
        /// </summary>
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

        /// <summary>
        /// Font size of the textbox
        /// </summary>
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

        #region Constructor

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
            printCommand = new UICommand(Print_Command);
            exitCommand = new UICommand(Exit_Command);

            // edit
            undoCommand = new UICommand(Undo_Command);
            redoCommand = new UICommand(Redo_Command);
            cutCommand = new UICommand(Cut_Command);
            copyCommand = new UICommand(Copy_Command);
            pasteCommand = new UICommand(Paste_Command);
            findCommand = new UICommand(Find_Command);
            findAndReplaceCommand = new UICommand(FindReplace_Command);
            gotoCommand = new UICommand(Goto_Command);
            gotoGoCommand = new UICommand(GotoGo_Command);
            normalizeLineEndingdCommand = new UICommand(NormalizeLineEndings_Command);
            defineCommand = new UICommand(Define_Command);
            sortCommand = new UICommand(Sort_Command);
            selectAllCommand = new UICommand(SelectAll_Command);

            // insert
            todaysDateCommand = new UICommand(TodaysDate_Command);
            currentTimeCommand = new UICommand(CurrentTime_Command);
            dateAndTimeCommand = new UICommand(DateAndTime_Command);

            #endregion

            InitializeComponent();

            // titlebar: Pad# <version #>
            this.Title = Global.APP_NAME + " " + Global.VERSION;

            var args = Environment.GetCommandLineArgs();

            // if we have a command line argument
            if (args.Length > 1 && args[1].Length > 0)
            {
                // assume it is a path to a file, try to open the file
                open(args[1]);
            }

            // set hyperlink color
            textbox.TextArea.TextView.LinkTextForegroundBrush = SystemColors.HighlightBrush;

            // register PositionChanged event
            textbox.TextArea.Caret.PositionChanged += textbox_PositionChanged;

            settings = UISettings.load();

            // if settings couldn't load
            if (settings == null)
            {
                Global.actionMessage("Failed to load settings.json.", 
                    "A potential fix may be to run " +
                    Global.APP_NAME + " as an administrator.");

                // call constructor for default settings
                settings = new UISettings();
            }

            // apply settings to UI
            applySettings(settings);

            // set font, size
            populateFontDropdown(fontDropdown);
            selectFont(fontDropdown, textbox.FontFamily);
        }

        #endregion

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

            // font
            textbox.FontFamily = settings.fontFamily;
            this.fontSize = settings.fontSize;
            fontSizeDropdown.Text = this.fontSize.ToString();

            this.WindowState = settings.windowState;

            // only apply size settings if not maximized
            if (this.WindowState == WindowState.Normal)
            {
                this.Height = settings.height;
                this.Width = settings.width;
            }

            // only set our location if it's within the bounds of the user's screen(s)
            if (settings.top >= SystemParameters.VirtualScreenTop && 
                settings.left >= SystemParameters.VirtualScreenLeft &&
                settings.top <= (SystemParameters.VirtualScreenHeight - Math.Abs(SystemParameters.VirtualScreenTop) - this.Height) &&
                settings.left <= (SystemParameters.VirtualScreenWidth - Math.Abs(SystemParameters.VirtualScreenLeft) - this.Width))
            {
                this.Top = settings.top;
                this.Left = settings.left;
            }
            

            // check the selected date/time formats
            checkIfSameValue(dateFormatMenu, settings.dateFormat);
            checkIfSameValue(timeFormatMenu, settings.timeFormat);

            // set toggles
            showLineNumbersDropdown.IsChecked = settings.showLineNumbers;
            showStatusBarDropdown.IsChecked = settings.showStatusBar;
            wordWrapDropdown.IsChecked = settings.wordWrap;
            topmostDrowndown.IsChecked = settings.topmost;

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

        #endregion

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
            try
            {
                // set textbox and dropdown font to the new selected font
                var font = new FontFamily((fontDropdown.SelectedItem as ComboBoxItem).Content.ToString());
                textbox.FontFamily = font;
                fontDropdown.FontFamily = font;
            }
            catch { /* go with the last best matched font */ }
        }

        private void fontSizeDropdown_Changed(object sender, EventArgs e)
        {
            double size;
            if (double.TryParse(fontSizeDropdown.Text, out size))
            {
                fontSize = size;
                fontSizeDropdown.Text = fontSize.ToString();
            }
            else
            {
                fontSizeDropdown.Text = fontSize.ToString();
            }
        }

        #endregion

        #region Menu

        #region File

        private void New_Command()
        {
            // text not saved, promt them: are you sure?
            if (textbox.Text != savedText && Alert.showDialog(
                "Are you sure you want to make a new file? Your unsaved changes to the current file will be lost.",
                Global.APP_NAME, "Yes", "Cancel") != AlertResult.button1Clicked)
            {
                return;
            }

            // reset all values to null/""
            file = null;
            savedText = "";
            textbox.Text = "";
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
            // text not saved, promt them: are you sure?
            if (textbox.Text != savedText && Alert.showDialog(
                "Are you sure you want to open a file? Your unsaved changes to the current file will be lost.",
                Global.APP_NAME, "Yes", "Cancel") != AlertResult.button1Clicked)
            {
                return;
            }

            var openDialog = new OpenFileDialog();
            openDialog.Title = "Open";
            openDialog.DefaultExt = ".txt";
            openDialog.Filter = "Text Files|*.txt|All Files|*.*";
            openDialog.Multiselect = false;
            var result = openDialog.ShowDialog();

            string path = openDialog.FileName;

            // if a file was selected, save it
            if (result == true && path != null && path != "")
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
                    // open the current file's directory in Windows Explorer
                    Process.Start("explorer.exe", file.DirectoryName);
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

        private void Print_Command()
        {
            var printDialog = new PrintDialog();

            if (printDialog.ShowDialog() == true)
            {
                // get a FlowDocument from our editor
                var flowDoc = DocumentPrinter.CreateFlowDocumentForEditor(textbox);

                printDialog.PrintDocument(
                    // get document paginator from flowdoc
                    ((IDocumentPaginatorSource)flowDoc).DocumentPaginator,

                    // description
                    "Pad#: " + lblFileName.Text);
            }
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
            // close goto
            gotoPanel.Visibility = Visibility.Collapsed;

            // find open, replace closed
            findPanelParent.Visibility = Visibility.Visible;
            replacePanelParent.Visibility = Visibility.Collapsed;
            openFindHelper();
        }

        private void FindReplace_Command()
        {
            // close goto
            gotoPanel.Visibility = Visibility.Collapsed;

            // both find and replace open
            findPanelParent.Visibility = Visibility.Visible;
            replacePanelParent.Visibility = Visibility.Visible;
            openFindHelper();
        }

        private void Goto_Command()
        {
            // close find and replace
            findPanelParent.Visibility = Visibility.Collapsed;
            replacePanelParent.Visibility = Visibility.Collapsed;

            // open goto
            gotoPanel.Visibility = Visibility.Visible;
            txtGoto.Focus();
            txtGoto.SelectAll();
        }

        private void NormalizeLineEndings_Command()
        {
            // set all line endings to /r/n
            TextEditorUtils.normalizeLineEndings(textbox, true);

            // give the user some feedback - this change won't be obvious
            Alert.showDialog(@"Done. All line endings are now \r\n.", Global.APP_NAME);
        }

        private void Define_Command()
        {
            string text = textbox.SelectedText.Trim();

            if (text != "")
            {
                // we have a file
                if (LocalDictionary.downloaded)
                {
                    // it's in memory
                    if (LocalDictionary.loaded)
                    {
                        showDefinition(text);
                    }
                    else // load file, showDefinition
                    {
                        loadDictionary(() => showDefinition(text));
                    }
                }
                else // no file
                {
                    // donwload, load, showDefinition
                    LocalDictionary.download(() =>
                    {
                        loadDictionary(() =>
                        {
                            showDefinition(text);
                        });
                    }, (ex) => // failed
                    {
                        Global.actionMessage("Failed to download the dictionary", ex.Message);
                    });

                    Alert.showDialog(
                        "The dictionary is now downloading for the first time. This may take a moment.",
                        Global.APP_NAME);
                }
            }
            else
            {
                Alert.showDialog("Please select a word to define.", Global.APP_NAME);
            }
        }

        private void Sort_Command()
        {
            TextEditorUtils.sortLines(textbox, textbox.SelectedText);
        }

        private void SelectAll_Command()
        {
            textbox.SelectAll();
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

            // select theme
            this.theme = item.Header.ToString() == "Light" ? Theme.light : Theme.dark;

            // uncheck other themes
            uncheckSiblings(item);

            // set theme for app
            Application.Current.Resources.
                    MergedDictionaries[0].Source = ThemeManager.themeUri(this.theme);
        }

        private void theme_Unchecked(object sender, RoutedEventArgs e)
        {
            var item = sender as MenuItem;

            // if this setting is selected
            if (this.theme.ToString() == item.Header.ToString().ToLower())
            {
                keepMenuItemChecked(item, theme_Checked);
            }
        }

        private void date_Checked(object sender, RoutedEventArgs e)
        {
            var item = sender as MenuItem;
            settings.dateFormat = item.Header.ToString();
            uncheckSiblings(item);
        }

        private void date_Unchecked(object sender, RoutedEventArgs e)
        {
            var item = sender as MenuItem;

            // if this setting is selected
            if (settings.dateFormat == item.Header.ToString())
            {
                keepMenuItemChecked(item, date_Checked);
            }
        }

        private void time_Checked(object sender, RoutedEventArgs e)
        {
            var item = sender as MenuItem;
            settings.timeFormat = item.Header.ToString();
            uncheckSiblings(item);
        }

        private void time_Unchecked(object sender, RoutedEventArgs e)
        {
            var item = sender as MenuItem;

            // if this setting is selected
            if (settings.timeFormat == item.Header.ToString())
            {
                keepMenuItemChecked(item, time_Checked);
            }
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

        private void topmost_Checked(object sender, RoutedEventArgs e)
        {
            settings.topmost = topmostDrowndown.IsChecked;
            this.Topmost = topmostDrowndown.IsChecked;
        }

        #endregion

        #region Help

        private void help_Click(object sender, RoutedEventArgs e)
        {
            Global.launch((sender as MenuItem).Tag.ToString());
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Call LocalDictionary.define with <see cref="word"/>,
        /// show an Alert dialog with the definition (or couldn't find ...)
        /// </summary>
        /// <param name="word">Word to define</param>
        private void showDefinition(string word)
        {
            string definition = LocalDictionary.define(word);

            if (definition != null)
            {
                Alert.showDialog(word + ": " + definition, Global.APP_NAME);
            }
            else
            {
                Alert.showDialog("Couldn't find a definition for '" + word + "'", Global.APP_NAME);
            }
        }

        /// <summary>
        /// Calls LocalDictionary.Load with a default error callback
        /// </summary>
        /// <param name="callback">Success callback</param>
        private void loadDictionary(Action callback)
        {
            LocalDictionary.load(callback, (ex) =>
            {
                Global.actionMessage("Failed to read from the dictionary", ex.Message);
            });
        }

        /// <summary>
        /// Insert the specified text into <see cref="textbox"/>,
        /// then move the caret to the end of the inserted text
        /// </summary>
        /// <param name="text">Text to insert</param>
        private void insert(string text)
        {
            // grab position we're going to before we reset textbox.Text
            int position = textbox.CaretOffset + text.Length;

            // insert the text
            textbox.Text = textbox.Text.Insert(textbox.CaretOffset, text);

            // go to the previously calculated position
            textbox.CaretOffset = position;
        }

        /// <summary>
        /// Unchecks all of the sibling MenuItems of provided <see cref="item"/>
        /// </summary>
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

        /// <summary>
        /// Check all child MenuItems whose header == provided <see cref="value"/>
        /// </summary>
        /// <param name="parent">Parent MenuItem</param>
        /// <param name="value">Header value</param>
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

        /// <summary>
        /// put selected text into find box, select it
        /// </summary>
        private void openFindHelper()
        {
            // put selected text into find box
            if (textbox.SelectionLength > 0)
            {
                txtFind.Text = textbox.SelectedText;
            }

            txtFind.Focus();
            txtFind.SelectAll();
        }

        /// <summary>
        /// Keeps the passed <see cref="MenuItem"/> checked but doesn't trigger its Checked event
        /// </summary>
        /// <param name="item">MenuItem to keep checked</param>
        /// <param name="handler">item's Checked event handler</param>
        private void keepMenuItemChecked(MenuItem item, RoutedEventHandler handler)
        {
            item.Checked -= handler;
            item.IsChecked = true;
            item.Checked += handler;
        }

        #endregion

        #endregion

        #region Find + Replace + Goto

        #region Find

        /// <summary>
        /// Sets the Foreground of <see cref="txtFind"/>
        /// to its normal color or red based on <see cref="wasFound"/>
        /// </summary>
        /// <param name="wasFound">Was the text found within the textbox?</param>
        private void setFoundForeground(bool wasFound)
        {
            if (wasFound) // found
            {
                // normal text color
                txtFind.Foreground = (Brush)FindResource("foreColorDark");
            }
            else // not found
            {
                // red text color
                txtFind.Foreground = Brushes.LightSalmon;
            }
        }

        private bool findHelper(int start, bool lookback = false)
        {
            bool found = TextEditorUtils.findNext(textbox, txtFind.Text, 
                start, matchCase.IsChecked == true, lookback);

            setFoundForeground(found);

            return found;
        }

        private void txtFind_TextChanged(object sender, RoutedEventArgs e)
        {
            // find first instance of text entered
            findHelper(0);
        }

        private void findUp_Click(object sender, RoutedEventArgs e)
        {
            // look back from selection
            findHelper(textbox.SelectionStart, true);
        }

        private void findDown_Click(object sender, RoutedEventArgs e)
        {
            // look forward from selection
            findHelper(textbox.SelectionStart + textbox.SelectionLength);
        }

        private void closeFindReplace_Click(object sender, RoutedEventArgs e)
        {
            // close find and replace
            findPanelParent.Visibility = Visibility.Collapsed;
            replacePanelParent.Visibility = Visibility.Collapsed;
        }

        #endregion

        #region Replace

        private bool replaceHelper()
        {
            bool replaced = TextEditorUtils.replaceNext(textbox, txtFind.Text, txtReplace.Text, 
                textbox.SelectionStart + textbox.SelectionLength, matchCase.IsChecked == true);

            setFoundForeground(replaced);

            return replaced;
        }

        private void replaceNext_Click(object sender, RoutedEventArgs e)
        {
            replaceHelper();
        }

        private void replaceAll_Click(object sender, RoutedEventArgs e)
        {
            TextEditorUtils.replaceAll(textbox, txtFind.Text, txtReplace.Text, 
                matchCase.IsChecked == true, (count) =>
            {
                return count > 0 && Alert.showDialog(
                    string.Format("Replace {0} instances of {1} with {2}?", count, txtFind.Text, txtReplace.Text), 
                    Global.APP_NAME, "OK", "Cancel") == AlertResult.button1Clicked;
            });
        }

        #endregion

        #region Goto

        private void GotoGo_Command()
        {
            int line;
            if (int.TryParse(txtGoto.Text, out line))
            {
                textbox.ScrollTo(line, 0);
            }
            else
            {
                Alert.showDialog("Line number must be an integer.", Global.APP_NAME);
                txtGoto.Focus();
                txtGoto.SelectAll();
            }
        }

        private void goto_Click(object sender, RoutedEventArgs e)
        {
            GotoGo_Command();
        }

        private void closeGoto_Click(object sender, RoutedEventArgs e)
        {
            gotoPanel.Visibility = Visibility.Collapsed;
        }

        #endregion

        #endregion

        #region Window Events

        private void window_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            // Ctrl+Scroll
            if ((Keyboard.Modifiers & ModifierKeys.Control) != 0)
            {
                int delta = 1;
                if (e.Delta < 0)
                {
                    delta = -1;
                }
                
                // increase/decrease the font size based on whether we're scrolling up or down
                fontSize += delta;
                fontSizeDropdown.Text = fontSize.ToString();
            }
        }

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
                        "A potential fix may be to run " +
                        Global.APP_NAME + " as an administrator.");
                }
            }
        }

        #endregion

        #region Main Textbox Events

        private void textbox_GotFocus(object sender, EventArgs e)
        {
            // make sure the font dropdown always displays the correct font
            fontDropdown.Text = textbox.FontFamily.Source;
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

            // force UI to acknowledge fileSaved
            fileSaved = false;
        }

        #endregion

        #region IO helpers

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
            saveDialog.FileName = fileSaved ? file.Name : "document.txt";
            saveDialog.Title = "Save As";
            saveDialog.DefaultExt = ".txt";
            saveDialog.Filter = "Text Files|*.txt|All Files|*.*";
            saveDialog.AddExtension = true;
            var result = saveDialog.ShowDialog();

            string path = saveDialog.FileName;

            // if a file was selected, save it
            if (result == true && path != null && path != "")
            {
                save(path);
            }
        }

        #endregion

        #endregion
    }
}
