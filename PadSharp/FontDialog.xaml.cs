using System;
using System.ComponentModel;
using System.Windows;

namespace PadSharp
{
    /// <summary>
    /// Interaction logic for FontDialog.xaml
    /// </summary>
    public partial class FontDialog : Window, INotifyPropertyChanged
    {
        /// <summary>
        /// Required to implement INotifyPropertyChanged
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged = (sender, e) => { };

        /// <summary>
        /// The owner of this window
        /// </summary>
        private MainView _mainView;

        public FontDialog(MainView owner)
        {
            InitializeComponent();

            Owner = owner;
            DataContext = owner;
            _mainView = owner;
        }

        private void Done_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
