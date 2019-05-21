//**********************
//SwEx.AddIn - development tools for SOLIDWORKS add-ins
//Copyright(C) 2019 www.codestack.net
//License: https://github.com/codestack-net-dev/sw-dev-tools-addin/blob/master/LICENSE
//Product URL: https://www.codestack.net/labs/solidworks/swex/add-in/
//**********************

using CodeStack.SwEx.AddIn.Base;
using SolidWorks.Interop.sldworks;
using System.ComponentModel;

namespace CodeStack.SwEx.AddIn.Core
{
    /// <summary>
    /// Specific implementation of document handler which provides overrides for the document lifecycle events
    /// </summary>
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

        /// <summary>
        /// Override to handle the initialization of the document
        /// </summary>
        /// <remarks>This method is called when document is opened or loaded with assembly or drawing</remarks>
        public virtual void OnInit()
        {
        }

        /// <summary>
        /// Override to handle the activation of the document (when it is opened in its own window)
        /// </summary>
        public virtual void OnActivate()
        {
        }

        /// <summary>
        /// Override to read the data from the third party storage via <see cref="ModelDocExtension.Access3rdPartyStorageStore(IModelDoc2, string, bool)"/> method
        /// </summary>
        public virtual void OnLoadFromStorageStore()
        {
        }

        /// <summary>
        /// Override to read the data from the 3rd party stream via <see cref="ModelDocExtension.Access3rdPartyStream(IModelDoc2, string, bool)"/>
        /// </summary>
        public virtual void OnLoadFromStream()
        {
        }

        /// <summary>
        /// Override to save the data from the third party storage via <see cref="ModelDocExtension.Access3rdPartyStorageStore(IModelDoc2, string, bool)"/> method
        /// </summary>
        public virtual void OnSaveToStorageStore()
        {
        }

        /// <summary>
        /// Override to save the data from the 3rd party stream via <see cref="ModelDocExtension.Access3rdPartyStream(IModelDoc2, string, bool)"/>
        /// </summary>
        public virtual void OnSaveToStream()
        {
        }
        
        public void Dispose()
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

        /// <summary>
        /// Override to dispose the resources
        /// </summary>
        /// <remarks>Invoked when document has been destroyed</remarks>
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
