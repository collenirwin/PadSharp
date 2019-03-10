using BoinWPF;
using PadSharp.Utils;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace PadSharp
{
    /// <summary>
    /// A dialog window with a calendar for inserting dates into the text editor
    /// </summary>
    public partial class DateInsertDialog : Window
    {
        /// <summary>
        /// The owner of this window
        /// </summary>
        private readonly MainView _mainView;

        public DateInsertDialog(MainView owner)
        {
            InitializeComponent();

            Owner = owner;
            _mainView = owner;

            // select today's date by default
            DatePicker.SelectedDate = DateTime.Now;
        }

        /// <summary>
        /// Display <see cref="DatePicker.SelectedDate"/> in the user's date format
        /// </summary>
        private void SetDateDisplay()
        {
            DateDisplay.Text = DatePicker.SelectedDate?.ToString(_mainView.DateFormat) ?? "-";
        }

        protected override void OnPreviewMouseUp(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseUp(e);

            // this is to avoid having to click to remove focus from the calendar before clicking a button
            if (Mouse.Captured is Calendar || Mouse.Captured is CalendarItem)
            {
                Mouse.Capture(null);
            }
        }

        private void DatePicker_SelectedDatesChanged(object sender, SelectionChangedEventArgs e)
        {
            SetDateDisplay();
        }

        private void DateFormat_Click(object sender, RoutedEventArgs e)
        {
            var dateTimeFormatDialog = new DateTimeFormatDialog(_mainView);
            dateTimeFormatDialog.ShowDialog();
            SetDateDisplay();
        }

        private void InsertDate_Click(object sender, RoutedEventArgs e)
        {
            // if they have a date selected, insert it into the textbox and close
            if (DatePicker.SelectedDate != null)
            {
                _mainView.textbox.Insert(DateDisplay.Text);
                Close();
            }
            else
            {
                Alert.showDialog("Please select a date to insert.", Global.AppName);
            }
        }
    }
}
