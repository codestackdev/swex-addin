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
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace CodeStack.SwEx.AddIn.Core
{
    /// <summary>
    /// Specific implementation of document handler which provides overrides for the document lifecycle events
    /// </summary>
    public class DocumentHandler : IDocumentHandler
    {
        private const int S_OK = 0;
        private const int S_FALSE = 1;

        private DocumentSaveDelegate m_SaveDelegate;
        private bool m_SaveSubscribed;

        private ObjectSelectionDelegate m_SelectionDelegate;
        private bool m_SelectionSubscribed;

        private Access3rdPartyDataDelegate m_Access3rdPartyDataDelegate;
        private bool m_Access3rdPartyDataSubscribed;

        private ItemModifyDelegate m_ItemModifyDelegate;
        private bool m_ItemModifySubscribed;

        private ConfigurationChangeDelegate m_ConfigurationChangeDelegate;
        private bool m_ConfigurationChangeSubscribed;

        private CustomPropertyModifyDelegate m_DocumentCustomPropertyChangeDelegate;
        private bool m_DocumentCustomPropertyChangeSubscirbed;
        private CustomPropertiesEventsHandler m_CustPrpsEventsHandler;

        public event DocumentStateChangedDelegate Initialized;
        public event DocumentStateChangedDelegate Activated;
        public event DocumentStateChangedDelegate Destroyed;

        public event DocumentSaveDelegate Save
        {
            add
            {
                if (!m_SaveSubscribed)
                {
                    m_SaveSubscribed = true;

                    if (Model is PartDoc)
                    {
                        (Model as PartDoc).AutoSaveNotify += OnAutoSaveNotify;
                        (Model as PartDoc).FileSaveAsNotify2 += OnFileSaveAsNotify2;
                        (Model as PartDoc).FileSaveNotify += OnFileSaveNotify;
                        (Model as PartDoc).FileSavePostCancelNotify += OnFileSavePostCancelNotify;
                        (Model as PartDoc).FileSavePostNotify += OnFileSavePostNotify;
                    }
                    else if (Model is AssemblyDoc)
                    {
                        (Model as AssemblyDoc).AutoSaveNotify += OnAutoSaveNotify;
                        (Model as AssemblyDoc).FileSaveAsNotify2 += OnFileSaveAsNotify2;
                        (Model as AssemblyDoc).FileSaveNotify += OnFileSaveNotify;
                        (Model as AssemblyDoc).FileSavePostCancelNotify += OnFileSavePostCancelNotify;
                        (Model as AssemblyDoc).FileSavePostNotify += OnFileSavePostNotify;
                    }
                    else if (Model is DrawingDoc)
                    {
                        (Model as DrawingDoc).AutoSaveNotify += OnAutoSaveNotify;
                        (Model as DrawingDoc).FileSaveAsNotify2 += OnFileSaveAsNotify2;
                        (Model as DrawingDoc).FileSaveNotify += OnFileSaveNotify;
                        (Model as DrawingDoc).FileSavePostCancelNotify += OnFileSavePostCancelNotify;
                        (Model as DrawingDoc).FileSavePostNotify += OnFileSavePostNotify;
                    }
                }

                m_SaveDelegate += value;
            }
            remove
            {
                m_SaveDelegate -= value;

                if (m_SaveDelegate == null)
                {
                    UnsubscribeSaveEvents();
                }
            }
        }

        public event ObjectSelectionDelegate Selection
        {
            add
            {
                if (!m_SelectionSubscribed)
                {
                    m_SelectionSubscribed = true;

                    if (Model is PartDoc)
                    {
                        (Model as PartDoc).ClearSelectionsNotify += OnClearSelectionsNotify;
                        (Model as PartDoc).NewSelectionNotify += OnNewSelectionNotify;
                        (Model as PartDoc).UserSelectionPreNotify += OnUserSelectionPreNotify;
                        (Model as PartDoc).UserSelectionPostNotify += OnUserSelectionPostNotify;
                    }
                    else if (Model is AssemblyDoc)
                    {
                        (Model as AssemblyDoc).ClearSelectionsNotify += OnClearSelectionsNotify;
                        (Model as AssemblyDoc).NewSelectionNotify += OnNewSelectionNotify;
                        (Model as AssemblyDoc).UserSelectionPreNotify += OnUserSelectionPreNotify;
                        (Model as AssemblyDoc).UserSelectionPostNotify += OnUserSelectionPostNotify;
                    }
                    else if (Model is DrawingDoc)
                    {
                        (Model as DrawingDoc).ClearSelectionsNotify += OnClearSelectionsNotify;
                        (Model as DrawingDoc).NewSelectionNotify += OnNewSelectionNotify;
                        (Model as DrawingDoc).UserSelectionPreNotify += OnUserSelectionPreNotify;
                        (Model as DrawingDoc).UserSelectionPostNotify += OnUserSelectionPostNotify;
                    }
                }

                m_SelectionDelegate += value;
            }
            remove
            {
                m_SelectionDelegate -= value;

                if (m_SelectionDelegate == null)
                {
                    UnsubscribeSelectionEvents();
                }
            }
        }

        public event Access3rdPartyDataDelegate Access3rdPartyData
        {
            add
            {
                if (!m_Access3rdPartyDataSubscribed)
                {
                    m_Access3rdPartyDataSubscribed = true;

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
                    (App as SldWorks).OnIdleNotify += OnIdleHandleThirdPartyStorageNotify;
                }

                m_Access3rdPartyDataDelegate += value;
            }
            remove
            {
                m_Access3rdPartyDataDelegate -= value;

                if (m_Access3rdPartyDataDelegate == null)
                {
                    Unsubscribe3rdPartyDataAccessEvents();
                }
            }
        }

        public event CustomPropertyModifyDelegate CustomPropertyModify
        {
            add
            {
                if (!m_DocumentCustomPropertyChangeSubscirbed)
                {
                    m_DocumentCustomPropertyChangeSubscirbed = true;

                    m_CustPrpsEventsHandler = new CustomPropertiesEventsHandler(this, App, Model);
                    m_CustPrpsEventsHandler.PropertyChanged += OnCustomPropertiesPropertyModified;
                }

                m_DocumentCustomPropertyChangeDelegate += value;
            }
            remove
            {
                m_DocumentCustomPropertyChangeDelegate -= value;

                if (m_DocumentCustomPropertyChangeDelegate == null)
                {
                    UnsubscribeCustomPropertyChangedEvents();
                }
            }
        }
        
        public event ItemModifyDelegate ItemModify
        {
            add
            {
                if (!m_ItemModifySubscribed)
                {
                    m_ItemModifySubscribed = true;

                    if (Model is PartDoc)
                    {
                        (Model as PartDoc).AddItemNotify += OnAddItemNotify;
                        (Model as PartDoc).DeleteItemNotify += OnDeleteItemNotify;
                        (Model as PartDoc).DeleteItemPreNotify += OnDeleteItemPreNotify;
                        (Model as PartDoc).PreRenameItemNotify += OnPreRenameItemNotify;
                        (Model as PartDoc).RenameItemNotify += OnRenameItemNotify;
                    }
                    else if (Model is AssemblyDoc)
                    {
                        (Model as AssemblyDoc).AddItemNotify += OnAddItemNotify;
                        (Model as AssemblyDoc).DeleteItemNotify += OnDeleteItemNotify;
                        (Model as AssemblyDoc).DeleteItemPreNotify += OnDeleteItemPreNotify;
                        (Model as AssemblyDoc).PreRenameItemNotify += OnPreRenameItemNotify;
                        (Model as AssemblyDoc).RenameItemNotify += OnRenameItemNotify;
                    }
                    else if (Model is DrawingDoc)
                    {
                        (Model as DrawingDoc).AddItemNotify += OnAddItemNotify;
                        (Model as DrawingDoc).DeleteItemNotify += OnDeleteItemNotify;
                        (Model as DrawingDoc).DeleteItemPreNotify += OnDeleteItemPreNotify;
                        (Model as DrawingDoc).RenameItemNotify += OnRenameItemNotify;
                    }
                }

                m_ItemModifyDelegate += value;
            }
            remove
            {
                m_ItemModifyDelegate -= value;

                if (m_ItemModifyDelegate == null)
                {
                    UnsubscribeItemModifiedEvents();
                }
            }
        }

        public event ConfigurationChangeDelegate ConfigurationChange
        {
            add
            {
                if (!m_ConfigurationChangeSubscribed)
                {
                    m_ConfigurationChangeSubscribed = true;

                    if (Model is PartDoc)
                    {
                        (Model as PartDoc).ConfigurationChangeNotify += OnConfigurationChangeNotify;
                    }
                    else if (Model is AssemblyDoc)
                    {
                        (Model as AssemblyDoc).ConfigurationChangeNotify += OnConfigurationChangeNotify;
                    }
                    else if (Model is DrawingDoc)
                    {
                        (Model as DrawingDoc).ActivateSheetPreNotify += OnActivateSheetPreNotify;
                        (Model as DrawingDoc).ActivateSheetPostNotify += OnActivateSheetPostNotify;
                    }
                }

                m_ConfigurationChangeDelegate += value;
            }
            remove
            {
                m_ConfigurationChangeDelegate -= value;

                if (m_ConfigurationChangeDelegate == null)
                {
                    
                }
            }
        }

        private abstract class EventsHandler<TDel> : IDisposable
            where TDel : class, ICloneable, ISerializable
        {
            protected const int S_OK = 0;
            protected const int S_FALSE = 1;

            public TDel Delegate { get; set; }

            protected readonly DocumentHandler m_DocHandler;
            
            private bool m_IsSubscribed;

            internal EventsHandler(DocumentHandler docHandler)
            {
                m_DocHandler = docHandler;

                m_IsSubscribed = false;
            }

            private void SubscribeIfNeeded()
            {
                if (!m_IsSubscribed)
                {
                    if (m_DocHandler.Model is PartDoc)
                    {
                        SubscribePartEvents(m_DocHandler.Model as PartDoc);
                    }
                    else if (m_DocHandler.Model is AssemblyDoc)
                    {
                        SubscribeAssemblyEvents(m_DocHandler.Model as AssemblyDoc);
                    }
                    else if (m_DocHandler.Model is DrawingDoc)
                    {
                        SubscribeDrawingEvents(m_DocHandler.Model as DrawingDoc);
                    }
                    else
                    {
                        throw new InvalidCastException("Model type is null or invalid");
                    }

                    m_IsSubscribed = true;
                }
            }

            private void UnsubscribeIfNeeded()
            {
                if (m_IsSubscribed)
                {
                    if (m_DocHandler.Model is PartDoc)
                    {
                        UnsubscribePartEvents(m_DocHandler.Model as PartDoc);
                    }
                    else if (m_DocHandler.Model is AssemblyDoc)
                    {
                        UnsubscribeAssemblyEvents(m_DocHandler.Model as AssemblyDoc);
                    }
                    else if (m_DocHandler.Model is DrawingDoc)
                    {
                        UnsubscribeDrawingEvents(m_DocHandler.Model as DrawingDoc);
                    }
                    else
                    {
                        throw new InvalidCastException("Model type is null or invalid");
                    }

                    m_IsSubscribed = false;
                }
            }

            public void Dispose()
            {
                UnsubscribeIfNeeded();
            }
            
            protected abstract void SubscribePartEvents(PartDoc part);
            protected abstract void SubscribeAssemblyEvents(AssemblyDoc assm);
            protected abstract void SubscribeDrawingEvents(DrawingDoc draw);
            protected abstract void UnsubscribePartEvents(PartDoc part);
            protected abstract void UnsubscribeAssemblyEvents(AssemblyDoc assm);
            protected abstract void UnsubscribeDrawingEvents(DrawingDoc draw);

            internal void Attach(TDel del)
            {
                SubscribeIfNeeded();
                OnAttach(del);
            }

            internal void Detach(TDel del)
            {
                OnDetach(del);

                if (Delegate == null)
                {
                    UnsubscribeIfNeeded();
                }
            }

            protected abstract void OnAttach(TDel del);
            protected abstract void OnDetach(TDel del);
        }

        private class RebuildEventsHandler : EventsHandler<RebuildDelegate>
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

        private readonly RebuildEventsHandler m_RebuildEventsHandler;

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

        private bool m_Is3rdPartyStreamLoaded;
        private bool m_Is3rdPartyStoreLoaded;

        public DocumentHandler()
        {
            m_RebuildEventsHandler = new RebuildEventsHandler(this);
        }

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public virtual void Init(ISldWorks app, IModelDoc2 model)
        {
            m_Is3rdPartyStreamLoaded = false;
            m_Is3rdPartyStoreLoaded = false;

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
        private void OnAccess3rdPartyData(DocumentHandler docHandler, Access3rdPartyDataAction_e type)
        {
            switch (type)
            {
#pragma warning disable CS0618
                case Access3rdPartyDataAction_e.StorageRead:
                    OnLoadFromStorageStore();
                    break;

                case Access3rdPartyDataAction_e.StorageWrite:
                    OnSaveToStorageStore();
                    break;

                case Access3rdPartyDataAction_e.StreamRead:
                    OnLoadFromStream();
                    break;

                case Access3rdPartyDataAction_e.StreamWrite:
                    OnSaveToStream();
                    break;
#pragma warning restore CS0618
            }
        }
        //---

        private int OnActiveModelDocChangeNotify()
        {
            if (App.ActiveDoc == Model)
            {
                Activated?.Invoke(this);
                OnActivate();
            }

            return S_OK;
        }

        private int OnIdleHandleThirdPartyStorageNotify()
        {
            EnsureLoadFromStream();
            EnsureLoadFromStorageStore();

            //only need to handle loading one time
            (App as SldWorks).OnIdleNotify -= OnIdleHandleThirdPartyStorageNotify;

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

            UnsubscribeSaveEvents();

            UnsubscribeSelectionEvents();

            Unsubscribe3rdPartyDataAccessEvents();

            UnsubscribeCustomPropertyChangedEvents();

            UnsubscribeItemModifiedEvents();

            UnsubscribeConfigurationChangedEvents();

            m_RebuildEventsHandler.Dispose();

            Destroyed?.Invoke(this);
            OnDestroy();
        }

        #region Events Unsubscribing

        private void UnsubscribeSaveEvents()
        {
            if (m_SaveSubscribed)
            {
                if (Model is PartDoc)
                {
                    (Model as PartDoc).AutoSaveNotify -= OnAutoSaveNotify;
                    (Model as PartDoc).FileSaveAsNotify2 -= OnFileSaveAsNotify2;
                    (Model as PartDoc).FileSaveNotify -= OnFileSaveNotify;
                    (Model as PartDoc).FileSavePostCancelNotify -= OnFileSavePostCancelNotify;
                    (Model as PartDoc).FileSavePostNotify -= OnFileSavePostNotify;
                }
                else if (Model is AssemblyDoc)
                {
                    (Model as AssemblyDoc).AutoSaveNotify -= OnAutoSaveNotify;
                    (Model as AssemblyDoc).FileSaveAsNotify2 -= OnFileSaveAsNotify2;
                    (Model as AssemblyDoc).FileSaveNotify -= OnFileSaveNotify;
                    (Model as AssemblyDoc).FileSavePostCancelNotify -= OnFileSavePostCancelNotify;
                    (Model as AssemblyDoc).FileSavePostNotify -= OnFileSavePostNotify;
                }
                else if (Model is DrawingDoc)
                {
                    (Model as DrawingDoc).AutoSaveNotify -= OnAutoSaveNotify;
                    (Model as DrawingDoc).FileSaveAsNotify2 -= OnFileSaveAsNotify2;
                    (Model as DrawingDoc).FileSaveNotify -= OnFileSaveNotify;
                    (Model as DrawingDoc).FileSavePostCancelNotify -= OnFileSavePostCancelNotify;
                    (Model as DrawingDoc).FileSavePostNotify -= OnFileSavePostNotify;
                }

                m_SaveSubscribed = false;
            }
        }

        private void UnsubscribeSelectionEvents()
        {
            if (m_SelectionSubscribed)
            {
                if (Model is PartDoc)
                {
                    (Model as PartDoc).ClearSelectionsNotify -= OnClearSelectionsNotify;
                    (Model as PartDoc).NewSelectionNotify -= OnNewSelectionNotify;
                    (Model as PartDoc).UserSelectionPreNotify -= OnUserSelectionPreNotify;
                    (Model as PartDoc).UserSelectionPostNotify -= OnUserSelectionPostNotify;
                }
                else if (Model is AssemblyDoc)
                {
                    (Model as AssemblyDoc).ClearSelectionsNotify -= OnClearSelectionsNotify;
                    (Model as AssemblyDoc).NewSelectionNotify -= OnNewSelectionNotify;
                    (Model as AssemblyDoc).UserSelectionPreNotify -= OnUserSelectionPreNotify;
                    (Model as AssemblyDoc).UserSelectionPostNotify -= OnUserSelectionPostNotify;
                }
                else if (Model is DrawingDoc)
                {
                    (Model as DrawingDoc).ClearSelectionsNotify -= OnClearSelectionsNotify;
                    (Model as DrawingDoc).NewSelectionNotify -= OnNewSelectionNotify;
                    (Model as DrawingDoc).UserSelectionPreNotify -= OnUserSelectionPreNotify;
                    (Model as DrawingDoc).UserSelectionPostNotify -= OnUserSelectionPostNotify;
                }

                m_SelectionSubscribed = false;
            }
        }

        private void Unsubscribe3rdPartyDataAccessEvents()
        {
            if (m_Access3rdPartyDataSubscribed)
            {
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

                m_Access3rdPartyDataSubscribed = false;
            }
        }

        private void UnsubscribeCustomPropertyChangedEvents()
        {
            if (m_DocumentCustomPropertyChangeSubscirbed)
            {
                m_CustPrpsEventsHandler.PropertyChanged -= OnCustomPropertiesPropertyModified;
                m_CustPrpsEventsHandler.Dispose();
                m_CustPrpsEventsHandler = null;

                m_DocumentCustomPropertyChangeSubscirbed = false;
            }
        }

        private void UnsubscribeItemModifiedEvents()
        {
            if (m_ItemModifySubscribed)
            {
                if (Model is PartDoc)
                {
                    (Model as PartDoc).AddItemNotify -= OnAddItemNotify;
                    (Model as PartDoc).DeleteItemNotify -= OnDeleteItemNotify;
                    (Model as PartDoc).DeleteItemPreNotify -= OnDeleteItemPreNotify;
                    (Model as PartDoc).PreRenameItemNotify -= OnPreRenameItemNotify;
                    (Model as PartDoc).RenameItemNotify -= OnRenameItemNotify;
                }
                else if (Model is AssemblyDoc)
                {
                    (Model as AssemblyDoc).AddItemNotify -= OnAddItemNotify;
                    (Model as AssemblyDoc).DeleteItemNotify -= OnDeleteItemNotify;
                    (Model as AssemblyDoc).DeleteItemPreNotify -= OnDeleteItemPreNotify;
                    (Model as AssemblyDoc).PreRenameItemNotify -= OnPreRenameItemNotify;
                    (Model as AssemblyDoc).RenameItemNotify -= OnRenameItemNotify;
                }
                else if (Model is DrawingDoc)
                {
                    (Model as DrawingDoc).AddItemNotify -= OnAddItemNotify;
                    (Model as DrawingDoc).DeleteItemNotify -= OnDeleteItemNotify;
                    (Model as DrawingDoc).DeleteItemPreNotify -= OnDeleteItemPreNotify;
                    (Model as DrawingDoc).RenameItemNotify -= OnRenameItemNotify;
                }

                m_ItemModifySubscribed = false;
            }
        }

        private void UnsubscribeConfigurationChangedEvents()
        {
            if (m_ConfigurationChangeSubscribed)
            {
                if (Model is PartDoc)
                {
                    (Model as PartDoc).ConfigurationChangeNotify -= OnConfigurationChangeNotify;
                }
                else if (Model is AssemblyDoc)
                {
                    (Model as AssemblyDoc).ConfigurationChangeNotify -= OnConfigurationChangeNotify;
                }
                else if (Model is DrawingDoc)
                {
                    (Model as DrawingDoc).ActivateSheetPreNotify -= OnActivateSheetPreNotify;
                    (Model as DrawingDoc).ActivateSheetPostNotify -= OnActivateSheetPostNotify;
                }

                m_ConfigurationChangeSubscribed = false;
            }
        }

        #endregion

        /// <summary>
        /// Override to dispose the resources
        /// </summary>
        /// <remarks>Invoked when document has been destroyed</remarks>
        public virtual void OnDestroy()
        {
        }

        #region Events Handlers

        private int OnSaveToStorageStoreNotify()
        {
            m_Access3rdPartyDataDelegate.Invoke(this, Access3rdPartyDataAction_e.StorageWrite);
            return S_OK;
        }

        private int OnLoadFromStorageNotify()
        {
            //NOTE: by some reasons this event is triggered twice, adding the flag to avoid repetition
            return EnsureLoadFromStream();
        }

        private int OnSaveToStorageNotify()
        {
            m_Access3rdPartyDataDelegate.Invoke(this, Access3rdPartyDataAction_e.StreamWrite);

            return S_OK;
        }

        private int OnLoadFromStorageStoreNotify()
        {
            return EnsureLoadFromStorageStore();
        }

        private int OnUserSelectionPostNotify()
        {
            return m_SelectionDelegate.Invoke(this, swSelectType_e.swSelNOTHING, SelectionAction_e.UserPostSelect) ? S_OK : S_FALSE;
        }

        private int OnUserSelectionPreNotify(int selType)
        {
            return m_SelectionDelegate.Invoke(this, (swSelectType_e)selType, SelectionAction_e.UserPreSelect) ? S_OK : S_FALSE;
        }

        private int OnNewSelectionNotify()
        {
            return m_SelectionDelegate.Invoke(this, swSelectType_e.swSelNOTHING, SelectionAction_e.NewSelection) ? S_OK : S_FALSE;
        }

        private int OnClearSelectionsNotify()
        {
            return m_SelectionDelegate.Invoke(this, swSelectType_e.swSelNOTHING, SelectionAction_e.ClearSelection) ? S_OK : S_FALSE;
        }

        private int OnFileSavePostNotify(int saveType, string fileName)
        {
            return m_SaveDelegate.Invoke(this, fileName, SaveAction_e.PostSave) ? S_OK : S_FALSE;
        }

        private int OnFileSavePostCancelNotify()
        {
            return m_SaveDelegate.Invoke(this, "", SaveAction_e.PostCancel) ? S_OK : S_FALSE;
        }

        private int OnFileSaveNotify(string fileName)
        {
            return m_SaveDelegate.Invoke(this, fileName, SaveAction_e.PreSave) ? S_OK : S_FALSE;
        }

        private int OnFileSaveAsNotify2(string fileName)
        {
            return m_SaveDelegate.Invoke(this, fileName, SaveAction_e.SaveAs) ? S_OK : S_FALSE;
        }

        private int OnAutoSaveNotify(string fileName)
        {
            return m_SaveDelegate.Invoke(this, fileName, SaveAction_e.AutoSave) ? S_OK : S_FALSE;
        }

        private void OnCustomPropertiesPropertyModified(DocumentHandler docHandler, CustomPropertyChangeAction_e type, string name, string conf, string value)
        {
            m_DocumentCustomPropertyChangeDelegate.Invoke(docHandler, type, name, conf, value);
        }

        private int OnRenameItemNotify(int entityType, string oldName, string newName)
        {
            m_ItemModifyDelegate.Invoke(this, ItemModificationAction_e.Rename,
                (swNotifyEntityType_e)entityType, newName, oldName);

            return S_OK;
        }

        private int OnPreRenameItemNotify(int entityType, string oldName, string newName)
        {
            m_ItemModifyDelegate.Invoke(this, ItemModificationAction_e.PreRename,
                (swNotifyEntityType_e)entityType, newName, oldName);

            return S_OK;
        }

        private int OnDeleteItemPreNotify(int entityType, string itemName)
        {
            m_ItemModifyDelegate.Invoke(this, ItemModificationAction_e.PreDelete,
                (swNotifyEntityType_e)entityType, itemName);

            return S_OK;
        }

        private int OnDeleteItemNotify(int entityType, string itemName)
        {
            m_ItemModifyDelegate.Invoke(this, ItemModificationAction_e.Delete,
                (swNotifyEntityType_e)entityType, itemName);

            return S_OK;
        }

        private int OnAddItemNotify(int entityType, string itemName)
        {
            m_ItemModifyDelegate.Invoke(this, ItemModificationAction_e.Add,
                (swNotifyEntityType_e)entityType, itemName);

            return S_OK;
        }

        private int OnActivateSheetPreNotify(string sheetName)
        {
            m_ConfigurationChangeDelegate.Invoke(this, ConfigurationChangeAction_e.PreActivate,
                sheetName);

            return S_OK;
        }

        private int OnActivateSheetPostNotify(string sheetName)
        {
            m_ConfigurationChangeDelegate.Invoke(this, ConfigurationChangeAction_e.PostActivate,
                sheetName);

            return S_OK;
        }

        private int OnConfigurationChangeNotify(string configurationName, object obj, int objectType, int changeType)
        {
            const int POST_NOTIFICATION = 11;

            m_ConfigurationChangeDelegate.Invoke(this, (changeType == POST_NOTIFICATION ? ConfigurationChangeAction_e.PostActivate : ConfigurationChangeAction_e.PreActivate),
                configurationName);

            return S_OK;
        }

        #endregion

        private int EnsureLoadFromStream()
        {
            if (!m_Is3rdPartyStreamLoaded)
            {
                m_Is3rdPartyStreamLoaded = true;
                m_Access3rdPartyDataDelegate.Invoke(this, Access3rdPartyDataAction_e.StreamRead);
            }

            return S_OK;
        }

        private int EnsureLoadFromStorageStore()
        {
            if (!m_Is3rdPartyStoreLoaded)
            {
                m_Is3rdPartyStoreLoaded = true;
                m_Access3rdPartyDataDelegate.Invoke(this, Access3rdPartyDataAction_e.StorageRead);
            }

            return S_OK;
        }
    }
}
