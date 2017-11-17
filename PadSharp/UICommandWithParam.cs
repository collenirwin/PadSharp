﻿using System;

namespace PadSharp
{
    /// <summary>
    /// Barebones implementation of ICommand
    /// </summary>
    public class UICommandWithParam : UICommandBase
    {
        public Action<object> action { get; private set; }

        /// <summary>
        /// Constructor for UICommand
        /// </summary>
        /// <param name="action">Action to call via Execute</param>
        public UICommandWithParam(Action<object> action)
        {
            this.action = action;
        }

        /// <summary>
        /// Runs this.action
        /// </summary>
        /// <param name="parameter">Not used</param>
        public override void Execute(object parameter)
        {
            action(parameter);
        }
    }
}
