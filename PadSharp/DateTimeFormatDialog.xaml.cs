using PadSharp.Utils;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Threading;

namespace PadSharp
{
    /// <summary>
    /// Provides an interface for experimenting with date and time format strings
    /// </summary>
    public partial class DateTimeFormatDialog : Window
    {
        /// <summary>
        /// Timer that updates the date and time displays
        /// </summary>
        private readonly DispatcherTimer _timer;

        /// <summary>
        /// The owner of this window
        /// </summary>
        private readonly MainView _mainView;

        public DateTimeFormatDialog(MainView owner)
        {
            InitializeComponent();

            Owner = owner;
            _mainView = owner;
            DataContext = owner;

            UpdateDisplays();

            _timer = new DispatcherTimer
            {
                // quarter second interval
                Interval = new TimeSpan(0, 0, 0, 0, 250)
            };

            _timer.Tick += (sender, e) => UpdateDisplays();
            _timer.Start();
        }

        /// <summary>
        /// Updates the date and time displays
        /// </summary>
        public void UpdateDisplays()
        {
            // update the date display
            try
            {
                DateDisplay.Text = _mainView.DateFormat.Trim() == ""
                    ? DateDisplay.Tag.ToString()
                    : DateTime.Now.ToString(_mainView.DateFormat);
            }
            catch
            {
                DateDisplay.Text = DateDisplay.Tag.ToString();
            }

            // update the time display
            try
            {
                TimeDisplay.Text = _mainView.TimeFormat.Trim() == ""
                    ? TimeDisplay.Tag.ToString()
                    : DateTime.Now.ToString(_mainView.TimeFormat);
            }
            catch
            {
                TimeDisplay.Text = TimeDisplay.Tag.ToString();
            }
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            // if there was invalid input, reset to the defaults
            if (DateDisplay.Text == DateDisplay.Tag.ToString())
            {
                _mainView.DateFormat = UISettings.DefaultDateFormat;
            }

            if (TimeDisplay.Text == TimeDisplay.Tag.ToString())
            {
                _mainView.TimeFormat = UISettings.DefaultTimeFormat;
            }

            _timer.Stop();
        }

        private void Done_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void Defaults_Click(object sender, EventArgs e)
        {
            _mainView.DateFormat = UISettings.DefaultDateFormat;
            _mainView.TimeFormat = UISettings.DefaultTimeFormat;
        }

        private void Guide_Click(object sender, EventArgs e)
        {
            Global.Launch("https://docs.microsoft.com/en-us/dotnet/standard/base-types/custom-date-and-time-format-strings");
        }
    }
}
