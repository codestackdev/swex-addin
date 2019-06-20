using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CodeStack.SwEx.AddIn.Enums
{
    /// <summary>
    /// Position of the spacer in the toolbar or menu
    /// </summary>
    /// <remarks>Used as a parameter of <see cref="Attributes.CommandSpacerAttribute"/></remarks>
    public enum CommandSpacerPosition_e
    {
        /// <summary>
        /// Add spacer before this command
        /// </summary>
        Before,

        /// <summary>
        /// Add spacer after this command
        /// </summary>
        After
    }
}
