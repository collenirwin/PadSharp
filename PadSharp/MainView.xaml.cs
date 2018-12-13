using BoinWPF;
using BoinWPF.Themes;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using ICSharpCode.AvalonEdit.Utils;
using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml;

namespace PadSharp
{
    /// <summary>
    /// Interaction logic for MainView.xaml
    /// </summary>
    public partial class MainView : Window, INotifyPropertyChanged
    {
        #region Fields, Properties

        #region Fields

        // required to implement (from INotifyPropertyChanged)
        public event PropertyChangedEventHandler PropertyChanged = (sender, e) => { };

        // 5m file size max
        const int FILE_SIZE_LIMIT = 5000000;

        // 500px font size max
        const double MAX_FONT_SIZE = 500;

        // our marker for highlighting a checked line
        const string CHECK_MARK = "✔";

        // title: Pad# <version #>
        const string TITLE = Global.APP_NAME + " " + Global.VERSION;

        // settings object (contains all user settings)
        readonly UISettings settings;

        // Theme we're using
        Theme theme;

        // text from currently open file
        string savedText = "";

        // should we prompt the user to reload the file if it has been modified
        bool promptForReload = false;

        #endregion

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
        public UICommand checkSpellingCommand { get; private set; }
        public UICommand normalizeLineEndingdCommand { get; private set; }
        public UICommand selectAllCommand { get; private set; }

        #endregion

        #region Insert

        public UICommand checkMarkCommand { get; private set; }
        public UICommand addCheckMarkCommand { get; private set; }
        public UICommand todaysDateCommand { get; private set; }
        public UICommand currentTimeCommand { get; private set; }
        public UICommand dateAndTimeCommand { get; private set; }

        #endregion

        #region Selection

        public UICommand boldCommand { get; private set; }
        public UICommand italicCommand { get; private set; }
        public UICommand underlineCommand { get; private set; }
        public UICommand lowerCaseCommand { get; private set; }
        public UICommand upperCaseCommand { get; private set; }
        public UICommand titleCaseCommand { get; private set; }
        public UICommand toggleCaseCommand { get; private set; }
        public UICommand defineCommand { get; private set; }
        public UICommand reverseCommand { get; private set; }
        public UICommandWithParam sortCommand { get; private set; }

        #endregion

        #region Settings

        public UICommand toggleLineNumbersCommand { get; private set; }
        public UICommand toggleStatusBarCommand { get; private set; }
        public UICommand toggleWordWrapCommand { get; private set; }
        public UICommand toggleTopmostCommand { get; private set; }

        #endregion

        #endregion

        #region Properties with Associated Fields

        /// <summary>
        /// Goes in the title bar of the app. Format: [filename?] - TITLE
        /// </summary>
        public string title
        {
            get
            {
                return file != null ? file.Name + " - " + TITLE : TITLE;
            }
            private set
            {
                // just force the UI to update
                PropertyChanged(this, new PropertyChangedEventArgs("title"));
            }
        }

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
                    title = title; // force title to update
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
            private set
            {
                // hack? update the UI while keeping this essentially readonly
                PropertyChanged(this, new PropertyChangedEventArgs("fileSaved"));
            }
        }

