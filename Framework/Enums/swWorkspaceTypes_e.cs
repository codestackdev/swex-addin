//**********************
//SwEx.AddIn - development tools for SOLIDWORKS add-ins
//Copyright(C) 2019 www.codestack.net
//License: https://github.com/codestackdev/swex-addin/blob/master/LICENSE
//Product URL: https://www.codestack.net/labs/solidworks/swex/add-in/
//**********************

using System;

namespace CodeStack.SwEx.AddIn.Enums
{
    /// <summary>
    /// Provides the enumeration of various workspaces in SOLIDWORKS
    /// </summary>
    [Flags]
    public enum swWorkspaceTypes_e
    {
        /// <summary>
        /// Environment when no documents are loaded (e.g. new session of SOLIDWORKS)
        /// </summary>
        NoDocuments = 1,

        /// <summary>
        /// Part document (*.sldprt)
        /// </summary>
        Part = 2 << 0,

        /// <summary>
        /// Assembly document (*.sldasm)
        /// </summary>
        Assembly = 2 << 1,

        /// <summary>
        /// Drawing document (*.slddrw)
        /// </summary>
        Drawing = 2 << 2,

        /// <summary>
        /// All SOLIDWORKS documents (*.sldprt, *.sldasm, *.slddrw)
        /// </summary>
        AllDocuments = Part | Assembly | Drawing,

        /// <summary>
        /// All environments
        /// </summary>
        All = AllDocuments | NoDocuments
    }
}
