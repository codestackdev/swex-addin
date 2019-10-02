//**********************
//SwEx.AddIn - development tools for SOLIDWORKS add-ins
//Copyright(C) 2019 www.codestack.net
//License: https://github.com/codestackdev/swex-addin/blob/master/LICENSE
//Product URL: https://www.codestack.net/labs/solidworks/swex/add-in/
//**********************

using CodeStack.SwEx.AddIn.Core;
using CodeStack.SwEx.AddIn.Enums;

namespace CodeStack.SwEx.AddIn.Delegates
{
    /// <summary>
    /// Delegate of <see cref="DocumentHandler.Save"/> event
    /// </summary>
    /// <param name="docHandler">Document Handler which sends this notification</param>
    /// <param name="fileName">Full path to save the file</param>
    /// <param name="state">Type of the save operation</param>
    /// <returns>Return false within the <see cref="SaveState_e.PreSave"/> to cancel the save operation</returns>
    public delegate bool DocumentSaveDelegate(DocumentHandler docHandler, string fileName, SaveState_e state);
}
