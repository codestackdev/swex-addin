using CodeStack.SwEx.AddIn.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CodeStack.SwEx.AddIn.Attributes
{
    /// <summary>
    /// Marks the command to be separated by the spacer in the menu and the toolbar
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class CommandSpacerAttribute : Attribute
    {
        internal CommandSpacerPosition_e Position { get; private set; }

        /// <summary>
        /// Create spacer with position
        /// </summary>
        /// <param name="pos">Position of the spacer</param>
        public CommandSpacerAttribute(CommandSpacerPosition_e pos = CommandSpacerPosition_e.Before)
        {
            Position = pos;
        }
    }
}
