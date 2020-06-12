using System.Windows;

namespace BoinWPF
{
    public enum AlertResult
    {
        Button1Clicked,
        Button2Clicked,
        Closed
    }

    /// <summary>
    /// Small (but scaling) dialog window
    /// </summary>
    public partial class Alert : Window
    {
        /// <summary>
        /// Show a modal dialog with the specified text displayed
        /// </summary>
        /// <param name="message">Message to display</param>
        /// <param name="title">Titlebar text</param>
        /// <param name="button1Text">Text for the first button</param>
        /// <param name="button2Text">Text for the second button</param>
        public static AlertResult ShowDialog(string message, string title = "",
            string button1Text = "OK", string button2Text = null)
        {
            return GetAlertResult(new Alert(message, title, button1Text, button2Text));
        }

        /// <summary>
        /// Show a modal dialog with the specified text displayed.
        /// This one has a toggleable "more information" textbox.
        /// </summary>
        /// <param name="message">Message to display</param>
        /// <param name="moreInfo">Information to put in a toggleable textbox</param>
        /// <param name="title">Titlebar text</param>
        /// <param name="button1Text">Text for the first button</param>
        /// <param name="button2Text">Text for the second button</param>
        public static AlertResult ShowMoreInfoDialog(string message, string moreInfo, string title = "", 
            string button1Text = "OK", string button2Text = null)
        {
            return GetAlertResult(new Alert(message, moreInfo, title, button1Text, button2Text));
        }

        private static AlertResult GetAlertResult(Alert alert)
        {
            var result = alert.ShowDialog();

            if (result.GetValueOrDefault())
            {
                return AlertResult.Button1Clicked;
            }

            if (result == null)
            {
                return AlertResult.Closed;
            }

            return AlertResult.Button2Clicked;
        }

        /// <summary>
        /// Small (but scaling) dialog window
        /// </summary>
        /// <param name="message">Message to display</param>
        /// <param name="moreInfo">Information to put in a toggleable textbox</param>
        /// <param name="title">Titlebar text</param>
        /// <param name="button1Text">Text for the first button</param>
        /// <param name="button2Text">Text for the second button</param>
        public Alert(string message, string moreInfo, string title,
            string button1Text, string button2Text)
        {
            InitializeComponent();

            MessageTextBox.Text = message;
            Button1.Content = button1Text;

            if (moreInfo != null)
            {
                ShowMoreButton.Visibility = Visibility.Visible;
                MoreInfoTextBox.Text = moreInfo;
            }

            if (button2Text != null)
            {
                Button2.Content = button2Text;
                Button2.Visibility = Visibility.Visible;
            }

            Title = title;
        }

        /// <summary>
        /// Small (but scaling) dialog window
        /// </summary>
        /// <param name="message">Message to display</param>
        /// <param name="title">Titlebar text</param>
        /// <param name="button1Text">Text for the first button</param>
        /// <param name="button2Text">Text for the second button</param>
        public Alert(string message, string title, string button1Text, string button2Text) 
            : this(message, null, title, button1Text, button2Text) { }

        private void Button1_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void Button2_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            DialogResult = null;
        }

        private void ShowMoreButton_Click(object sender, RoutedEventArgs e)
        {
            // toggle the more info textbox
            if (ShowMoreButton.Content.ToString() == "More")
            {
                MoreInfoScrollView.Visibility = Visibility.Visible;
                ShowMoreButton.Content = "Less";
            }
            else
            {
                MoreInfoScrollView.Visibility = Visibility.Collapsed;
                ShowMoreButton.Content = "More";
            }
        }
    }
}