        /// <summary>
        /// Hides or shows statusBar
        /// </summary>
        public bool statusBarVisible
        {
            get
            {
                return statusBar != null
                    ? statusBar.Visibility == Visibility.Visible 
                    : false;
            }
            set
            {
                if (statusBar == null)
                {
                    return;
                }

                statusBar.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
                PropertyChanged(this, new PropertyChangedEventArgs("statusBarVisible"));
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

        bool _lineNumberVisible;

        /// <summary>
        /// Show the line number in the status bar?
        /// </summary>
        public bool lineNumberVisible
        {
            get { return _lineNumberVisible; }
            private set
            {
                if (_lineNumberVisible != value)
                {
                    _lineNumberVisible = value;
                    PropertyChanged(this, new PropertyChangedEventArgs("lineNumberVisible"));
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

        bool _columnNumberVisible;

        /// <summary>
        /// Show the column number in the status bar?
        /// </summary>
        public bool columnNumberVisible
        {
            get { return _columnNumberVisible; }
            private set
            {
                if (_columnNumberVisible != value)
                {
                    _columnNumberVisible = value;
                    PropertyChanged(this, new PropertyChangedEventArgs("columnNumberVisible"));
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

        bool _wordCountVisible;

        /// <summary>
        /// Show the word count in the status bar?
        /// </summary>
        public bool wordCountVisible
        {
            get { return _wordCountVisible; }
            private set
            {
                if (_wordCountVisible != value)
                {
                    _wordCountVisible = value;
                    PropertyChanged(this, new PropertyChangedEventArgs("wordCountVisible"));
                }
            }
        }

        bool _charCountVisible;

        /// <summary>
        /// Show the char count in the status bar?
        /// </summary>
        public bool charCountVisible
        {
            get { return _charCountVisible; }
            private set
            {
                if (_charCountVisible != value)
                {
                    _charCountVisible = value;
                    PropertyChanged(this, new PropertyChangedEventArgs("charCountVisible"));
                }
            }
        }

        /// <summary>
        /// Font size of the textbox
        /// </summary>
        public double fontSize
        {
            get { return textbox.FontSize; }
            private set
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

        /// <summary>
        /// Show the Selection Menu whenever text is selected
        /// </summary>
        public Visibility selectionMenuVisibility
        {
            get
            {
                return textbox != null && textbox.SelectionLength > 0
                    ? Visibility.Visible
                    : Visibility.Collapsed;
            }
            private set
            {
                // just force the UI to update
                PropertyChanged(this,
                    new PropertyChangedEventArgs("selectionMenuVisibility"));
            }
        }

        #endregion

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
            checkSpellingCommand = new UICommand(CheckSpelling_Command);
            normalizeLineEndingdCommand = new UICommand(NormalizeLineEndings_Command);
            selectAllCommand = new UICommand(SelectAll_Command);

            // insert
            checkMarkCommand = new UICommand(CheckMark_Command);
            addCheckMarkCommand = new UICommand(AddCheckMark_Command);
            todaysDateCommand = new UICommand(TodaysDate_Command);
            currentTimeCommand = new UICommand(CurrentTime_Command);
            dateAndTimeCommand = new UICommand(DateAndTime_Command);

            // selection
            boldCommand = new UICommand(Bold_Command);
            italicCommand = new UICommand(Italic_Command);
            underlineCommand = new UICommand(Underline_Command);
            lowerCaseCommand = new UICommand(LowerCase_Command);
            upperCaseCommand = new UICommand(UpperCase_Command);
            titleCaseCommand = new UICommand(TitleCase_Command);
            toggleCaseCommand = new UICommand(ToggleCase_Command);
            defineCommand = new UICommand(Define_Command);
            reverseCommand = new UICommand(Reverse_Command);
            sortCommand = new UICommandWithParam(Sort_Command);

            // settings
            toggleLineNumbersCommand = new UICommand(ToggleLineNumbers_Command);
            toggleStatusBarCommand = new UICommand(ToggleStatusBar_Command);
            toggleWordWrapCommand = new UICommand(ToggleWordWrap_Command);
            toggleTopmostCommand = new UICommand(ToggleTopmost_Command);

            #endregion

            // get rid of indent selection keybind
            AvalonEditCommands.IndentSelection.InputGestures.Clear();

            InitializeComponent();

            var args = Environment.GetCommandLineArgs();

            // if we have a command line argument
            if (args.Length > 1 && args[1].Length > 0)
            {
                // assume it is a path to a file, try to open the file
                open(args[1]);

                // couldn't open the file, shutdown
                if (file == null)
                {
                    Application.Current.Shutdown();
                }
            }

            // set hyperlink color
            textbox.TextArea.TextView.LinkTextForegroundBrush = SystemColors.HighlightBrush;

            // register PositionChanged event
            textbox.TextArea.Caret.PositionChanged += textbox_PositionChanged;

            // register SelectionChanged event
            textbox.TextArea.SelectionChanged += textArea_SelectionChanged;

            // load the syntax highlighting xml from our resource file
            using (var file = Assembly.GetExecutingAssembly().GetManifestResourceStream("PadSharp.Syntax.xshd"))
            using (var reader = new XmlTextReader(file))
            {
                textbox.SyntaxHighlighting = HighlightingLoader.Load(reader, HighlightingManager.Instance);
            }

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
            showStatusBarDropdown.IsChecked = settings.showStatusBar;
            textbox.WordWrap = settings.wordWrap;
            textbox.ShowLineNumbers = settings.showLineNumbers;
            this.Topmost = settings.topmost;

            // set status bar visibilities
            lineNumberVisible = settings.lineNumberVisible;
            columnNumberVisible = settings.columnNumberVisible;
            wordCountVisible = settings.wordCountVisible;
            charCountVisible = settings.charCountVisible;
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

            settings.showLineNumbers = textbox.ShowLineNumbers;
            settings.showStatusBar = statusBar.Visibility == Visibility.Visible;
            settings.wordWrap = textbox.WordWrap;
            settings.topmost = this.Topmost;

            settings.lineNumberVisible = lineNumberVisible;
            settings.columnNumberVisible = columnNumberVisible;
            settings.wordCountVisible = wordCountVisible;
            settings.charCountVisible = charCountVisible;
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
            // make sure the new window has the same settings as this one
            setSettings(settings);
            settings.save();

            // make the new window and show it
            var newWindow = new MainView();

            // make sure there's no file open because there will be if there is a path in the command line args
            newWindow.file = null;
            newWindow.textbox.Text = "";
            newWindow.Show();
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

        private void CheckSpelling_Command()
        {
            // words are 3 or more letters, no numbers/symbols other than '
            var words = Regex.Matches(textbox.Text, @"\b([a-z]|'){3,}\b", RegexOptions.IgnoreCase);

            bool mistakes = false;
            foreach (Match word in words)
            {
                if (!WordList.containsWord(word.Value))
                {
                    mistakes = true; // spelling mistakes were made!

                    // highlight undefined word
                    textbox.SelectionLength = 0; // needed so we don't go out of range
                    textbox.SelectionStart = word.Index;
                    textbox.SelectionLength = word.Length;

                    // scroll to the undefined word
                    var location = textbox.Document.GetLocation(word.Index);
                    textbox.ScrollTo(location.Line, location.Column);

                    var result = Alert.showDialog(
                        string.Format("'{0}' isn't in Pad#'s dictionary.", word.Value),
                        Global.APP_NAME, "Next", "Stop Spelling Check");

                    if (result == AlertResult.button2Clicked)
                    {
                        // user clicked "Stop Spelling Check"
                        return;
                    }
                }
            }

            // change our message depending on whether we found spelling mistakes
            string message = mistakes
                ? "No other spelling mistakes were found"
                : "No spelling mistakes were found";

            Global.actionMessage(message, "Note: some words may not be in Pad#'s dictionary. " +
                "In order for a word to be checked it must be at least 3 letters long, " +
                "and it must not contain any numbers or symbols (other than apostrophes (')).");
        }

        private void NormalizeLineEndings_Command()
        {
            // set all line endings to \r\n
            textbox.normalizeLineEndings(true);

            // give the user some feedback - this change won't be obvious
            Alert.showDialog(@"Done. All line endings are now \r\n.", Global.APP_NAME);
        }

        private void SelectAll_Command()
        {
            textbox.SelectAll();
        }

        #endregion

        #region Insert

        private void CheckMark_Command()
        {
            // split the textbox text into lines
            var lines = textbox.Text.Replace("\r", "").Split('\n');

            // get the line we're currently on
            int lineIndex = textbox.TextArea.Caret.Line - 1;
            string line = lines[lineIndex];

            bool hasCheck = line.Length > 0 && line.Substring(0, 1) == CHECK_MARK;

            if (!hasCheck)
            {
                // throw a CHECK_MARK and a space at the beginning of the line we're on
                lines[lineIndex] = CHECK_MARK + " " + line;
            }
            else
            {
                // remove the CHECK_MARK (and the space if it's there) from beginning of the line we're on
                lines[lineIndex] = line.Remove(0, line.Length > 1 && line.Substring(1, 1) == " " ? 2 : 1);
            }

            // throw our edited lines back into the textbox
            textbox.Document.Text = string.Join("\r\n", lines);
            textbox.TextArea.Caret.Line = lineIndex + 1;
            textbox.TextArea.Caret.Column = 0;
        }

        private void AddCheckMark_Command()
        {
            insert(CHECK_MARK);
        }

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

        #region Selection

        private void Bold_Command()
        {
            toggleFontStyle("**");
        }

        private void Italic_Command()
        {
            toggleFontStyle("*");
        }

        private void Underline_Command()
        {
            toggleFontStyle("__");
        }

        private void LowerCase_Command()
        {
            textbox.replaceSelectedText(textbox.SelectedText.ToLower());
        }

        private void UpperCase_Command()
        {
            textbox.replaceSelectedText(textbox.SelectedText.ToUpper());
        }

        private void TitleCase_Command()
        {
            textbox.replaceSelectedText(textbox.SelectedText.titleCase());
        }

        private void ToggleCase_Command()
        {
            textbox.replaceSelectedText(textbox.SelectedText.toggleCase());
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
                    // download, load, showDefinition
                    downloadDictionary(() => showDefinition(text));
                }
            }
            else
            {
                Alert.showDialog("Please select a word to define.", Global.APP_NAME);
            }
        }

        private void Reverse_Command()
        {
            textbox.replaceSelectedText(textbox.SelectedText.reverseLines());
        }

        private void Sort_Command(object descending)
        {
            textbox.replaceSelectedText(textbox.SelectedText.sortLines((bool)descending));
        }

        #region Helpers

        private void toggleFontStyle(string marker)
        {
            string text = textbox.SelectedText;

            // toggle marker for all lines
            if (text.Contains('\n'))
            {
                textbox.replaceSelectedText(text.toggleLineStart(marker + " "));
            }
            else // toggle bold within line
            {
                textbox.replaceSelectedText(text.toggleStartAndEnd(marker + " ", " " + marker));
            }
        }

        #endregion

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

        private void ToggleLineNumbers_Command()
        {
            // toggle the IsChecked of the MenuItem
            showLineNumbersDropdown.IsChecked = !showLineNumbersDropdown.IsChecked;
            textbox.ShowLineNumbers = showLineNumbersDropdown.IsChecked;
        }

        private void ToggleStatusBar_Command()
        {
            // toggle the visibility of the status bar
            statusBarVisible = !statusBarVisible;
        }

        private void ToggleWordWrap_Command()
        {
            // toggle the IsChecked of the MenuItem
            wordWrapDropdown.IsChecked = !wordWrapDropdown.IsChecked;
            textbox.WordWrap = wordWrapDropdown.IsChecked;
        }

        private void ToggleTopmost_Command()
        {
            // toggle the IsChecked of the MenuItem
            topmostDrowndown.IsChecked = !topmostDrowndown.IsChecked;
            this.Topmost = topmostDrowndown.IsChecked;
        }

        #endregion

        #region Help

        private void fontStyleGuide_Click(object sender, RoutedEventArgs e)
        {
            var fontStyleFile = Path.Combine(Global.DATA_PATH, "font-style-guide.txt");

            // file isn't there yet. See if we can create it.
            if (!File.Exists(fontStyleFile))
            {
                try
                {
                    Global.createDirectoryAndFile(fontStyleFile);
                    File.WriteAllText(fontStyleFile, Properties.Resources.font_style_guide);
                }
                catch (Exception ex)
                {
                    Global.actionMessage(
                        "Cannot access your APPDATA folder, which is where the font style guide is located.", ex.Message);
                    return;
                }
            }

            // open the file in the user's default text editor
            Global.launch(fontStyleFile);
        }

        private void help_Click(object sender, RoutedEventArgs e)
        {
            Global.launch((sender as MenuItem).Tag.ToString());
        }

        #endregion
        void dog() { }
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
        /// Calls LocalDictionary.load with a default error callback
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
        /// Calls LocalDictionary.download, then loadDictionary with default error callbacks
        /// </summary>
        /// <param name="callback">Success callback</param>
        private void downloadDictionary(Action callback)
        {
            // download, load, call callback
            LocalDictionary.download(() =>
            {
                loadDictionary(() =>
                {
                    callback();
                });
            }, (ex) => // failed
            {
                Global.actionMessage("Failed to download the dictionary", ex.Message);
            });

            Alert.showDialog(
                "The dictionary is now downloading for the first time. This may take a moment.",
                Global.APP_NAME);
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
            textbox.Document.Text = textbox.Document.Text.Insert(textbox.CaretOffset, text);

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

        /// <summary>
        /// Calls textbox.findNext() with the appropriate arguments based on the ui.
        /// loopback should be true if searching backwards from the start.
        /// Updates lblMatches.Text with the number of matches found using the regex in txtFind.Text.
        /// </summary>
        /// <param name="start">Where to start searching</param>
        /// <param name="lookback">Look back from this point?</param>
        private void findHelper(int start, bool lookback = false)
        {
            bool _matchCase = matchCase.IsChecked == true;

            bool found = textbox.findNext(txtFind.Text, 
                start, _matchCase, lookback);

            setFoundForeground(found);

            // if there's nothing in txtFind, don't count the matches
            if (txtFind.Text == "")
            {
                lblMatches.Text = "-";
                return;
            }

            // update lblMatches with the number of matches (async)
            textbox.Document.Text.countMatchesAsync(txtFind.Text, _matchCase, (count) =>
            {
                lblMatches.Text = count.ToString();
            },
            (ex) =>
            {
                lblMatches.Text = "-";
            });
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
            bool replaced = textbox.replaceNext(txtFind.Text, txtReplace.Text, 
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
            textbox.replaceAll(txtFind.Text, txtReplace.Text, 
                matchCase.IsChecked == true, (count) =>
            {
                // this determines whether or not to run the replace
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

        private void window_Activated(object sender, EventArgs e)
        {
            // check to see if the open file has been modified
            if (file != null && File.Exists(file.FullName) && promptForReload)
            {
                try
                {
                    string fileText = File.ReadAllText(file.FullName);

                    // file has been modified
                    if (fileText != savedText)
                    {
                        promptForReload = false;

                        // set savedText to the new text from the file
                        savedText = fileText;
                        fileSaved = fileSaved;

                        // want to reload it?
                        var result = Alert.showDialog(
                            string.Format("\"{0}\" has been modified by another program. Would you like to reload it?", file.Name),
                            Global.APP_NAME, "Reload from file", "Keep my changes");

                        // yes i do
                        if (result == AlertResult.button1Clicked)
                        {
                            open(file.FullName);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.log(typeof(MainView), ex, "window_Activated");
                }
            }
        }

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

        private void textArea_SelectionChanged(object sender, EventArgs e)
        {
            // force UI to acknowledge selectionMenuVisibility (we're not actually affecting it)
            selectionMenuVisibility = Visibility.Visible;
        }

        #endregion

        #region IO helpers

        /// <summary>
        /// Attempts to open the file specified by path in the editor.
        /// Gives appropriate error prompts.
        /// Also sets this.file to the newly opened file's FileInfo.
        /// </summary>
        /// <param name="path">Path to the desired file</param>
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

                        // prompt the user if the file changes
                        promptForReload = true;
                    }
                    else
                    {
                        Global.actionMessage("Failed to open '" + path + "'", 
                            string.Format("File is too large (must be under {0}k).", FILE_SIZE_LIMIT / 1000));
                    }
                }
                else
                {
                    Global.actionMessage("Failed to open '" + path + "'", "File does not exist.");
                }
            }
            catch (Exception ex)
            {
                string message = "Failed to open '" + path + "'";
                Global.actionMessage(message, ex.Message);
                Logger.log(typeof(MainView), ex, message);
            }
        }

        /// <summary>
        /// Attempts to save the file specified by path.
        /// Gives an appropriate error message if it can't save.
        /// Also sets this.file to the (possibly updated) file's FileInfo.
        /// </summary>
        /// <param name="path">Path to the desired file</param>
        private void save(string path)
        {
            try
            {
                // write textbox text to path
                File.WriteAllText(path, textbox.Text);
                savedText = textbox.Text;
                file = new FileInfo(path);

                // prompt the user if the file changes
                promptForReload = true;
            }
            catch (Exception ex)
            {
                string message = "Failed to save to '" + path + "'";
                Global.actionMessage(message, ex.Message);
                Logger.log(typeof(MainView), ex, message);
            }
        }

        /// <summary>
        /// Opens a SaveFileDialog prompting the user to select a place to save,
        /// then calls save with the selected path.
        /// </summary>
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
