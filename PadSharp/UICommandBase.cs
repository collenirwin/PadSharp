using System;
using System.Windows.Input;

namespace PadSharp
{
    /// <summary>
    /// Barebones implementation of ICommand (base class)
    /// </summary>
    public abstract class UICommandBase : ICommand
    {
        public event EventHandler CanExecuteChanged = (sender, e) => { };

        public abstract void Execute(object parameter);

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
