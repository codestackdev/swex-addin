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
    public delegate bool ObjectSelectionDelegate(DocumentHandler docHandler, swSelectType_e selType, SelectionAction_e type);
}
