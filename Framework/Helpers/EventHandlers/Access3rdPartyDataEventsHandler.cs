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
    internal class Access3rdPartyDataEventsHandler : EventsHandler<Access3rdPartyDataDelegate>
    {
        private bool m_Is3rdPartyStreamLoaded;
        private bool m_Is3rdPartyStoreLoaded;

        public Access3rdPartyDataEventsHandler(DocumentHandler docHandler) : base(docHandler)
        {
            m_Is3rdPartyStreamLoaded = false;
            m_Is3rdPartyStoreLoaded = false;
        }

        protected override void OnAttach(Access3rdPartyDataDelegate del)
        {
            Delegate += del;
        }

        protected override void OnDetach(Access3rdPartyDataDelegate del)
        {
            Delegate -= del;
        }

        protected override void SubscribeAssemblyEvents(AssemblyDoc assm)
        {
            assm.LoadFromStorageNotify += OnLoadFromStorageNotify;
            assm.LoadFromStorageStoreNotify += OnLoadFromStorageStoreNotify;
            assm.SaveToStorageStoreNotify += OnSaveToStorageStoreNotify;
            assm.SaveToStorageNotify += OnSaveToStorageNotify;

            SubscribeIdleEvent();
        }

        protected override void SubscribeDrawingEvents(DrawingDoc draw)
        {
            draw.LoadFromStorageNotify += OnLoadFromStorageNotify;
            draw.LoadFromStorageStoreNotify += OnLoadFromStorageStoreNotify;
            draw.SaveToStorageStoreNotify += OnSaveToStorageStoreNotify;
            draw.SaveToStorageNotify += OnSaveToStorageNotify;

            SubscribeIdleEvent();
        }

        protected override void SubscribePartEvents(PartDoc part)
        {
            part.LoadFromStorageNotify += OnLoadFromStorageNotify;
            part.LoadFromStorageStoreNotify += OnLoadFromStorageStoreNotify;
            part.SaveToStorageStoreNotify += OnSaveToStorageStoreNotify;
            part.SaveToStorageNotify += OnSaveToStorageNotify;

            SubscribeIdleEvent();
        }

        protected override void UnsubscribeAssemblyEvents(AssemblyDoc assm)
        {
            assm.LoadFromStorageNotify -= OnLoadFromStorageNotify;
            assm.LoadFromStorageStoreNotify -= OnLoadFromStorageStoreNotify;
            assm.SaveToStorageStoreNotify -= OnSaveToStorageStoreNotify;
            assm.SaveToStorageNotify -= OnSaveToStorageNotify;
        }

        protected override void UnsubscribeDrawingEvents(DrawingDoc draw)
        {
            draw.LoadFromStorageNotify -= OnLoadFromStorageNotify;
            draw.LoadFromStorageStoreNotify -= OnLoadFromStorageStoreNotify;
            draw.SaveToStorageStoreNotify -= OnSaveToStorageStoreNotify;
            draw.SaveToStorageNotify -= OnSaveToStorageNotify;
        }

        protected override void UnsubscribePartEvents(PartDoc part)
        {
            part.LoadFromStorageNotify -= OnLoadFromStorageNotify;
            part.LoadFromStorageStoreNotify -= OnLoadFromStorageStoreNotify;
            part.SaveToStorageStoreNotify -= OnSaveToStorageStoreNotify;
            part.SaveToStorageNotify -= OnSaveToStorageNotify;
        }

        private void SubscribeIdleEvent()
        {
            //NOTE: load from storage notification is not always raised
            //it is not raised when model is loaded with assembly, it won't be also raised if the document already loaded
            //as a workaround force call loading within the idle notification
            (m_DocHandler.App as SldWorks).OnIdleNotify += OnIdleHandleThirdPartyStorageNotify;
        }

        private int OnSaveToStorageStoreNotify()
        {
            Delegate.Invoke(m_DocHandler, Access3rdPartyDataState_e.StorageWrite);
            return S_OK;
        }

        private int OnLoadFromStorageNotify()
        {
            //NOTE: by some reasons this event is triggered twice, adding the flag to avoid repetition
            return EnsureLoadFromStream();
        }

        private int OnSaveToStorageNotify()
        {
            Delegate.Invoke(m_DocHandler, Access3rdPartyDataState_e.StreamWrite);

            return S_OK;
        }

        private int OnLoadFromStorageStoreNotify()
        {
            return EnsureLoadFromStorageStore();
        }

        private int OnIdleHandleThirdPartyStorageNotify()
        {
            EnsureLoadFromStream();
            EnsureLoadFromStorageStore();

            //only need to handle loading one time
            (m_DocHandler.App as SldWorks).OnIdleNotify -= OnIdleHandleThirdPartyStorageNotify;

            return S_OK;
        }

        private int EnsureLoadFromStream()
        {
            if (!m_Is3rdPartyStreamLoaded)
            {
                m_Is3rdPartyStreamLoaded = true;
                Delegate.Invoke(m_DocHandler, Access3rdPartyDataState_e.StreamRead);
            }

            return S_OK;
        }

        private int EnsureLoadFromStorageStore()
        {
            if (!m_Is3rdPartyStoreLoaded)
            {
                m_Is3rdPartyStoreLoaded = true;
                Delegate.Invoke(m_DocHandler, Access3rdPartyDataState_e.StorageRead);
            }

            return S_OK;
        }
    }
}
