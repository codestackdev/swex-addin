//**********************
//SwEx.AddIn - development tools for SOLIDWORKS add-ins
//Copyright(C) 2019 www.codestack.net
//License: https://github.com/codestackdev/swex-addin/blob/master/LICENSE
//Product URL: https://www.codestack.net/labs/solidworks/swex/add-in/
//**********************

namespace CodeStack.SwEx.AddIn.Enums
{
    /// <summary>
    /// Type of the modification action of the item
    /// </summary>
    public enum ItemModificationAction_e
    {
        /// <summary>
        /// New item added
        /// </summary>
        Add,

        /// <summary>
        /// Item has been deleted
        /// </summary>
        Delete,

        /// <summary>
        /// Item is about to be deleted
        /// </summary>
        PreDelete,

        /// <summary>
        /// Item has been renamed
        /// </summary>
        Rename,

        /// <summary>
        /// Item is about to be renamed
        /// </summary>
        PreRename
    }
}