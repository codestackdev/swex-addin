using CodeStack.SwEx.AddIn.Base;
using SolidWorks.Interop.sldworks;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace CodeStack.SwEx.AddIn.Core
{
    public abstract class DocumentHandler : IDocumentHandler
    {
        private const int S_OK = 0;

        protected ISldWorks App { get; private set; }
        protected IModelDoc2 Model { get; private set; }

        private bool m_Is3rdPartyStreamLoaded;
        private bool m_Is3rdPartyStoreLoaded;

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public virtual void Init(ISldWorks app, IModelDoc2 model)
        {
            m_Is3rdPartyStreamLoaded = false;
            m_Is3rdPartyStoreLoaded = false;

            App = app;
            Model = model;

            if (Model is PartDoc)
            {
                (Model as PartDoc).LoadFromStorageNotify += OnLoadFromStorageNotify;
                (Model as PartDoc).LoadFromStorageStoreNotify += OnLoadFromStorageStoreNotify;
                (Model as PartDoc).SaveToStorageStoreNotify += OnSaveToStorageStoreNotify;
                (Model as PartDoc).SaveToStorageNotify += OnSaveToStorageNotify;
            }
            else if (Model is AssemblyDoc)
            {
                (Model as AssemblyDoc).LoadFromStorageNotify += OnLoadFromStorageNotify;
                (Model as AssemblyDoc).LoadFromStorageStoreNotify += OnLoadFromStorageStoreNotify;
                (Model as AssemblyDoc).SaveToStorageStoreNotify += OnSaveToStorageStoreNotify;
                (Model as AssemblyDoc).SaveToStorageNotify += OnSaveToStorageNotify;
            }
            else if (Model is DrawingDoc)
            {
                (Model as DrawingDoc).LoadFromStorageNotify += OnLoadFromStorageNotify;
                (Model as DrawingDoc).LoadFromStorageStoreNotify += OnLoadFromStorageStoreNotify;
                (Model as DrawingDoc).SaveToStorageStoreNotify += OnSaveToStorageStoreNotify;
                (Model as DrawingDoc).SaveToStorageNotify += OnSaveToStorageNotify;
            }

            //NOTE: load from storage notification is not always raised
            //it is not raised when model is loaded with assembly, it won't be also raised if the document already loaded
            //as a workaround force call loading within the idle notification
            (App as SldWorks).OnIdleNotify += OnIdleNotify;

            (App as SldWorks).ActiveModelDocChangeNotify += OnActiveModelDocChangeNotify;

            OnInit();
        }

        private int OnActiveModelDocChangeNotify()
        {
            if (App.ActiveDoc == Model)
            {
                OnActivate();
            }

            return S_OK;
        }

        private int OnIdleNotify()
        {
            EnsureLoadFromStream();
            EnsureLoadFromStorageStore();

            //only need to handle loading one time
            (App as SldWorks).OnIdleNotify -= OnIdleNotify;

            return S_OK;
        }

        public virtual void OnInit()
        {
        }

        public virtual void OnActivate()
        {
        }

        public virtual void OnLoadFromStorageStore()
        {
        }

        public virtual void OnLoadFromStream()
        {
        }

        public virtual void OnSaveToStorageStore()
        {
        }

        public virtual void OnSaveToStream()
        {
        }

        public virtual void Dispose()
        {
            (App as SldWorks).ActiveModelDocChangeNotify -= OnActiveModelDocChangeNotify;

            if (Model is PartDoc)
            {
                (Model as PartDoc).LoadFromStorageNotify -= OnLoadFromStorageNotify;
                (Model as PartDoc).LoadFromStorageStoreNotify -= OnLoadFromStorageStoreNotify;
                (Model as PartDoc).SaveToStorageStoreNotify -= OnSaveToStorageStoreNotify;
                (Model as PartDoc).SaveToStorageNotify -= OnSaveToStorageNotify;
            }
            else if (Model is AssemblyDoc)
            {
                (Model as AssemblyDoc).LoadFromStorageNotify -= OnLoadFromStorageNotify;
                (Model as AssemblyDoc).LoadFromStorageStoreNotify -= OnLoadFromStorageStoreNotify;
                (Model as AssemblyDoc).SaveToStorageStoreNotify -= OnSaveToStorageStoreNotify;
                (Model as AssemblyDoc).SaveToStorageNotify -= OnSaveToStorageNotify;
            }
            else if (Model is DrawingDoc)
            {
                (Model as DrawingDoc).LoadFromStorageNotify -= OnLoadFromStorageNotify;
                (Model as DrawingDoc).LoadFromStorageStoreNotify -= OnLoadFromStorageStoreNotify;
                (Model as DrawingDoc).SaveToStorageStoreNotify -= OnSaveToStorageStoreNotify;
                (Model as DrawingDoc).SaveToStorageNotify -= OnSaveToStorageNotify;
            }

            OnDestroy();
        }

        public virtual void OnDestroy()
        {
        }

        private int OnSaveToStorageStoreNotify()
        {
            OnSaveToStorageStore();
            return S_OK;
        }

        private int OnLoadFromStorageNotify()
        {
            //NOTE: by some reasons this event is triggered twice, adding the flag to avoid repetition
            EnsureLoadFromStream();

            return S_OK;
        }

        private int OnSaveToStorageNotify()
        {
            OnSaveToStream();
            return S_OK;
        }

        private int OnLoadFromStorageStoreNotify()
        {
            EnsureLoadFromStorageStore();

            return S_OK;
        }

        private void EnsureLoadFromStream()
        {
            if (!m_Is3rdPartyStreamLoaded)
            {
                m_Is3rdPartyStreamLoaded = true;
                OnLoadFromStream();
            }
        }

        private void EnsureLoadFromStorageStore()
        {
            if (!m_Is3rdPartyStoreLoaded)
            {
                m_Is3rdPartyStoreLoaded = true;
                OnLoadFromStorageStore();
            }
        }
    }
}
