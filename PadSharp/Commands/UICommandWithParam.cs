using System;

namespace PadSharp.Commands
{
    /// <summary>
    /// Barebones implementation of ICommand
    /// </summary>
    public class UICommandWithParam : UICommandBase
    {
        /// <summary>
        /// The method this command runs when <see cref="Execute"/> is called
        /// </summary>
        public Action<object> Action { get; private set; }

        /// <summary>
        /// Constructor for UICommand
        /// </summary>
        /// <param name="action">Action to call via Execute</param>
        public UICommandWithParam(Action<object> action)
        {
            Action = action;
        }

        /// <summary>
        /// Runs <see cref="Action"/>
        /// </summary>
        /// <param name="parameter">Not used</param>
        public override void Execute(object parameter)
        {
            Action(parameter);
        }
    }
}
