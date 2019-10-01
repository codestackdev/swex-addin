//**********************
//SwEx.AddIn - development tools for SOLIDWORKS add-ins
//Copyright(C) 2019 www.codestack.net
//License: https://github.com/codestackdev/swex-addin/blob/master/LICENSE
//Product URL: https://www.codestack.net/labs/solidworks/swex/add-in/
//**********************

using CodeStack.SwEx.AddIn.Core;

namespace CodeStack.SwEx.AddIn.Delegates
{
    /// <summary>
    /// Delegate of <see cref="DocumentHandler.Initialized"/>, <see cref="DocumentHandler.Activated"/>, <see cref="DocumentHandler.Destroyed"/> event,
    /// </summary>
    /// <param name="docHandler">Document Handler which sends this notification</param>
    public delegate void DocumentStateChangedDelegate(DocumentHandler docHandler);
}
