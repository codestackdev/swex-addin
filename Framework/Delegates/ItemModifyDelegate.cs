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
    /// Delegate of <see cref="DocumentHandler.ItemModify"/> event
    /// </summary>
    /// <param name="docHandler">Document Handler which sends this notification</param>
    /// <param name="action">Item modification type</param>
    /// <param name="entType">Modified entity type as defined in <see href="http://help.solidworks.com/2017/english/api/swconst/SolidWorks.Interop.swconst~SolidWorks.Interop.swconst.swNotifyEntityType_e.html">swNotifyEntityType_e</see> enumeration</param>
    /// <param name="name">Name of the item</param>
    /// <param name="oldName">Old name of the item if <see cref="ItemModificationAction_e.PreRename"/> or <see cref="ItemModificationAction_e.Rename"/> operation</param>
    public delegate void ItemModifyDelegate(DocumentHandler docHandler, ItemModificationAction_e action, swNotifyEntityType_e entType, string name, string oldName = "");
}
