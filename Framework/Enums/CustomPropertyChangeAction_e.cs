//**********************
//SwEx.AddIn - development tools for SOLIDWORKS add-ins
//Copyright(C) 2019 www.codestack.net
//License: https://github.com/codestackdev/swex-addin/blob/master/LICENSE
//Product URL: https://www.codestack.net/labs/solidworks/swex/add-in/
//**********************

namespace CodeStack.SwEx.AddIn.Enums
{
    /// <summary>
    /// Type of the modification action on custom properties
    /// </summary>
    public enum CustomPropertyChangeAction_e
    {
        /// <summary>
        /// New custom property is added
        /// </summary>
        Add,

        /// <summary>
        /// Custom property is removed
        /// </summary>
        Delete,

        /// <summary>
        /// Custom property value is changed
        /// </summary>
        Modify
    }
}