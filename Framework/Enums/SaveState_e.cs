//**********************
//SwEx.AddIn - development tools for SOLIDWORKS add-ins
//Copyright(C) 2019 www.codestack.net
//License: https://github.com/codestackdev/swex-addin/blob/master/LICENSE
//Product URL: https://www.codestack.net/labs/solidworks/swex/add-in/
//**********************

namespace CodeStack.SwEx.AddIn.Enums
{
    /// <summary>
    /// Stage of saving the document
    /// </summary>
    public enum SaveState_e
    {
        /// <summary>
        /// Automatic save
        /// </summary>
        AutoSave,

        /// <summary>
        /// Saving document as new file
        /// </summary>
        SaveAs,

        /// <summary>
        /// Document is about to be saved
        /// </summary>
        PreSave,

        /// <summary>
        /// Document has been saved
        /// </summary>
        PostSave,

        /// <summary>
        /// After document saving has been canceled
        /// </summary>
        PostCancel
    }
}