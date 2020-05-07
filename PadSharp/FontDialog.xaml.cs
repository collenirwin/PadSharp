using System;
using System.Windows;

namespace PadSharp
{
    /// <summary>
    /// Interaction logic for FontDialog.xaml
    /// </summary>
    public partial class FontDialog : Window
    {
        public FontDialog(MainView owner)
        {
            InitializeComponent();

            Owner = owner;
            DataContext = owner;
        }

        private void Done_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
