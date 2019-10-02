//**********************
//SwEx.AddIn - development tools for SOLIDWORKS add-ins
//Copyright(C) 2019 www.codestack.net
//License: https://github.com/codestackdev/swex-addin/blob/master/LICENSE
//Product URL: https://www.codestack.net/labs/solidworks/swex/add-in/
//**********************

using CodeStack.SwEx.AddIn.Core;
using CodeStack.SwEx.AddIn.Enums;
using SolidWorks.Interop.swconst;

namespace CodeStack.SwEx.AddIn.Delegates
{
    /// <summary>
    /// Delegate of <see cref="DocumentHandler.Selection"/> event
    /// </summary>
    /// <param name="docHandler">Document Handler which sends this notification</param>
    /// <param name="selType">Type of the selected object as defined in <see href="http://help.solidworks.com/2014/english/api/swconst/SolidWorks.Interop.swconst~SolidWorks.Interop.swconst.swSelectType_e.html">swSelectType_e</see> enumeration</param>
    /// <param name="state">Type of the selection operation</param>
    /// <returns>Return false if <see cref="SelectionState_e.UserPreSelect"/> to cancel the user selection</returns>
    public delegate bool ObjectSelectionDelegate(DocumentHandler docHandler, swSelectType_e selType, SelectionState_e state);
}
