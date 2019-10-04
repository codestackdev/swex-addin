//**********************
//SwEx.AddIn - development tools for SOLIDWORKS add-ins
//Copyright(C) 2019 www.codestack.net
//License: https://github.com/codestackdev/swex-addin/blob/master/LICENSE
//Product URL: https://www.codestack.net/labs/solidworks/swex/add-in/
//**********************

using CodeStack.SwEx.AddIn.Core;
using CodeStack.SwEx.AddIn.Delegates;
using SolidWorks.Interop.sldworks;

namespace CodeStack.SwEx.AddIn.Helpers.EventHandlers
{
    internal class DimensionChangeEventsHandler : EventsHandler<DimensionChangeDelegate>
    {
        internal DimensionChangeEventsHandler(DocumentHandler docHandler) : base(docHandler)
        {
        }

        protected override void SubscribeAssemblyEvents(AssemblyDoc assm)
        {
            assm.DimensionChangeNotify += OnDimensionChangeNotify;
        }

        protected override void SubscribeDrawingEvents(DrawingDoc draw)
        {
            draw.DimensionChangeNotify += OnDimensionChangeNotify;
        }

        protected override void SubscribePartEvents(PartDoc part)
        {
            part.DimensionChangeNotify += OnDimensionChangeNotify;
        }

        protected override void UnsubscribeAssemblyEvents(AssemblyDoc assm)
        {
            assm.DimensionChangeNotify -= OnDimensionChangeNotify;
        }

        protected override void UnsubscribeDrawingEvents(DrawingDoc draw)
        {
            draw.DimensionChangeNotify -= OnDimensionChangeNotify;
        }

        protected override void UnsubscribePartEvents(PartDoc part)
        {
            part.DimensionChangeNotify -= OnDimensionChangeNotify;
        }

        private int OnDimensionChangeNotify(object displayDim)
        {
            Delegate.Invoke(m_DocHandler, displayDim as IDisplayDimension);
            return S_OK;
        }

        protected override void OnAttach(DimensionChangeDelegate del)
        {
            Delegate += del;
        }

        protected override void OnDetach(DimensionChangeDelegate del)
        {
            Delegate -= del;
        }
    }
}
