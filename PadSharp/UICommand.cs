using System;
using System.Windows.Input;

namespace PadSharp
{
    /// <summary>
    /// Barebones implementation of ICommand
    /// </summary>
    public class UICommand : ICommand
    {
        public event EventHandler CanExecuteChanged = (sender, e) => { };

        public Action action { get; private set; }

        /// <summary>
        /// Constructor for UICommand
        /// </summary>
        /// <param name="action">Action to call via Execute</param>
        public UICommand(Action action)
        {
            this.action = action;
        }

        /// <summary>
        /// Runs this.action
        /// </summary>
        /// <param name="parameter">Not used</param>
        public void Execute(object parameter)
        {
            action();
        }

        /// <summary>
        /// Can always execute
        /// </summary>
        /// <param name="parameter">Not used</param>
        /// <returns>true</returns>
        public bool CanExecute(object parameter)
        {
            return true;
        }
    }
}
