//**********************
//SwEx.AddIn - development tools for SOLIDWORKS add-ins
//Copyright(C) 2019 www.codestack.net
//License: https://github.com/codestackdev/swex-addin/blob/master/LICENSE
//Product URL: https://www.codestack.net/labs/solidworks/swex/add-in/
//**********************

namespace CodeStack.SwEx.AddIn.Enums
{
    /// <summary>
    /// State of the rebuild operation
    /// </summary>
    public enum RebuildState_e
    {
        /// <summary>
        /// Document is about to be rebuilt
        /// </summary>
        PreRebuild,

        /// <summary>
        /// Document has been rebuilt
        /// </summary>
        PostRebuild
    }
}