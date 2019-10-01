//**********************
//SwEx.AddIn - development tools for SOLIDWORKS add-ins
//Copyright(C) 2019 www.codestack.net
//License: https://github.com/codestackdev/swex-addin/blob/master/LICENSE
//Product URL: https://www.codestack.net/labs/solidworks/swex/add-in/
//**********************

using CodeStack.SwEx.AddIn.Core;
using CodeStack.SwEx.AddIn.Delegates;
using CodeStack.SwEx.AddIn.Enums;
using SolidWorks.Interop.sldworks;

namespace CodeStack.SwEx.AddIn.Helpers.EventHandlers
{
    internal class RebuildEventsHandler : EventsHandler<RebuildDelegate>
    {
        internal RebuildEventsHandler(DocumentHandler docHandler) : base(docHandler)
        {
        }

        protected override void SubscribeAssemblyEvents(AssemblyDoc assm)
        {
            assm.RegenNotify += OnRegenNotify;
            assm.RegenPostNotify2 += OnRegenPostNotify2;
        }

        protected override void SubscribeDrawingEvents(DrawingDoc draw)
        {
            draw.RegenNotify += OnRegenNotify;
            draw.RegenPostNotify += OnDrawingRegenPostNotify;
        }

        protected override void SubscribePartEvents(PartDoc part)
        {
            part.RegenNotify += OnRegenNotify;
            part.RegenPostNotify2 += OnRegenPostNotify2;
        }

        protected override void UnsubscribeAssemblyEvents(AssemblyDoc assm)
        {
            assm.RegenNotify -= OnRegenNotify;
            assm.RegenPostNotify2 -= OnRegenPostNotify2;
        }

        protected override void UnsubscribeDrawingEvents(DrawingDoc draw)
        {
            draw.RegenNotify -= OnRegenNotify;
            draw.RegenPostNotify -= OnDrawingRegenPostNotify;
        }

        protected override void UnsubscribePartEvents(PartDoc part)
        {
            part.RegenNotify -= OnRegenNotify;
            part.RegenPostNotify2 -= OnRegenPostNotify2;
        }

        private int OnRegenNotify()
        {
            return Delegate.Invoke(m_DocHandler, RebuildAction_e.PreRebuild) ? S_OK : S_FALSE;
        }

        private int OnDrawingRegenPostNotify()
        {
            return Delegate.Invoke(m_DocHandler, RebuildAction_e.PostRebuild) ? S_OK : S_FALSE;
        }

        private int OnRegenPostNotify2(object stopFeature)
        {
            return Delegate.Invoke(m_DocHandler, RebuildAction_e.PostRebuild) ? S_OK : S_FALSE;
        }

        protected override void OnAttach(RebuildDelegate del)
        {
            Delegate += del;
        }

        protected override void OnDetach(RebuildDelegate del)
        {
            Delegate -= del;
        }
    }
}
