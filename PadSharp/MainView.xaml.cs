using BoinWPF;
using BoinWPF.Themes;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using ICSharpCode.AvalonEdit.Utils;
using Microsoft.Win32;
using PadSharp.Commands;
using PadSharp.Utils;
using RegularExtensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
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
    /// Main window - all text editing is done here
    /// </summary>
    public partial class MainView : Window, INotifyPropertyChanged
    {
        #region Fields, Properties

        #region Fields

        /// <summary>
        /// Required to implement INotifyPropertyChanged
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged = (sender, e) => { };

        /// <summary>
        /// Settings object (contains all user settings)
        /// </summary>
        private readonly UISettings _settings;

        /// <summary>
        /// 5m file size max
        /// </summary>
        private const int _fileSizeLimit = 5_000_000;

        /// <summary>
        /// 500px font size max
        /// </summary>
        private const double _maxFontSize = 500;

        /// <summary>
        /// Our marker for highlighting a checked line
        /// </summary>
        private const string _checkMark = "✔";

        /// <summary>
        /// Pad# (version #)
        /// </summary>
        private const string _title = Global.AppName + " " + Global.Version;

        /// <summary>
        /// Currently applied theme
        /// </summary>
        private Theme _theme;

        /// <summary>
        /// Text from currently open file
        /// </summary>
        private string _savedText = "";

        /// <summary>
        /// Should we prompt the user to reload the file if it has been modified?
        /// </summary>
        private bool _promptForReload = false;

        #endregion

        #region Properties

        #region UICommands

        #region File

        public ICommand NewCommand { get; }
        public ICommand NewWindowCommand { get; }
        public ICommand OpenCommand { get; }
        public ICommand OpenInExplorerCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand SaveAsCommand { get; }
        public ICommand PrintCommand { get; }
        public ICommand ExitCommand { get; }

        #endregion

        #region Edit

        public ICommand UndoCommand { get; }
        public ICommand RedoCommand { get; }
        public ICommand CutCommand { get; }
        public ICommand CopyCommand { get; }
        public ICommand PasteCommand { get; }
        public ICommand FindCommand { get; }
        public ICommand FindAndReplaceCommand { get; }
        public ICommand GotoCommand { get; }
        public ICommand GotoGoCommand { get; }
        public ICommand CheckSpellingCommand { get; }
        public ICommand NormalizeLineEndingdCommand { get; }
        public ICommand SelectAllCommand { get; }

        #endregion

        #region Insert

        public ICommand CheckMarkCommand { get; }
        public ICommand AddCheckMarkCommand { get; }
        public ICommand DateInsertDialogCommand { get; }
        public ICommand TodaysDateCommand { get;}
        public ICommand CurrentTimeCommand { get; }
        public ICommand DateAndTimeCommand { get; }

        #endregion

        #region Selection

        public ICommand BoldCommand { get; }
        public ICommand ItalicCommand { get; }
        public ICommand UnderlineCommand { get; }
        public ICommand LowerCaseCommand { get; }
        public ICommand UpperCaseCommand { get; }
        public ICommand TitleCaseCommand { get; }
        public ICommand ToggleCaseCommand { get; }
        public ICommand DefineCommand { get; }
        public ICommand ReverseCommand { get; }
        public ICommand SortCommand { get; }

        #endregion

        #region Settings

        public ICommand FontCommand { get; }
        public ICommand DateTimeFormatCommand { get; }
        public ICommand ToggleLineNumbersCommand { get; }
        public ICommand ToggleStatusBarCommand { get; }
        public ICommand ToggleWordWrapCommand { get; }
        public ICommand ToggleTopmostCommand { get; }

        #endregion

        #region Help

        public UICommand AboutCommand { get; }

        #endregion

        #endregion

        #region Properties with Associated Fields

        /// <summary>
        /// Goes in the title bar of the app. Format: [filename?] - TITLE
        /// </summary>
        public string FullTitle => OpenFile != null ? $"{OpenFile.Name} - {_title}" : _title;

        private FileInfo _openFile;

        /// <summary>
        /// Currently open file
        /// </summary>
        public FileInfo OpenFile
        {
            get => _openFile;
            set
            {
                if (_openFile != value)
                {
                    _openFile = value;
                    RaiseChange(nameof(OpenFile));
                    RaiseChange(nameof(FileSaved));
                    RaiseChange(nameof(FullTitle));
                }
            }
        }

        /// <summary>
        /// Is the file saved?
        /// </summary>
        public bool FileSaved => OpenFile != null && textbox.Text == _savedText;

        /// <summary>
        /// Date format string we use when inserting dates
        /// </summary>
        public string DateFormat
        {
            get => _settings?.DateFormat ?? UISettings.Default.DateFormat;
            set => SetAndRaiseIfChanged(oldValue: _settings.DateFormat, newValue: value,
                () => _settings.DateFormat = value);
        }

        /// <summary>
        /// Time format string we use when inserting times
        /// </summary>
        public string TimeFormat
        {
            get => _settings?.TimeFormat ?? UISettings.Default.TimeFormat;
            set => SetAndRaiseIfChanged(oldValue: _settings.TimeFormat, newValue: value,
                () => _settings.TimeFormat = value);
        }

        /// <summary>
        /// Hides or shows statusBar
        /// </summary>
        public bool StatusBarVisible
        {
            get => statusBar?.Visibility == Visibility.Visible;
            set
            {
                if (statusBar == null)
                {
                    return;
                }

                statusBar.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
                RaiseChange(nameof(StatusBarVisible));
            }
        }

        private int _lineNumber = 1;

        /// <summary>
        /// Line number the caret is on
        /// </summary>
        public int LineNumber
        {
            get => _lineNumber;
            set => SetAndRaiseIfChanged(ref _lineNumber, value);
        }

        /// <summary>
        /// Show the line number in the status bar?
        /// </summary>
        public bool LineNumberVisible
        {
            get => _settings?.LineNumberVisible ?? UISettings.Default.LineNumberVisible;
            set => SetAndRaiseIfChanged(oldValue: _settings.LineNumberVisible, newValue: value,
                () => _settings.LineNumberVisible = value);
        }

        private int _columnNumber = 1;

        /// <summary>
        /// Column number the caret is on
        /// </summary>
        public int ColumnNumber
        {
            get => _columnNumber;
            set => SetAndRaiseIfChanged(ref _columnNumber, value);
        }

        /// <summary>
        /// Show the column number in the status bar?
        /// </summary>
        public bool ColumnNumberVisible
        {
            get => _settings?.ColumnNumberVisible ?? UISettings.Default.ColumnNumberVisible;
            set => SetAndRaiseIfChanged(oldValue: _settings.ColumnNumberVisible, newValue: value,
                () => _settings.ColumnNumberVisible = value);
        }

        private int _wordCount = 0;

        /// <summary>
        /// Number of words in textbox (found using \b\S+\b)
        /// </summary>
        public int WordCount
        {
            get => _wordCount;
            set => SetAndRaiseIfChanged(ref _wordCount, value);
        }

        /// <summary>
        /// Show the word count in the status bar?
        /// </summary>
        public bool WordCountVisible
        {
            get => _settings?.WordCountVisible ?? UISettings.Default.WordCountVisible;
            set => SetAndRaiseIfChanged(oldValue: _settings.WordCountVisible, newValue: value,
                () => _settings.WordCountVisible = value);
        }

        /// <summary>
        /// Show the char count in the status bar?
        /// </summary>
        public bool CharCountVisible
        {
            get => _settings?.CharCountVisible ?? UISettings.Default.CharCountVisible;
            set => SetAndRaiseIfChanged(oldValue: _settings.CharCountVisible, newValue: value,
                () => _settings.CharCountVisible = value);
        }

        /// <summary>
        /// Font family of <see cref="textbox"/>
        /// </summary>
        public FontFamily TextBoxFontFamily
        {
            get => textbox?.FontFamily ?? UISettings.Default.FontFamily;
            set => SetAndRaiseIfChanged(oldValue: textbox.FontFamily, newValue: value,
                () => textbox.FontFamily = value);
        }

        /// <summary>
        /// Font size of <see cref="textbox"/>
        /// </summary>
        public double TextBoxFontSize
        {
            get => textbox?.FontSize ?? UISettings.Default.FontSize;
            set
            {
                if (textbox.FontSize == value)
                {
                    return;
                }

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

                RaiseChange(nameof(TextBoxFontSize));
            }
        }

        /// <summary>
        /// All font families available for selection
        /// </summary>
        public ObservableCollection<FontFamily> FontFamilies { get; } = new ObservableCollection<FontFamily>();

        /// <summary>
        /// Show the Selection Menu whenever text is selected
        /// </summary>
        public Visibility SelectionMenuVisibility => (textbox?.SelectionLength ?? 0) > 0
            ? Visibility.Visible
            : Visibility.Collapsed;

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
            SaveAsCommand = new UICommand(SaveAs);
            PrintCommand = new UICommand(Print_Command);
            ExitCommand = new UICommand(Close);

            // edit
            UndoCommand = new UICommand(() => textbox.Undo());
            RedoCommand = new UICommand(() => textbox.Redo());
            CutCommand = new UICommand(() => textbox.Cut());
            CopyCommand = new UICommand(() => textbox.Copy());
            PasteCommand = new UICommand(() => textbox.Paste());
            FindCommand = new UICommand(Find_Command);
            FindAndReplaceCommand = new UICommand(FindReplace_Command);
            GotoCommand = new UICommand(Goto_Command);
            GotoGoCommand = new UICommand(GotoGo_Command);
            CheckSpellingCommand = new UICommand(CheckSpelling_Command);
            NormalizeLineEndingdCommand = new UICommand(NormalizeLineEndings_Command);
            SelectAllCommand = new UICommand(() => textbox.SelectAll());

            // insert
            CheckMarkCommand = new UICommand(CheckMark_Command);
            AddCheckMarkCommand = new UICommand(AddCheckMark_Command);
            DateInsertDialogCommand = new UICommand(DateInsertDialog_Command);
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
            FontCommand = new UICommand(Font_Command);
            DateTimeFormatCommand = new UICommand(DateTimeFormat_Command);
            ToggleLineNumbersCommand = new UICommand(ToggleLineNumbers_Command);
            ToggleStatusBarCommand = new UICommand(ToggleStatusBar_Command);
            ToggleWordWrapCommand = new UICommand(ToggleWordWrap_Command);
            ToggleTopmostCommand = new UICommand(ToggleTopmost_Command);

            // help
            AboutCommand = new UICommand(About_Command);

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

            // grab the system fonts, populating FontFamilies
            FontUtils.PopulateFontCollection(FontFamilies);

            // minimize the window so that we can adjust the size/position in the loaded event
            // without showing the window in its initial state
            WindowState = WindowState.Minimized;
        }

        #endregion

        #region Settings

        /// <summary>
        /// Set all window properties to match the given settings
        /// </summary>
        private void ApplySettings(UISettings settings)
        {
            // set theme
            _theme = settings.Theme;
            Application.Current.Resources.
                MergedDictionaries[0].Source = ThemeManager.themeUri(_theme);

            CheckIfSameValue(themeMenu, _theme == Theme.light ? "Light" : "Dark");

            // font
            textbox.FontFamily = settings.FontFamily;
            TextBoxFontSize = settings.FontSize;
            fontSizeDropdown.Text = TextBoxFontSize.ToString();

            var topBound = SystemParameters.VirtualScreenHeight - Math.Abs(SystemParameters.VirtualScreenTop) - Height;
            var leftBound = SystemParameters.VirtualScreenWidth - Math.Abs(SystemParameters.VirtualScreenLeft) - Width;

            // only set our location if it's within the bounds of the user's screen(s)
            if (settings.Top >= SystemParameters.VirtualScreenTop && 
                settings.Left >= SystemParameters.VirtualScreenLeft &&
                settings.Top <= topBound &&
                settings.Left <= leftBound)
            {
                Top = settings.Top;
                Left = settings.Left;
            }

            WindowState = settings.WindowState;

            // make sure we don't start minimized
            if (WindowState == WindowState.Minimized)
            {
                WindowState = WindowState.Normal;
            }

            // only apply size settings if not maximized
            if (WindowState == WindowState.Normal)
            {
                Height = settings.Height;
                Width = settings.Width;
            }

            // set toggles
            showStatusBarDropdown.IsChecked = settings.ShowStatusBar;
            textbox.WordWrap = settings.WordWrap;
            textbox.ShowLineNumbers = settings.ShowLineNumbers;
            Topmost = settings.Topmost;

            RaiseChange(nameof(LineNumberVisible));
            RaiseChange(nameof(ColumnNumberVisible));
            RaiseChange(nameof(WordCountVisible));
            RaiseChange(nameof(CharCountVisible));
        }

        /// <summary>
        /// Set all JSON properties in a settings object to match window properties
        /// </summary>
        private void SetSettings(UISettings settings)
        {
            settings.Theme = _theme;
            settings.FontFamily = textbox.FontFamily;
            settings.FontSize = TextBoxFontSize;
            settings.WindowState = WindowState;

            settings.Top = Top;
            settings.Left = Left;
            settings.Height = Height;
            settings.Width = Width;

            settings.ShowLineNumbers = textbox.ShowLineNumbers;
            settings.ShowStatusBar = statusBar.Visibility == Visibility.Visible;
            settings.WordWrap = textbox.WordWrap;
            settings.Topmost = Topmost;
        }

        #endregion

        #region Font

        /// <summary>
        /// Handles changing the textbox font size via the dropdown
        /// </summary>
        private void fontSizeDropdown_Changed(object sender, EventArgs e)
        {
            if (double.TryParse(fontSizeDropdown.Text, out double size))
            {
                TextBoxFontSize = size;
            }
            else
            {
                fontSizeDropdown.SelectedItem = TextBoxFontSize.ToString();
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
            if (result.GetValueOrDefault() && !string.IsNullOrEmpty(path))
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
                SaveAs();
            } 
            else
            {
                Save(OpenFile.FullName);
            }
        }

        private void Print_Command()
        {
            var printDialog = new PrintDialog();

            // user pressed 'ok' to print
            if (printDialog.ShowDialog().GetValueOrDefault())
            {
                // get a FlowDocument from our editor
                var flowDoc = DocumentPrinter.CreateFlowDocumentForEditor(textbox);

                printDialog.PrintDocument(
                    ((IDocumentPaginatorSource)flowDoc).DocumentPaginator,
                    $"Pad#: {lblFileName.Text}");
            }
        }

        #endregion

        #region Edit

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
            textbox.NormalizeLineEndings(windows: true);

            // give the user some feedback - this change won't be obvious
            Alert.showDialog("Done. All line endings have been converted to the Windows format (CRLF).",
                Global.AppName);
        }

        #endregion

        #region Insert

        private void AddCheckMark_Command()
        {
            textbox.Insert(_checkMark);
        }

        private void DateInsertDialog_Command()
        {
            var dateInsertDialog = new DateInsertDialog(owner: this);
            dateInsertDialog.ShowDialog();
        }

        private void TodaysDate_Command()
        {
            textbox.Insert(DateTime.Now.ToString(DateFormat));
        }

        private void CurrentTime_Command()
        {
            textbox.Insert(DateTime.Now.ToString(TimeFormat));
        }

        private void DateAndTime_Command()
        {
            textbox.Insert(DateTime.Now.ToString($"{DateFormat} {TimeFormat}"));
        }

        #endregion

        #region Selection

        private void Bold_Command()
        {
            ToggleFontStyle("**");
        }

        private void Italic_Command()
        {
            ToggleFontStyle("*");
        }

        private void CheckMark_Command()
        {
            textbox.ToggleSelectionLineStart(_checkMark + " ");
        }

        private void Underline_Command()
        {
            ToggleFontStyle("__");
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
                    Alert.showDialog("Couldn't download the dictionary. " +
                        "Please ensure that you are connected to the internet and try again.",
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
                    var result = Alert.showDialog($"Couldn't load the local dictionary. " +
                        "Would you like to view the log?",
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

        private void ToggleFontStyle(string marker)
        {
            string text = textbox.SelectedText;

            // toggle marker for all lines
            if (text == "" || text.Contains('\n'))
            {
                textbox.ToggleSelectionLineStart(marker + " ");
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
            Application.Current.Resources.MergedDictionaries[0].Source = ThemeManager.themeUri(_theme);
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

        private void Font_Command()
        {
            var fontDialog = new FontDialog(owner: this);
            fontDialog.ShowDialog();
        }

        private void DateTimeFormat_Command()
        {
            var dateTimeFormatDialog = new DateTimeFormatDialog(owner: this);
            dateTimeFormatDialog.ShowDialog();
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
                        "Cannot access your APPDATA folder, which is where the font style guide is located.",
                        ex.Message);
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

        private void About_Command()
        {
            AboutWindow.Instance.Show();
            AboutWindow.Instance.Focus();
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
                Alert.showDialog($"{word}: {definition}", Global.AppName);
            }
            else
            {
                Alert.showDialog($"Couldn't find a definition for '{word}'", Global.AppName);
            }
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
        /// Put selected text into find box, select it
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
            bool _matchCase = matchCase.IsChecked.GetValueOrDefault();
            bool found = textbox.FindNext(txtFind.Text, start, _matchCase, lookback);

            SetFoundForeground(found);

            // if there's nothing in txtFind, don't count the matches
            if (txtFind.Text == "")
            {
                lblMatches.Text = "-";
                return;
            }

            // update lblMatches with the number of matches, or '-' if there is an invalid regex string
            lblMatches.Text = (await textbox.Document.Text
                .TryCountMatchesAsync(txtFind.Text, _matchCase))?.ToString() ?? "-";
        }

        private void txtFind_TextChanged(object sender, RoutedEventArgs e)
        {
            // find first instance of text entered
            FindHelper(start: 0);
        }

        private void findUp_Click(object sender, RoutedEventArgs e)
        {
            // look back from selection
            FindHelper(start: textbox.SelectionStart, lookback: true);
        }

        private void findDown_Click(object sender, RoutedEventArgs e)
        {
            // look forward from selection
            FindHelper(start: textbox.SelectionStart + textbox.SelectionLength);
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
            textbox.ReplaceNext(txtFind.Text, txtReplace.Text,
                textbox.SelectionStart + textbox.SelectionLength, matchCase.IsChecked.GetValueOrDefault());

            // run find again to update the ui
            FindHelper(start: 0);
        }

        private void replaceAll_Click(object sender, RoutedEventArgs e)
        {
            textbox.ReplaceAll(txtFind.Text, txtReplace.Text, 
                matchCase.IsChecked.GetValueOrDefault(), count =>
                // this determines whether or not to run the replace
                count > 0 && Alert.showDialog($"Replace {count} instances of {txtFind.Text} with {txtReplace.Text}?",
                    title: Global.AppName, button1Text: "OK", button2Text: "Cancel") == AlertResult.button1Clicked);

            // run find again to update the ui
            FindHelper(start: 0);
        }

        #endregion

        #region Goto

        private void GotoGo_Command()
        {
            if (int.TryParse(txtGoto.Text, out int line))
            {
                textbox.ScrollTo(line, column: 0);
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

                        // want to reload it?
                        var result = Alert.showDialog($"'{OpenFile.Name}' has been modified by another program. " +
                            "Would you like to reload it?",
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
            // apply settings to UI
            ApplySettings(_settings);

            // check for a new version if we're not debugging
            if (!Debugger.IsAttached)
            {
                // grab the newest version from the repo
                string newVersion = await VersionChecker.TryGetNewVersionAsync();

                if (newVersion != null && newVersion != Global.Version)
                {
                    // new version available: download it?
                    var result = Alert.showDialog(
                        $"A new version of {Global.AppName} is available (version {newVersion}). " +
                        "Would you like to download it?",
                        title: "Pad#", button1Text: "Yes", button2Text: "No");

                    // go to the link to the setup file in the repo if the user clicked Yes
                    if (result == AlertResult.button1Clicked)
                    {
                        Global.Launch(
                            "https://github.com/collenirwin/PadSharp/blob/master/setup/pad_sharp_setup.exe");
                    }
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
                TextBoxFontSize += delta;
                fontSizeDropdown.Text = TextBoxFontSize.ToString();
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
            WordCount = textbox.Text.Matches(@"\b\S+\b").Count;
            RaiseChange(nameof(FileSaved));
        }

        private void textArea_SelectionChanged(object sender, EventArgs e)
        {
            RaiseChange(nameof(SelectionMenuVisibility));
        }

        #endregion

        #region IO Helpers

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
        private void SaveAs()
        {
            string fileName = OpenFile?.Name;

            // file hasn't been saved yet
            if (fileName == null)
            {
                // grab the first 20 characters of the first line,
                // after all illegal characters and leading whitespace have been removed
                fileName = textbox.Text
                    .RegexReplace(@"[\r*\\/<>:\|\?]", "")
                    .TrimStart()
                    .Match(@"^[^\n]{1,20}")
                    .Value
                    .Trim() + ".txt";

                // nothing to grab; default to document.txt
                if (fileName == ".txt")
                {
                    fileName = "document.txt";
                }
            }

            var saveDialog = new SaveFileDialog
            {
                FileName = fileName,
                Title = "Save As",
                DefaultExt = ".txt",
                Filter = "Text Files|*.txt|All Files|*.*",
                AddExtension = true
            };

            var result = saveDialog.ShowDialog();

            string path = saveDialog.FileName;

            // if a file was selected, save it
            if (result == true && !string.IsNullOrEmpty(path))
            {
                Save(path);
            }
        }

        #endregion

        #region PropertyChanged Helpers

        /// <summary>
        /// Raises the <see cref="PropertyChanged"/> event with the given property name
        /// </summary>
        /// <param name="propName">Name of the property that changed</param>
        private void RaiseChange(string propName)
        {
            PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }

        /// <summary>
        /// Assigns to the given property and calls <see cref="RaiseChange"/>,
        /// but only if the new value differs from the old
        /// </summary>
        /// <typeparam name="T">Type of the property</typeparam>
        /// <param name="prop">Reference to the property to set</param>
        /// <param name="newValue">New value for the property</param>
        /// <param name="propName">Automatically assigned property name</param>
        private void SetAndRaiseIfChanged<T>(ref T prop, T newValue, [CallerMemberName] string propName = "")
        {
            if (!EqualityComparer<T>.Default.Equals(prop, newValue))
            {
                prop = newValue;
                RaiseChange(propName);
            }
        }

        /// <summary>
        /// Calls the setter action and calls <see cref="RaiseChange"/>,
        /// but only if the new value differs from the old
        /// </summary>
        /// <typeparam name="T">Type of the property</typeparam>
        /// <param name="oldValue">Reference to the property to set</param>
        /// <param name="newValue">New value for the property</param>
        /// <param name="setter">Action to assign the new value</param>
        /// <param name="propName">Automatically assigned property name</param>
        private void SetAndRaiseIfChanged<T>(T oldValue, T newValue, Action setter,
            [CallerMemberName] string propName = "")
        {
            if (!EqualityComparer<T>.Default.Equals(oldValue, newValue))
            {
                setter();
                RaiseChange(propName);
            }
        }

        #endregion

        #endregion
    }
}
