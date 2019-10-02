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
    /// Delegate of <see cref="DocumentHandler.Access3rdPartyData"/> event
    /// </summary>
    /// <param name="docHandler">Document Handler which sends this notification</param>
    /// <param name="state">Type of the 3rd party storage access</param>
    public delegate void Access3rdPartyDataDelegate(DocumentHandler docHandler, Access3rdPartyDataState_e state);
}
