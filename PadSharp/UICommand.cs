﻿using System;

namespace PadSharp
{
    /// <summary>
    /// Barebones implementation of ICommand
    /// </summary>
    public class UICommand : UICommandBase
    {
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
        public override void Execute(object parameter)
        {
            action();
        }
    }
}
