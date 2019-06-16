//**********************
//SwEx.AddIn - development tools for SOLIDWORKS add-ins
//Copyright(C) 2019 www.codestack.net
//License: https://github.com/codestack-net-dev/sw-dev-tools-addin/blob/master/LICENSE
//Product URL: https://www.codestack.net/labs/solidworks/swex/add-in/
//**********************

using CodeStack.SwEx.AddIn.Base;
using CodeStack.SwEx.Common.Diagnostics;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System;
using System.Diagnostics;

namespace CodeStack.SwEx.AddIn.Helpers
{
    internal class DocumentHandlerWrapper<TDocHandler> : IDisposable
            where TDocHandler : IDocumentHandler, new()
    {
        internal event Action<IModelDoc2> DocumentDestroyed;

        private readonly IModelDoc2 m_Model;
        private readonly TDocHandler m_DocHandler;
        private readonly ILogger m_Logger;

        internal DocumentHandlerWrapper(ISldWorks app, IModelDoc2 model, ILogger logger)
        {
            m_Logger = logger;
            m_Model = model;

            m_Logger.Log($"Creating document handler for '{model.GetTitle()}' document");

            m_DocHandler = new TDocHandler();
            m_DocHandler.Init(app, model);

            AttachEvents();
        }

        internal TDocHandler Handler
        {
            get
            {
                return m_DocHandler;
            }
        }

        private void AttachEvents()
        {
            if (m_Model is PartDoc)
            {
                (m_Model as PartDoc).DestroyNotify2 += OnDestroyNotify;
            }
            else if (m_Model is AssemblyDoc)
            {
                (m_Model as AssemblyDoc).DestroyNotify2 += OnDestroyNotify;
            }
            else if (m_Model is DrawingDoc)
            {
                (m_Model as DrawingDoc).DestroyNotify2 += OnDestroyNotify;
            }
        }

        private void DetachEvents()
        {
            if (m_Model is PartDoc)
            {
                (m_Model as PartDoc).DestroyNotify2 -= OnDestroyNotify;
            }
            else if (m_Model is AssemblyDoc)
            {
                (m_Model as AssemblyDoc).DestroyNotify2 -= OnDestroyNotify;
            }
            else if (m_Model is DrawingDoc)
            {
                (m_Model as DrawingDoc).DestroyNotify2 -= OnDestroyNotify;
            }
        }

        private int OnDestroyNotify(int destroyType)
        {
            const int S_OK = 0;

            if (destroyType == (int)swDestroyNotifyType_e.swDestroyNotifyDestroy)
            {
                m_Logger.Log($"Destroying '{m_Model.GetTitle()}' document");

                DocumentDestroyed?.Invoke(m_Model);

                Dispose();
            }
            else if (destroyType == (int)swDestroyNotifyType_e.swDestroyNotifyHidden)
            {
                m_Logger.Log($"Hiding '{m_Model.GetTitle()}' document");
            }
            else
            {
                Debug.Assert(false, "Not supported type of destroy");
            }

            return S_OK;
        }

        public void Dispose()
        {
            m_DocHandler.Dispose();
            
            DetachEvents();
        }
    }
}
