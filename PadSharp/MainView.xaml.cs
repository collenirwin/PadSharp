﻿using BoinWPF;
using BoinWPF.Themes;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using ICSharpCode.AvalonEdit.Utils;
using Microsoft.Win32;
using RegularExtensions;
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

        // required to implement INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged = (sender, e) => { };

        // 5m file size max
        private const int _fileSizeLimit = 5_000_000;

        // 500px font size max
        private const double _maxFontSize = 500;

        // our marker for highlighting a checked line
        private const string _checkMark = "✔";

        // title: Pad# <version #>
        private const string _title = Global.AppName + " " + Global.Version;

        // settings object (contains all user settings)
        private readonly UISettings _settings;

        // Theme we're using
        private Theme _theme;

        // text from currently open file
        private string _savedText = "";

        // should we prompt the user to reload the file if it has been modified
        private bool _promptForReload = false;

        #endregion

        #region Properties

        #region UICommands

        #region File

        public UICommand NewCommand { get; private set; }
        public UICommand NewWindowCommand { get; private set; }
        public UICommand OpenCommand { get; private set; }
        public UICommand OpenInExplorerCommand { get; private set; }
        public UICommand SaveCommand { get; private set; }
        public UICommand SaveAsCommand { get; private set; }
        public UICommand PrintCommand { get; private set; }
        public UICommand ExitCommand { get; private set; }

        #endregion

        #region Edit

        public UICommand UndoCommand { get; private set; }
        public UICommand RedoCommand { get; private set; }
        public UICommand CutCommand { get; private set; }
        public UICommand CopyCommand { get; private set; }
        public UICommand PasteCommand { get; private set; }
        public UICommand FindCommand { get; private set; }
        public UICommand FindAndReplaceCommand { get; private set; }
        public UICommand GotoCommand { get; private set; }
        public UICommand GotoGoCommand { get; private set; }
        public UICommand CheckSpellingCommand { get; private set; }
        public UICommand NormalizeLineEndingdCommand { get; private set; }
        public UICommand SelectAllCommand { get; private set; }

        #endregion

        #region Insert

        public UICommand CheckMarkCommand { get; private set; }
        public UICommand AddCheckMarkCommand { get; private set; }
        public UICommand TodaysDateCommand { get; private set; }
        public UICommand CurrentTimeCommand { get; private set; }
        public UICommand DateAndTimeCommand { get; private set; }

        #endregion

        #region Selection

        public UICommand BoldCommand { get; private set; }
        public UICommand ItalicCommand { get; private set; }
        public UICommand UnderlineCommand { get; private set; }
        public UICommand LowerCaseCommand { get; private set; }
        public UICommand UpperCaseCommand { get; private set; }
        public UICommand TitleCaseCommand { get; private set; }
        public UICommand ToggleCaseCommand { get; private set; }
        public UICommand DefineCommand { get; private set; }
        public UICommand ReverseCommand { get; private set; }
        public UICommandWithParam SortCommand { get; private set; }

        #endregion

        #region Settings

        public UICommand ToggleLineNumbersCommand { get; private set; }
        public UICommand ToggleStatusBarCommand { get; private set; }
        public UICommand ToggleWordWrapCommand { get; private set; }
        public UICommand ToggleTopmostCommand { get; private set; }

        #endregion

        #endregion

        #region Properties with Associated Fields

        /// <summary>
        /// Goes in the title bar of the app. Format: [filename?] - TITLE
        /// </summary>
        public string FullTitle
        {
            get
            {
                return OpenFile != null ? OpenFile.Name + " - " + _title : _title;
            }
            set
            {
                // just force the UI to update
                PropertyChanged(this, new PropertyChangedEventArgs(nameof(FullTitle)));
            }
        }

        private FileInfo _openFile;

        /// <summary>
        /// Currently open file
        /// </summary>
        public FileInfo OpenFile
        {
            get { return _openFile; }
            set
            {
                if (_openFile != value)
                {
                    _openFile = value;
                    PropertyChanged(this, new PropertyChangedEventArgs(nameof(OpenFile)));
                    FileSaved = true;
                    FullTitle = FullTitle; // force title to update
                }
            }
        }

        /// <summary>
        /// Is the file saved?
        /// </summary>
        public bool FileSaved
        {
            get
            {
                return OpenFile != null && textbox.Text == _savedText;
            }
            set
            {
                // hack? update the UI while keeping this essentially readonly
                PropertyChanged(this, new PropertyChangedEventArgs(nameof(FileSaved)));
            }
        }

        /// <summary>
        /// Hides or shows statusBar
        /// </summary>
        public bool StatusBarVisible
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
                PropertyChanged(this, new PropertyChangedEventArgs(nameof(StatusBarVisible)));
            }
        }

        private int _lineNumber = 1;

        /// <summary>
        /// Line number the caret is on
        /// </summary>
        public int LineNumber
        {
            get { return _lineNumber; }
            set
            {
                if (_lineNumber != value)
                {
                    _lineNumber = value;
                    PropertyChanged(this, new PropertyChangedEventArgs(nameof(LineNumber)));
                }
            }
        }

        private bool _lineNumberVisible;

        /// <summary>
        /// Show the line number in the status bar?
        /// </summary>
        public bool LineNumberVisible
        {
            get { return _lineNumberVisible; }
            set
            {
                if (_lineNumberVisible != value)
                {
                    _lineNumberVisible = value;
                    PropertyChanged(this, new PropertyChangedEventArgs(nameof(LineNumberVisible)));
                }
            }
        }

        private int _columnNumber = 1;

        /// <summary>
        /// Column number the caret is on
        /// </summary>
        public int ColumnNumber
        {
            get { return _columnNumber; }
            set
            {
                if (_columnNumber != value)
                {
                    _columnNumber = value;
                    PropertyChanged(this, new PropertyChangedEventArgs(nameof(ColumnNumber)));
                }
            }
        }

        private bool _columnNumberVisible;

        /// <summary>
        /// Show the column number in the status bar?
        /// </summary>
        public bool ColumnNumberVisible
        {
            get { return _columnNumberVisible; }
            set
            {
                if (_columnNumberVisible != value)
                {
                    _columnNumberVisible = value;
                    PropertyChanged(this, new PropertyChangedEventArgs(nameof(ColumnNumberVisible)));
                }
            }
        }

        private int _wordCount = 0;

        /// <summary>
        /// Number of words in textbox (found using \b\S+\b)
        /// </summary>
        public int WordCount
        {
            get { return _wordCount; }
            set
            {
                if (_wordCount != value)
                {
                    _wordCount = value;
                    PropertyChanged(this, new PropertyChangedEventArgs(nameof(WordCount)));
                }
            }
        }

        private bool _wordCountVisible;

        /// <summary>
        /// Show the word count in the status bar?
        /// </summary>
        public bool WordCountVisible
        {
            get { return _wordCountVisible; }
            set
            {
                if (_wordCountVisible != value)
                {
                    _wordCountVisible = value;
                    PropertyChanged(this, new PropertyChangedEventArgs(nameof(WordCountVisible)));
                }
            }
        }

        private bool _charCountVisible;

        /// <summary>
        /// Show the char count in the status bar?
        /// </summary>
        public bool CharCountVisible
        {
            get { return _charCountVisible; }
            set
            {
                if (_charCountVisible != value)
                {
                    _charCountVisible = value;
                    PropertyChanged(this, new PropertyChangedEventArgs(nameof(CharCountVisible)));
                }
            }
        }

        /// <summary>
        /// Font size of the textbox
        /// </summary>
        public new double FontSize
        {
            get { return textbox.FontSize; }
            set
            {
                // make sure fontSize is within acceptable range
                if (value > _maxFontSize)
                {
                    textbox.FontSize = _maxFontSize;
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
        public Visibility SelectionMenuVisibility
        {
            get
            {
                return textbox != null && textbox.SelectionLength > 0
                    ? Visibility.Visible
                    : Visibility.Collapsed;
            }
            set
            {
                // just force the UI to update
                PropertyChanged(this,
                    new PropertyChangedEventArgs(nameof(SelectionMenuVisibility)));
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
            NewCommand = new UICommand(New_Command);
            NewWindowCommand = new UICommand(NewWindow_Command);
            OpenCommand = new UICommand(Open_Command);
            OpenInExplorerCommand = new UICommand(OpenInExplorer_Command);
            SaveCommand = new UICommand(Save_Command);
            SaveAsCommand = new UICommand(SaveAs_Command);
            PrintCommand = new UICommand(Print_Command);
            ExitCommand = new UICommand(Exit_Command);

            // edit
            UndoCommand = new UICommand(Undo_Command);
            RedoCommand = new UICommand(Redo_Command);
            CutCommand = new UICommand(Cut_Command);
            CopyCommand = new UICommand(Copy_Command);
            PasteCommand = new UICommand(Paste_Command);
            FindCommand = new UICommand(Find_Command);
            FindAndReplaceCommand = new UICommand(FindReplace_Command);
            GotoCommand = new UICommand(Goto_Command);
            GotoGoCommand = new UICommand(GotoGo_Command);
            CheckSpellingCommand = new UICommand(CheckSpelling_Command);
            NormalizeLineEndingdCommand = new UICommand(NormalizeLineEndings_Command);
            SelectAllCommand = new UICommand(SelectAll_Command);

            // insert
            CheckMarkCommand = new UICommand(CheckMark_Command);
            AddCheckMarkCommand = new UICommand(AddCheckMark_Command);
            TodaysDateCommand = new UICommand(TodaysDate_Command);
            CurrentTimeCommand = new UICommand(CurrentTime_Command);
            DateAndTimeCommand = new UICommand(DateAndTime_Command);

            // selection
            BoldCommand = new UICommand(Bold_Command);
            ItalicCommand = new UICommand(Italic_Command);
            UnderlineCommand = new UICommand(Underline_Command);
            LowerCaseCommand = new UICommand(LowerCase_Command);
            UpperCaseCommand = new UICommand(UpperCase_Command);
            TitleCaseCommand = new UICommand(TitleCase_Command);
            ToggleCaseCommand = new UICommand(ToggleCase_Command);
            DefineCommand = new UICommand(Define_Command);
            ReverseCommand = new UICommand(Reverse_Command);
            SortCommand = new UICommandWithParam(Sort_Command);

            // settings
            ToggleLineNumbersCommand = new UICommand(ToggleLineNumbers_Command);
            ToggleStatusBarCommand = new UICommand(ToggleStatusBar_Command);
            ToggleWordWrapCommand = new UICommand(ToggleWordWrap_Command);
            ToggleTopmostCommand = new UICommand(ToggleTopmost_Command);

            #endregion

            // get rid of indent selection keybind
            AvalonEditCommands.IndentSelection.InputGestures.Clear();

            InitializeComponent();

            var args = Environment.GetCommandLineArgs();

            // if we have a command line argument
            if (args.Length > 1 && args[1].Length > 0)
            {
                // assume it is a path to a file, try to open the file
                Open(args[1]);

                // couldn't open the file, shutdown
                if (OpenFile == null)
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

            _settings = UISettings.Load();

            // if settings couldn't load
            if (_settings == null)
            {
                Global.ActionMessage("Failed to load settings.json.", 
                    "A potential fix may be to run " +
                    Global.AppName + " as an administrator.");

                // call constructor for default settings
                _settings = new UISettings();
            }

            // apply settings to UI
            ApplySettings(_settings);

            // set font, size
            PopulateFontDropdown(fontDropdown);
            SelectFont(fontDropdown, textbox.FontFamily);
        }

        #endregion

        #region Settings

        /// <summary>
        /// Set all window properties to match settings fields
        /// </summary>
        private void ApplySettings(UISettings settings)
        {
            // set theme
            _theme = settings.Theme;
            Application.Current.Resources.
                MergedDictionaries[0].Source = ThemeManager.themeUri(this._theme);

            CheckIfSameValue(themeMenu, _theme == Theme.light ? "Light" : "Dark");

            // font
            textbox.FontFamily = settings.FontFamily;
            FontSize = settings.FontSize;
            fontSizeDropdown.Text = FontSize.ToString();

            WindowState = settings.WindowState;

            // only apply size settings if not maximized
            if (WindowState == WindowState.Normal)
            {
                Height = settings.Height;
                Width = settings.Width;
            }

            // only set our location if it's within the bounds of the user's screen(s)
            if (settings.Top >= SystemParameters.VirtualScreenTop && 
                settings.Left >= SystemParameters.VirtualScreenLeft &&
                settings.Top <= (SystemParameters.VirtualScreenHeight - Math.Abs(SystemParameters.VirtualScreenTop) - Height) &&
                settings.Left <= (SystemParameters.VirtualScreenWidth - Math.Abs(SystemParameters.VirtualScreenLeft) - Width))
            {
                Top = settings.Top;
                Left = settings.Left;
            }
            

            // check the selected date/time formats
            CheckIfSameValue(dateFormatMenu, settings.DateFormat);
            CheckIfSameValue(timeFormatMenu, settings.TimeFormat);

            // set toggles
            showStatusBarDropdown.IsChecked = settings.ShowStatusBar;
            textbox.WordWrap = settings.WordWrap;
            textbox.ShowLineNumbers = settings.ShowLineNumbers;
            Topmost = settings.Topmost;

            // set status bar visibilities
            LineNumberVisible = settings.LineNumberVisible;
            ColumnNumberVisible = settings.ColumnNumberVisible;
            WordCountVisible = settings.WordCountVisible;
            CharCountVisible = settings.CharCountVisible;
        }

        /// <summary>
        /// Set all fields in settings object to match window properties
        /// </summary>
        private void SetSettings(UISettings settings)
        {
            settings.Theme = _theme;
            settings.FontFamily = textbox.FontFamily;
            settings.FontSize = FontSize;
            settings.WindowState = WindowState;

            settings.Top = Top;
            settings.Left = Left;
            settings.Height = Height;
            settings.Width = Width;

            settings.ShowLineNumbers = textbox.ShowLineNumbers;
            settings.ShowStatusBar = statusBar.Visibility == Visibility.Visible;
            settings.WordWrap = textbox.WordWrap;
            settings.Topmost = Topmost;

            settings.LineNumberVisible = LineNumberVisible;
            settings.ColumnNumberVisible = ColumnNumberVisible;
            settings.WordCountVisible = WordCountVisible;
            settings.CharCountVisible = CharCountVisible;
        }

        #endregion

        #region Font

        /// <summary>
        /// Grab all of the system fonts, sort them by name, and add them to a <see cref="ComboBox"/>
        /// </summary>
        /// <param name="dropdown"><see cref="ComboBox"/> to add the font family names to</param>
        private void PopulateFontDropdown(ComboBox dropdown)
        {
            // sort system fonts
            var fonts = Fonts.SystemFontFamilies.OrderBy(x => x.Source);

            // add 'em all to the dropdown
            foreach (var font in fonts)
            {
                var item = new ComboBoxItem
                {
                    FontFamily = font,
                    Content = font.Source
                };

                dropdown.Items.Add(item);
            }
        }

        /// <summary>
        /// Selects the given font in the given <see cref="ComboBox"/>. Defaults to the first item.
        /// </summary>
        /// <param name="dropdown"><see cref="ComboBox"/> to select the font within</param>
        /// <param name="font">font to select</param>
        private void SelectFont(ComboBox dropdown, FontFamily font)
        {
            foreach (ComboBoxItem item in dropdown.Items)
            {
                if (item.Content.ToString() == font.Source)
                {
                    dropdown.SelectedItem = item;
                    return;
                }
            }

            // default to the first font if we can't find given font
            if (dropdown.Items.Count != 0)
            {
                dropdown.SelectedIndex = 0;
            }
        }

        /// <summary>
        /// handles changing the textbox font via the dropdown
        /// </summary>
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

        /// <summary>
        /// Handles changing the textbox font size via the dropdown
        /// </summary>
        private void fontSizeDropdown_Changed(object sender, EventArgs e)
        {
            if (double.TryParse(fontSizeDropdown.Text, out double size))
            {
                FontSize = size;
                fontSizeDropdown.Text = FontSize.ToString();
            }
            else
            {
                fontSizeDropdown.Text = FontSize.ToString();
            }
        }

        #endregion

        #region Menu

        #region File

        private void New_Command()
        {
            // text not saved, promt them: are you sure?
            if (textbox.Text != _savedText && Alert.showDialog(
                "Are you sure you want to make a new file? Your unsaved changes to the current file will be lost.",
                Global.AppName, "Yes", "Cancel") != AlertResult.button1Clicked)
            {
                return;
            }

            // reset all values to null/""
            OpenFile = null;
            _savedText = "";
            textbox.Text = "";
        }

        private void NewWindow_Command()
        {
            // make sure the new window has the same settings as this one
            SetSettings(_settings);
            _settings.Save();

            try
            {
                // start another instance via the exe
                Process.Start(GetType().Assembly.Location);
            }
            catch (Exception ex)
            {
                Global.ActionMessage($"Failed to open another instance of {Global.AppName}.", ex.Message);
                Logger.Log(GetType(), ex, "opening a new window");
            }
        }

        private void Open_Command()
        {
            // text not saved, promt them: are you sure?
            if (textbox.Text != _savedText && Alert.showDialog(
                "Are you sure you want to open a file? Your unsaved changes to the current file will be lost.",
                Global.AppName, "Yes", "Cancel") != AlertResult.button1Clicked)
            {
                return;
            }

            var openDialog = new OpenFileDialog
            {
                Title = "Open",
                DefaultExt = ".txt",
                Filter = "Text Files|*.txt|All Files|*.*",
                Multiselect = false
            };

            var result = openDialog.ShowDialog();

            string path = openDialog.FileName;

            // if a file was selected, save it
            if (result == true && path != null && path != "")
            {
                Open(path);
            }
        }

        private void OpenInExplorer_Command()
        {
            if (OpenFile != null && OpenFile.Exists)
            {
                try
                {
                    // open the current file's directory in Windows Explorer
                    Process.Start("explorer.exe", OpenFile.DirectoryName);
                }
                catch (Exception ex)
                {
                    Global.ActionMessage("Failed to open the current file in Windows Explorer", ex.Message);
                }
            }
            else
            {
                Alert.showDialog("No file is open.", Global.AppName);
            }
        }

        private void Save_Command()
        {
            if (OpenFile == null)
            {
                saveAs();
            } 
            else
            {
                Save(OpenFile.FullName);
            }
        }

        private void SaveAs_Command()
        {
            saveAs();
        }

        private void Print_Command()
        {
            var printDialog = new PrintDialog();

            // user pressed 'ok' to print
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
            Close();
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
            OpenFindHelper();
        }

        private void FindReplace_Command()
        {
            // close goto
            gotoPanel.Visibility = Visibility.Collapsed;

            // both find and replace open
            findPanelParent.Visibility = Visibility.Visible;
            replacePanelParent.Visibility = Visibility.Visible;
            OpenFindHelper();
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
            var words = textbox.Text.Matches(@"\b([a-z]|'){3,}\b", RegexOptions.IgnoreCase);

            bool mistakes = false;
            foreach (Match word in words)
            {
                if (!WordList.ContainsWord(word.Value))
                {
                    mistakes = true; // spelling mistakes were made!

                    // highlight undefined word
                    textbox.SelectionLength = 0; // needed so we don't go out of range
                    textbox.SelectionStart = word.Index;
                    textbox.SelectionLength = word.Length;

                    // scroll to the undefined word
                    var location = textbox.Document.GetLocation(word.Index);
                    textbox.ScrollTo(location.Line, location.Column);

                    var result = Alert.showDialog($"'{word.Value}' isn't in Pad#'s dictionary.",
                        title: Global.AppName, button1Text: "Next", button2Text: "Stop Spelling Check");

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

            Global.ActionMessage(message, "Note: some words may not be in Pad#'s dictionary. " +
                "In order for a word to be checked it must be at least 3 letters long, " +
                "and it must not contain any numbers or symbols (other than apostrophes (')).");
        }

        private void NormalizeLineEndings_Command()
        {
            // set all line endings to \r\n
            textbox.NormalizeLineEndings(true);

            // give the user some feedback - this change won't be obvious
            Alert.showDialog("Done. All line endings have been converted to the Windows format (CRLF).", Global.AppName);
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

            bool hasCheck = line.Length > 0 && line.Substring(0, 1) == _checkMark;

            if (!hasCheck)
            {
                // throw a CHECK_MARK and a space at the beginning of the line we're on
                lines[lineIndex] = _checkMark + " " + line;
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
            Insert(_checkMark);
        }

        private void TodaysDate_Command()
        {
            Insert(DateTime.Now.ToString(_settings.DateFormat));
        }

        private void CurrentTime_Command()
        {
            Insert(DateTime.Now.ToString(_settings.TimeFormat));
        }

        private void DateAndTime_Command()
        {
            Insert(DateTime.Now.ToString(_settings.DateFormat + " " + _settings.TimeFormat));
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
            textbox.ReplaceSelectedText(textbox.SelectedText.ToLower());
        }

        private void UpperCase_Command()
        {
            textbox.ReplaceSelectedText(textbox.SelectedText.ToUpper());
        }

        private void TitleCase_Command()
        {
            textbox.ReplaceSelectedText(textbox.SelectedText.ToTitleCase());
        }

        private void ToggleCase_Command()
        {
            textbox.ReplaceSelectedText(textbox.SelectedText.ToggleCase());
        }

        private async void Define_Command()
        {
            // if we're downloading or reading the file currently, don't do anything
            if (LocalDictionary.Downloading || LocalDictionary.Loading)
            {
                return;
            }

            string text = textbox.SelectedText.Trim();

            // no text selected
            if (text == "")
            {
                Alert.showDialog("Please select a word to define.", Global.AppName);
                return;
            }

            // don't have a local copy and we aren't currently downloading one
            if (!LocalDictionary.Downloaded && !LocalDictionary.Downloading)
            {
                // download dictionary file from repo
                bool successful = await LocalDictionary.TryDownloadAsync();

                if (!successful)
                {
                    Alert.showDialog("Couldn't download the dictionary. Please ensure that you are connected to the internet and try again.",
                        Global.AppName);
                    return;
                }
            }

            // have a local copy and aren't currently reading it, but it isn't yet in memory
            if (!LocalDictionary.Loaded && !LocalDictionary.Loading)
            {
                // load file into memory
                bool successful = await LocalDictionary.TryLoadAsync();

                if (!successful)
                {
                    var result = Alert.showDialog($"Couldn't load the local dictionary. Would you like to view the log?",
                        title: Global.AppName, button1Text: "Yes", button2Text: "No");

                    if (result == AlertResult.button1Clicked)
                    {
                        try
                        {
                            Process.Start(Logger.FilePath);
                        }
                        catch (Exception ex)
                        {
                            Logger.Log(GetType(), ex, "Attempting to view log");
                        }
                    }

                    return;
                }
            }

            // have a local copy in memory
            if (LocalDictionary.Loaded)
            {
                ShowDefinition(text);
            }
        }

        private void Reverse_Command()
        {
            textbox.ReplaceSelectedText(textbox.SelectedText.ReverseLines());
        }

        private void Sort_Command(object descending)
        {
            textbox.ReplaceSelectedText(textbox.SelectedText.SortLines((bool)descending));
        }

        #region Helpers

        private void toggleFontStyle(string marker)
        {
            string text = textbox.SelectedText;

            // toggle marker for all lines
            if (text.Contains('\n'))
            {
                textbox.ReplaceSelectedText(text.ToggleLineStart(marker + " "));
            }
            else // toggle bold within line
            {
                textbox.ReplaceSelectedText(text.ToggleStartAndEnd(marker + " ", " " + marker));
            }
        }

        #endregion

        #endregion

        #region Settings

        private void theme_Checked(object sender, RoutedEventArgs e)
        {
            var item = sender as MenuItem;

            // select theme
            _theme = item.Header.ToString() == "Light" ? Theme.light : Theme.dark;

            // uncheck other themes
            UncheckSiblings(item);

            // set theme for app
            Application.Current.Resources.
                MergedDictionaries[0].Source = ThemeManager.themeUri(_theme);
        }

        private void theme_Unchecked(object sender, RoutedEventArgs e)
        {
            var item = sender as MenuItem;

            // if this setting is selected
            if (_theme.ToString() == item.Header.ToString().ToLower())
            {
                KeepMenuItemChecked(item, theme_Checked);
            }
        }

        private void date_Checked(object sender, RoutedEventArgs e)
        {
            var item = sender as MenuItem;
            _settings.DateFormat = item.Header.ToString();
            UncheckSiblings(item);
        }

        private void date_Unchecked(object sender, RoutedEventArgs e)
        {
            var item = sender as MenuItem;

            // if this setting is selected
            if (_settings.DateFormat == item.Header.ToString())
            {
                KeepMenuItemChecked(item, date_Checked);
            }
        }

        private void time_Checked(object sender, RoutedEventArgs e)
        {
            var item = sender as MenuItem;
            _settings.TimeFormat = item.Header.ToString();
            UncheckSiblings(item);
        }

        private void time_Unchecked(object sender, RoutedEventArgs e)
        {
            var item = sender as MenuItem;

            // if this setting is selected
            if (_settings.TimeFormat == item.Header.ToString())
            {
                KeepMenuItemChecked(item, time_Checked);
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
            StatusBarVisible = !StatusBarVisible;
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
            Topmost = topmostDrowndown.IsChecked;
        }

        #endregion

        #region Help

        private void fontStyleGuide_Click(object sender, RoutedEventArgs e)
        {
            var fontStyleFile = Path.Combine(Global.DataPath, "font-style-guide.txt");

            // file isn't there yet. See if we can create it.
            if (!File.Exists(fontStyleFile))
            {
                try
                {
                    Global.CreateDirectoryAndFile(fontStyleFile);
                    File.WriteAllText(fontStyleFile, Properties.Resources.font_style_guide);
                }
                catch (Exception ex)
                {
                    Global.ActionMessage(
                        "Cannot access your APPDATA folder, which is where the font style guide is located.", ex.Message);
                    return;
                }
            }

            // open the file in the user's default text editor
            Global.Launch(fontStyleFile);
        }

        private void help_Click(object sender, RoutedEventArgs e)
        {
            Global.Launch((sender as MenuItem).Tag.ToString());
        }

        #endregion
        
        #region Helpers

        /// <summary>
        /// Call <see cref="LocalDictionary.Define"/> with <see cref="word"/>,
        /// show an Alert dialog with the definition (or couldn't find ...)
        /// </summary>
        /// <param name="word">Word to define</param>
        private void ShowDefinition(string word)
        {
            string definition = LocalDictionary.Define(word);

            if (definition != null)
            {
                Alert.showDialog(word + ": " + definition, Global.AppName);
            }
            else
            {
                Alert.showDialog("Couldn't find a definition for '" + word + "'", Global.AppName);
            }
        }

        /// <summary>
        /// Insert the specified text into <see cref="textbox"/>,
        /// then move the caret to the end of the inserted text
        /// </summary>
        /// <param name="text">Text to insert</param>
        private void Insert(string text)
        {
            // grab position we're going to before we reset textbox.Text
            int position = textbox.CaretOffset + text.Length;

            // insert the text
            textbox.Document.Text = textbox.Document.Text.Insert(textbox.CaretOffset, text);

            // go to the previously calculated position
            textbox.CaretOffset = position;
        }

        /// <summary>
        /// Unchecks all of the siblings of the provided <see cref="MenuItem"/>
        /// </summary>
        private void UncheckSiblings(MenuItem item)
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
        /// Check all child MenuItems whose header == provided value
        /// </summary>
        /// <param name="parent">Parent MenuItem</param>
        /// <param name="value">Header value</param>
        private void CheckIfSameValue(MenuItem parent, string value)
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
        private void OpenFindHelper()
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
        private void KeepMenuItemChecked(MenuItem item, RoutedEventHandler handler)
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
        private void SetFoundForeground(bool wasFound)
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
        /// Calls <see cref="textbox.FindNext"/> with the appropriate arguments based on the ui.
        /// loopback should be true if searching backwards from the start.
        /// Updates lblMatches.Text with the number of matches found using the regex in txtFind.Text.
        /// </summary>
        /// <param name="start">Where to start searching</param>
        /// <param name="lookback">Look back from this point?</param>
        private async void FindHelper(int start, bool lookback = false)
        {
            bool _matchCase = matchCase.IsChecked == true;

            bool found = textbox.FindNext(txtFind.Text, 
                start, _matchCase, lookback);

            SetFoundForeground(found);

            // if there's nothing in txtFind, don't count the matches
            if (txtFind.Text == "")
            {
                lblMatches.Text = "-";
                return;
            }

            // update lblMatches with the number of matches, or '-' if there is an invalid regex string
            lblMatches.Text = (await textbox.Document.Text.TryCountMatchesAsync(txtFind.Text, _matchCase))?.ToString() ?? "-";
        }

        private void txtFind_TextChanged(object sender, RoutedEventArgs e)
        {
            // find first instance of text entered
            FindHelper(0);
        }

        private void findUp_Click(object sender, RoutedEventArgs e)
        {
            // look back from selection
            FindHelper(textbox.SelectionStart, true);
        }

        private void findDown_Click(object sender, RoutedEventArgs e)
        {
            // look forward from selection
            FindHelper(textbox.SelectionStart + textbox.SelectionLength);
        }

        private void closeFindReplace_Click(object sender, RoutedEventArgs e)
        {
            // close find and replace
            findPanelParent.Visibility = Visibility.Collapsed;
            replacePanelParent.Visibility = Visibility.Collapsed;
        }

        #endregion

        #region Replace

        private void replaceNext_Click(object sender, RoutedEventArgs e)
        {
            bool replaced = textbox.ReplaceNext(txtFind.Text, txtReplace.Text,
                textbox.SelectionStart + textbox.SelectionLength, matchCase.IsChecked == true);

            // run find again to update the ui
            FindHelper(0);
        }

        private void replaceAll_Click(object sender, RoutedEventArgs e)
        {
            textbox.ReplaceAll(txtFind.Text, txtReplace.Text, 
                matchCase.IsChecked == true, (count) =>
            {
                // this determines whether or not to run the replace
                return count > 0 && Alert.showDialog($"Replace {count} instances of {txtFind.Text} with {txtReplace.Text}?",
                    title: Global.AppName, button1Text: "OK", button2Text: "Cancel") == AlertResult.button1Clicked;
            });

            // run find again to update the ui
            FindHelper(0);
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
                Alert.showDialog("Line number must be an integer.", Global.AppName);
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
            if (OpenFile != null && File.Exists(OpenFile.FullName) && _promptForReload)
            {
                try
                {
                    string fileText = File.ReadAllText(OpenFile.FullName);

                    // file has been modified
                    if (fileText != _savedText)
                    {
                        _promptForReload = false;

                        // set savedText to the new text from the file
                        _savedText = fileText;
                        FileSaved = FileSaved;

                        // want to reload it?
                        var result = Alert
                            .showDialog($"'{OpenFile.Name}' has been modified by another program. Would you like to reload it?",
                                title: Global.AppName, button1Text: "Reload from file", button2Text: "Keep my changes");

                        // yes i do
                        if (result == AlertResult.button1Clicked)
                        {
                            Open(OpenFile.FullName);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.Log(typeof(MainView), ex, "window_Activated");
                }
            }
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // we don't need to check for a new verson if we're debugging
            if (Debugger.IsAttached)
            {
                return;
            }

            // grab the newest version from the repo
            string newVersion = await VersionChecker.TryGetNewVersionAsync();

            if (newVersion != null && newVersion != Global.Version)
            {
                // new version available: download it?
                var result = Alert
                    .showDialog($"A new version of {Global.AppName} is available (version {newVersion}). Would you like to download it?",
                        title: "Pad#", button1Text: "Yes", button2Text: "No");

                // go to the link to the setup file in the repo if the user clicked Yes
                if (result == AlertResult.button1Clicked)
                {
                    Global.Launch("https://github.com/collenirwin/PadSharp/blob/master/setup/pad_sharp_setup.exe");
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
                FontSize += delta;
                fontSizeDropdown.Text = FontSize.ToString();
            }
        }

        private void window_Closing(object sender, CancelEventArgs e)
        {
            // if the user has not saved their changes, but they wish to
            if (textbox.Text != _savedText &&
                Alert.showDialog("Close without saving?",
                Global.AppName, "Close", "Cancel") == AlertResult.button2Clicked)
            {
                e.Cancel = true; // don't close
            }
            else
            {
                // set all user settings
                SetSettings(_settings);

                // save user settings, show a message if saving fails
                if (_settings != null && !_settings.Save())
                {
                    Global.ActionMessage("Failed to save settings.json",
                        "A potential fix may be to run " +
                        Global.AppName + " as an administrator.");
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
            LineNumber = textbox.TextArea.Caret.Line;
            ColumnNumber = textbox.TextArea.Caret.Column;
        }

        private void textbox_TextChanged(object sender, EventArgs e)
        {
            // count the words on textchanged
            WordCount = Regex.Matches(textbox.Text, @"\b\S+\b").Count;

            // force UI to acknowledge fileSaved
            FileSaved = false;
        }

        private void textArea_SelectionChanged(object sender, EventArgs e)
        {
            // force UI to acknowledge selectionMenuVisibility (we're not actually affecting it)
            SelectionMenuVisibility = Visibility.Visible;
        }

        #endregion

        #region IO helpers

        /// <summary>
        /// Attempts to open the file specified by path in the editor.
        /// Gives appropriate error prompts.
        /// Also sets <see cref="OpenFile"/> to the newly opened file's FileInfo.
        /// </summary>
        /// <param name="path">Path to the desired file</param>
        private void Open(string path)
        {
            try
            {
                var _file = new FileInfo(path);

                if (_file.Exists)
                {
                    // under limit
                    if (_file.Length < _fileSizeLimit)
                    {
                        // read text from path into textbox
                        textbox.Text = File.ReadAllText(path);
                        _savedText = textbox.Text;
                        OpenFile = _file;

                        // prompt the user if the file changes
                        _promptForReload = true;
                    }
                    else
                    {
                        Global.ActionMessage("Failed to open '" + path + "'",
                            $"File is too large (must be under {_fileSizeLimit / 1000}k).");
                    }
                }
                else
                {
                    Global.ActionMessage("Failed to open '" + path + "'", "File does not exist.");
                }
            }
            catch (Exception ex)
            {
                string message = "Failed to open '" + path + "'";
                Global.ActionMessage(message, ex.Message);
                Logger.Log(typeof(MainView), ex, message);
            }
        }

        /// <summary>
        /// Attempts to save the file specified by path.
        /// Gives an appropriate error message if it can't save.
        /// Also sets <see cref="OpenFile"/> to the (possibly updated) file's FileInfo.
        /// </summary>
        /// <param name="path">Path to the desired file</param>
        private void Save(string path)
        {
            try
            {
                // write textbox text to path
                File.WriteAllText(path, textbox.Text);
                _savedText = textbox.Text;
                OpenFile = new FileInfo(path);

                // prompt the user if the file changes
                _promptForReload = true;
            }
            catch (Exception ex)
            {
                string message = "Failed to save to '" + path + "'";
                Global.ActionMessage(message, ex.Message);
                Logger.Log(typeof(MainView), ex, message);
            }
        }

        /// <summary>
        /// Opens a <see cref="SaveFileDialog"/> prompting the user to select a place to save,
        /// then calls save with the selected path.
        /// </summary>
        private void saveAs()
        {
            var saveDialog = new SaveFileDialog
            {
                FileName = FileSaved ? OpenFile.Name : "document.txt",
                Title = "Save As",
                DefaultExt = ".txt",
                Filter = "Text Files|*.txt|All Files|*.*",
                AddExtension = true
            };

            var result = saveDialog.ShowDialog();

            string path = saveDialog.FileName;

            // if a file was selected, save it
            if (result == true && path != null && path != "")
            {
                Save(path);
            }
        }

        #endregion

        #endregion
    }
}
