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

namespace CodeStack.SwEx.AddIn.Helpers.EventHandlers
{
    internal class CustomPropertyModifyEventHandler : EventsHandler<CustomPropertyModifyDelegate>
    {
        private CustomPropertiesEventsHandler m_CustPrpsEventsHandler;

        public CustomPropertyModifyEventHandler(DocumentHandler docHandler) : base(docHandler)
        {
        }

        protected override void OnAttach(CustomPropertyModifyDelegate del)
        {
            Delegate += del;   
        }

        protected override void OnDetach(CustomPropertyModifyDelegate del)
        {
            Delegate -= del;
        }

        protected override void SubscribeAssemblyEvents(AssemblyDoc assm)
        {
            SubscribeCustomPropertiesEventsHandler();
        }

        protected override void SubscribeDrawingEvents(DrawingDoc draw)
        {
            SubscribeCustomPropertiesEventsHandler();
        }

        protected override void SubscribePartEvents(PartDoc part)
        {
            SubscribeCustomPropertiesEventsHandler();
        }

        protected override void UnsubscribeAssemblyEvents(AssemblyDoc assm)
        {
            UnsubscribeCustomPropertiesEventsHandler();
        }

        protected override void UnsubscribeDrawingEvents(DrawingDoc draw)
        {
            UnsubscribeCustomPropertiesEventsHandler();
        }

        protected override void UnsubscribePartEvents(PartDoc part)
        {
            UnsubscribeCustomPropertiesEventsHandler();
        }

        private void SubscribeCustomPropertiesEventsHandler()
        {
            m_CustPrpsEventsHandler = new CustomPropertiesEventsHandler(m_DocHandler, m_DocHandler.App, m_DocHandler.Model);
            m_CustPrpsEventsHandler.CustomPropertiesModified += OnCustomPropertiesPropertyModified;
        }

        private void UnsubscribeCustomPropertiesEventsHandler()
        {
            m_CustPrpsEventsHandler.CustomPropertiesModified -= OnCustomPropertiesPropertyModified;
            m_CustPrpsEventsHandler.Dispose();
            m_CustPrpsEventsHandler = null;
        }

        private void OnCustomPropertiesPropertyModified(DocumentHandler docHandler, CustomPropertyModifyData[] modifications)
        {
            Delegate.Invoke(docHandler, modifications);
        }
    }
}
