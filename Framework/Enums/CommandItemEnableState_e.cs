using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeStack.Dev.Sw.AddIn.Enums
{
    public enum CommandItemEnableState_e
    {
        /// <summary>
        /// Deselects and disables the item
        /// </summary>
        DeselectDisable = 0,

        /// <summary>
        /// Deselects and enables the item; this is the default state if no update function is specified
        /// </summary>
        DeselectEnable = 1,

        /// <summary>
        /// Selects and disables the item
        /// </summary>
        SelectDisable = 2,

        /// <summary>
        /// Selects and enables the item 
        /// </summary>
        SelectEnable = 3
    }
}
