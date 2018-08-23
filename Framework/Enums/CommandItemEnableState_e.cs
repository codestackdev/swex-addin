//**********************
//SwEx.AddIn - development tools for SOLIDWORKS add-ins
//Copyright(C) 2018 www.codestack.net
//License: https://github.com/codestack-net-dev/sw-dev-tools-addin/blob/master/LICENSE
//Product URL: https://www.codestack.net/labs/solidworks/dev-tools-addin/
//**********************

namespace CodeStack.SwEx.AddIn.Enums
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
