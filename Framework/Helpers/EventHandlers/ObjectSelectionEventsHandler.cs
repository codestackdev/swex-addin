//**********************
//SwEx.AddIn - development tools for SOLIDWORKS add-ins
//Copyright(C) 2019 www.codestack.net
//License: https://github.com/codestackdev/swex-addin/blob/master/LICENSE
//Product URL: https://www.codestack.net/labs/solidworks/swex/add-in/
//**********************

using CodeStack.SwEx.AddIn.Delegates;
using SolidWorks.Interop.sldworks;
using CodeStack.SwEx.AddIn.Core;
using SolidWorks.Interop.swconst;
using CodeStack.SwEx.AddIn.Enums;

namespace CodeStack.SwEx.AddIn.Helpers.EventHandlers
{
    internal class ObjectSelectionEventsHandler : EventsHandler<ObjectSelectionDelegate>
    {
        public ObjectSelectionEventsHandler(DocumentHandler docHandler) : base(docHandler)
        {
        }

        protected override void OnAttach(ObjectSelectionDelegate del)
        {
            Delegate += del;
        }

        protected override void OnDetach(ObjectSelectionDelegate del)
        {
            Delegate -= del;
        }

        protected override void SubscribeAssemblyEvents(AssemblyDoc assm)
        {
            assm.ClearSelectionsNotify += OnClearSelectionsNotify;
            assm.NewSelectionNotify += OnNewSelectionNotify;
            assm.UserSelectionPreNotify += OnUserSelectionPreNotify;
            assm.UserSelectionPostNotify += OnUserSelectionPostNotify;
        }

        protected override void SubscribeDrawingEvents(DrawingDoc draw)
        {
            draw.ClearSelectionsNotify += OnClearSelectionsNotify;
            draw.NewSelectionNotify += OnNewSelectionNotify;
            draw.UserSelectionPreNotify += OnUserSelectionPreNotify;
            draw.UserSelectionPostNotify += OnUserSelectionPostNotify;
        }

        protected override void SubscribePartEvents(PartDoc part)
        {
            part.ClearSelectionsNotify += OnClearSelectionsNotify;
            part.NewSelectionNotify += OnNewSelectionNotify;
            part.UserSelectionPreNotify += OnUserSelectionPreNotify;
            part.UserSelectionPostNotify += OnUserSelectionPostNotify;
        }

        protected override void UnsubscribeAssemblyEvents(AssemblyDoc assm)
        {
            assm.ClearSelectionsNotify -= OnClearSelectionsNotify;
            assm.NewSelectionNotify -= OnNewSelectionNotify;
            assm.UserSelectionPreNotify -= OnUserSelectionPreNotify;
            assm.UserSelectionPostNotify -= OnUserSelectionPostNotify;
        }

        protected override void UnsubscribeDrawingEvents(DrawingDoc draw)
        {
            draw.ClearSelectionsNotify -= OnClearSelectionsNotify;
            draw.NewSelectionNotify -= OnNewSelectionNotify;
            draw.UserSelectionPreNotify -= OnUserSelectionPreNotify;
            draw.UserSelectionPostNotify -= OnUserSelectionPostNotify;
        }

        protected override void UnsubscribePartEvents(PartDoc part)
        {
            part.ClearSelectionsNotify -= OnClearSelectionsNotify;
            part.NewSelectionNotify -= OnNewSelectionNotify;
            part.UserSelectionPreNotify -= OnUserSelectionPreNotify;
            part.UserSelectionPostNotify -= OnUserSelectionPostNotify;
        }

        private int OnUserSelectionPostNotify()
        {
            return Delegate.Invoke(m_DocHandler, swSelectType_e.swSelNOTHING, SelectionState_e.UserPostSelect) ? S_OK : S_FALSE;
        }

        private int OnUserSelectionPreNotify(int selType)
        {
            return Delegate.Invoke(m_DocHandler, (swSelectType_e)selType, SelectionState_e.UserPreSelect) ? S_OK : S_FALSE;
        }

        private int OnNewSelectionNotify()
        {
            return Delegate.Invoke(m_DocHandler, swSelectType_e.swSelNOTHING, SelectionState_e.NewSelection) ? S_OK : S_FALSE;
        }

        private int OnClearSelectionsNotify()
        {
            return Delegate.Invoke(m_DocHandler, swSelectType_e.swSelNOTHING, SelectionState_e.ClearSelection) ? S_OK : S_FALSE;
        }
    }
}
