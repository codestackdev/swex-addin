using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeStack.Dev.Sw.AddIn.Enums
{
    public enum CommandItemEnableState_e
    {
        //Deselects and disables the item
        DeselectDisable = 0,

        //Deselects and enables the item; this is the default state if no update function is specified
        DeselectEnable = 1,

        //Selects and disables the item
        SelectDisable = 2,

        //Selects and enables the item 
        SelectEnable = 3
    }
}
