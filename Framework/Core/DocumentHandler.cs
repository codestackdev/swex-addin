//**********************
//SwEx.AddIn - development tools for SOLIDWORKS add-ins
//Copyright(C) 2019 www.codestack.net
//License: https://github.com/codestackdev/swex-addin/blob/master/LICENSE
//Product URL: https://www.codestack.net/labs/solidworks/swex/add-in/
//**********************

using CodeStack.SwEx.AddIn.Base;
using CodeStack.SwEx.AddIn.Delegates;
using CodeStack.SwEx.AddIn.Enums;
using CodeStack.SwEx.AddIn.Helpers;
using CodeStack.SwEx.AddIn.Helpers.EventHandlers;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace CodeStack.SwEx.AddIn.Core
{
    /// <summary>
    /// Specific implementation of document handler which exposes document lifecycle events
    /// </summary>
    public class DocumentHandler : IDocumentHandler
    {
        private readonly RebuildEventsHandler m_RebuildEventsHandler;
        private readonly ObjectSelectionEventsHandler m_ObjectSelectionEventsHandler;
        private readonly ItemModifyEventsHandler m_ItemModifyEventsHandler;
        private readonly DocumentSaveEventsHandler m_DocumentSaveEventsHandler;
        private readonly CustomPropertyModifyEventHandler m_CustomPropertyModifyEventHandler;
        private readonly ConfigurationChangeEventsHandler m_ConfigurationChangeEventsHandler;
        private readonly Access3rdPartyDataEventsHandler m_Access3rdPartyDataEventsHandler;

        public event DocumentStateChangedDelegate Initialized;
        public event DocumentStateChangedDelegate Activated;
        public event DocumentStateChangedDelegate Destroyed;

        /// <summary>
        /// Raised when document is saving or saved (including auto saving)
        /// </summary>
        public event DocumentSaveDelegate Save
        {
            add
            {
                m_DocumentSaveEventsHandler.Attach(value);
            }
            remove
            {
                m_DocumentSaveEventsHandler.Detach(value);
            }
        }

        /// <summary>
        /// Raised when object is selected in SOLIDWORKS (either by the user or API)
        /// </summary>
        public event ObjectSelectionDelegate Selection
        {
            add
            {
                m_ObjectSelectionEventsHandler.Attach(value);
            }
            remove
            {
                m_ObjectSelectionEventsHandler.Detach(value);
            }
        }

        /// <summary>
        /// Raised when 3rd party storage and stream are ready for access (reading or writing)
        /// </summary>
        public event Access3rdPartyDataDelegate Access3rdPartyData
        {
            add
            {
                m_Access3rdPartyDataEventsHandler.Attach(value);
            }
            remove
            {
                m_Access3rdPartyDataEventsHandler.Detach(value);
            }
        }

        /// <summary>
        /// Raised when custom properties modified (added, removed or changed) from the UI or API
        /// </summary>
        public event CustomPropertyModifyDelegate CustomPropertyModify
        {
            add
            {
                m_CustomPropertyModifyEventHandler.Attach(value);
            }
            remove
            {
                m_CustomPropertyModifyEventHandler.Detach(value);
            }
        }
        
        /// <summary>
        /// Raised when item (e.g. feature, configuration) is modified in the Feature Manager Tree (e.g. renamed, added or removed)
        /// </summary>
        public event ItemModifyDelegate ItemModify
        {
            add
            {
                m_ItemModifyEventsHandler.Attach(value);
            }
            remove
            {
                m_ItemModifyEventsHandler.Detach(value);
            }
        }

        /// <summary>
        /// Raised when configuration is changed in part or assembly or sheet is activated in the drawing
        /// </summary>
        public event ConfigurationChangeDelegate ConfigurationChange
        {
            add
            {
                m_ConfigurationChangeEventsHandler.Attach(value);
            }
            remove
            {
                m_ConfigurationChangeEventsHandler.Detach(value);
            }
        }

        /// <summary>
        /// Raised when model is regenerated either as force regeneration or parametric regeneration or after rollback
        /// </summary>
        public event RebuildDelegate Rebuild
        {
            add
            {
                m_RebuildEventsHandler.Attach(value);
            }
            remove
            {
                m_RebuildEventsHandler.Detach(value);
            }
        }

        /// <summary>
        /// Pointer to the SOLIDWORKS application
        /// </summary>
        /// <remarks>This pointer assigned before <see cref="OnInit"/> method or <see cref="Initialized"/> event</remarks>
        public ISldWorks App { get; private set; }

        /// <summary>
        /// Pointer to the model of this handler
        /// </summary>
        /// <remarks>This pointer assigned before <see cref="OnInit"/> method or <see cref="Initialized"/> event</remarks>
        public IModelDoc2 Model { get; private set; }
        
        public DocumentHandler()
        {
            m_RebuildEventsHandler = new RebuildEventsHandler(this);
            m_ObjectSelectionEventsHandler = new ObjectSelectionEventsHandler(this);
            m_ItemModifyEventsHandler = new ItemModifyEventsHandler(this);
            m_DocumentSaveEventsHandler = new DocumentSaveEventsHandler(this);
            m_CustomPropertyModifyEventHandler = new CustomPropertyModifyEventHandler(this);
            m_ConfigurationChangeEventsHandler = new ConfigurationChangeEventsHandler(this);
            m_Access3rdPartyDataEventsHandler = new Access3rdPartyDataEventsHandler(this);
        }

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public virtual void Init(ISldWorks app, IModelDoc2 model)
        {
            App = app;
            Model = model;

            //TODO: remove this once the obsolete methods are removed
            this.Access3rdPartyData += OnAccess3rdPartyData;
            //---

            (App as SldWorks).ActiveModelDocChangeNotify += OnActiveModelDocChangeNotify;

            Initialized?.Invoke(this);
            OnInit();
        }

        //TODO: remove this once the obsolete methods are removed
        private void OnAccess3rdPartyData(DocumentHandler docHandler, Access3rdPartyDataState_e type)
        {
            switch (type)
            {
#pragma warning disable CS0618
                case Access3rdPartyDataState_e.StorageRead:
                    OnLoadFromStorageStore();
                    break;

                case Access3rdPartyDataState_e.StorageWrite:
                    OnSaveToStorageStore();
                    break;

                case Access3rdPartyDataState_e.StreamRead:
                    OnLoadFromStream();
                    break;

                case Access3rdPartyDataState_e.StreamWrite:
                    OnSaveToStream();
                    break;
#pragma warning restore CS0618
            }
        }
        //---

        private int OnActiveModelDocChangeNotify()
        {
            const int S_OK = 0;
            
            if (App.ActiveDoc == Model)
            {
                Activated?.Invoke(this);
                OnActivate();
            }

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

        [Obsolete("Deprecated. Use Access3rdPartyData event with StorageRead type instead")]
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public virtual void OnLoadFromStorageStore()
        {
        }

        [Obsolete("Deprecated. Use Access3rdPartyData event with StreamRead type instead")]
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public virtual void OnLoadFromStream()
        {
        }

        [Obsolete("Deprecated. Use Access3rdPartyData event with StorageWrite type instead")]
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public virtual void OnSaveToStorageStore()
        {
        }

        [Obsolete("Deprecated. Use Access3rdPartyData event with StreamWrite type instead")]
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public virtual void OnSaveToStream()
        {
        }
        
        public void Dispose()
        {
            (App as SldWorks).ActiveModelDocChangeNotify -= OnActiveModelDocChangeNotify;

            //TODO: remove this once the obsolete methods are removed
            this.Access3rdPartyData -= OnAccess3rdPartyData;
            //---

            m_DocumentSaveEventsHandler.Dispose();
            m_ObjectSelectionEventsHandler.Dispose();
            m_Access3rdPartyDataEventsHandler.Dispose();
            m_CustomPropertyModifyEventHandler.Dispose();
            m_ItemModifyEventsHandler.Dispose();
            m_ConfigurationChangeEventsHandler.Dispose();
            m_RebuildEventsHandler.Dispose();

            Destroyed?.Invoke(this);
            OnDestroy();
        }
        
        /// <summary>
        /// Override to dispose the resources
        /// </summary>
        /// <remarks>Invoked when document has been destroyed</remarks>
        public virtual void OnDestroy()
        {
        }        
    }
}
