//**********************
//SwEx.AddIn - development tools for SOLIDWORKS add-ins
//Copyright(C) 2019 www.codestack.net
//License: https://github.com/codestackdev/swex-addin/blob/master/LICENSE
//Product URL: https://www.codestack.net/labs/solidworks/swex/add-in/
//**********************

namespace CodeStack.SwEx.AddIn.Enums
{
    /// <summary>
    /// State of the object selection
    /// </summary>
    public enum SelectionState_e
    {
        /// <summary>
        /// New selection (user or via API)
        /// </summary>
        NewSelection,

        /// <summary>
        /// Use is about to select the object
        /// </summary>
        UserPreSelect,

        /// <summary>
        /// User has selected the object
        /// </summary>
        UserPostSelect,

        /// <summary>
        /// Selection is cleared
        /// </summary>
        ClearSelection
    }
}