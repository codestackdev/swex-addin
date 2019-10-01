//**********************
//SwEx.AddIn - development tools for SOLIDWORKS add-ins
//Copyright(C) 2019 www.codestack.net
//License: https://github.com/codestackdev/swex-addin/blob/master/LICENSE
//Product URL: https://www.codestack.net/labs/solidworks/swex/add-in/
//**********************

using CodeStack.SwEx.AddIn.Delegates;
using SolidWorks.Interop.sldworks;
using CodeStack.SwEx.AddIn.Core;
using CodeStack.SwEx.AddIn.Enums;
using SolidWorks.Interop.swconst;

namespace CodeStack.SwEx.AddIn.Helpers.EventHandlers
{
    internal class ItemModifyEventsHandler : EventsHandler<ItemModifyDelegate>
    {
        public ItemModifyEventsHandler(DocumentHandler docHandler) : base(docHandler)
        {
        }

        protected override void OnAttach(ItemModifyDelegate del)
        {
            Delegate += del;
        }

        protected override void OnDetach(ItemModifyDelegate del)
        {
            Delegate -= del;
        }

        protected override void SubscribeAssemblyEvents(AssemblyDoc assm)
        {
            assm.AddItemNotify += OnAddItemNotify;
            assm.DeleteItemNotify += OnDeleteItemNotify;
            assm.DeleteItemPreNotify += OnDeleteItemPreNotify;
            assm.PreRenameItemNotify += OnPreRenameItemNotify;
            assm.RenameItemNotify += OnRenameItemNotify;
        }

        protected override void SubscribeDrawingEvents(DrawingDoc draw)
        {
            draw.AddItemNotify += OnAddItemNotify;
            draw.DeleteItemNotify += OnDeleteItemNotify;
            draw.DeleteItemPreNotify += OnDeleteItemPreNotify;
            draw.RenameItemNotify += OnRenameItemNotify;
        }

        protected override void SubscribePartEvents(PartDoc part)
        {
            part.AddItemNotify += OnAddItemNotify;
            part.DeleteItemNotify += OnDeleteItemNotify;
            part.DeleteItemPreNotify += OnDeleteItemPreNotify;
            part.PreRenameItemNotify += OnPreRenameItemNotify;
            part.RenameItemNotify += OnRenameItemNotify;
        }

        protected override void UnsubscribeAssemblyEvents(AssemblyDoc assm)
        {
            assm.AddItemNotify -= OnAddItemNotify;
            assm.DeleteItemNotify -= OnDeleteItemNotify;
            assm.DeleteItemPreNotify -= OnDeleteItemPreNotify;
            assm.PreRenameItemNotify -= OnPreRenameItemNotify;
            assm.RenameItemNotify -= OnRenameItemNotify;
        }

        protected override void UnsubscribeDrawingEvents(DrawingDoc draw)
        {
            draw.AddItemNotify -= OnAddItemNotify;
            draw.DeleteItemNotify -= OnDeleteItemNotify;
            draw.DeleteItemPreNotify -= OnDeleteItemPreNotify;
            draw.RenameItemNotify -= OnRenameItemNotify;
        }

        protected override void UnsubscribePartEvents(PartDoc part)
        {
            part.AddItemNotify -= OnAddItemNotify;
            part.DeleteItemNotify -= OnDeleteItemNotify;
            part.DeleteItemPreNotify -= OnDeleteItemPreNotify;
            part.PreRenameItemNotify -= OnPreRenameItemNotify;
            part.RenameItemNotify -= OnRenameItemNotify;
        }

        private int OnRenameItemNotify(int entityType, string oldName, string newName)
        {
            Delegate.Invoke(m_DocHandler, ItemModificationAction_e.Rename,
                (swNotifyEntityType_e)entityType, newName, oldName);

            return S_OK;
        }

        private int OnPreRenameItemNotify(int entityType, string oldName, string newName)
        {
            Delegate.Invoke(m_DocHandler, ItemModificationAction_e.PreRename,
                (swNotifyEntityType_e)entityType, newName, oldName);

            return S_OK;
        }

        private int OnDeleteItemPreNotify(int entityType, string itemName)
        {
            Delegate.Invoke(m_DocHandler, ItemModificationAction_e.PreDelete,
                (swNotifyEntityType_e)entityType, itemName);

            return S_OK;
        }

        private int OnDeleteItemNotify(int entityType, string itemName)
        {
            Delegate.Invoke(m_DocHandler, ItemModificationAction_e.Delete,
                (swNotifyEntityType_e)entityType, itemName);

            return S_OK;
        }

        private int OnAddItemNotify(int entityType, string itemName)
        {
            Delegate.Invoke(m_DocHandler, ItemModificationAction_e.Add,
                (swNotifyEntityType_e)entityType, itemName);

            return S_OK;
        }
    }
}
