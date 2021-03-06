﻿//**********************
//SwEx.AddIn - development tools for SOLIDWORKS add-ins
//Copyright(C) 2019 www.codestack.net
//License: https://github.com/codestackdev/swex-addin/blob/master/LICENSE
//Product URL: https://www.codestack.net/labs/solidworks/swex/add-in/
//**********************

using CodeStack.SwEx.AddIn.Base;
using CodeStack.SwEx.AddIn.Core;
using CodeStack.SwEx.AddIn.Helpers;
using CodeStack.SwEx.Common.Diagnostics;
using SolidWorks.Interop.sldworks;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace CodeStack.SwEx.AddIn.Core
{
    internal class DocumentsHandler<TDocHandler> : IDocumentsHandler<TDocHandler>
        where TDocHandler : IDocumentHandler, new()
    {
        public event Action<TDocHandler> HandlerCreated;

        private readonly SldWorks m_App;
        private readonly Dictionary<IModelDoc2, DocumentHandlerWrapper<TDocHandler>> m_Documents;
        private readonly ILogger m_Logger;

        public TDocHandler this[IModelDoc2 model]
        {
            get
            {
                DocumentHandlerWrapper<TDocHandler> wrapper;

                if (m_Documents.TryGetValue(model, out wrapper))
                {
                    return wrapper.Handler;
                }
                else
                {
                    throw new KeyNotFoundException("Specified model document is not registered");
                }
            }
        }
        
        internal DocumentsHandler(ISldWorks app, ILogger logger)
        {
            m_App = app as SldWorks;
            m_Logger = logger;

            m_Documents = new Dictionary<IModelDoc2, DocumentHandlerWrapper<TDocHandler>>();

            AttachToAllOpenedDocuments();

            m_App.DocumentLoadNotify2 += OnDocumentLoadNotify2;
        }

        private void AttachToAllOpenedDocuments()
        {
            var openDocs = m_App.GetDocuments() as object[];

            if (openDocs != null)
            {
                foreach (IModelDoc2 model in openDocs)
                {
                    AttachDocument(model);
                }
            }
        }

        private void AttachDocument(IModelDoc2 model)
        {
            if (!m_Documents.ContainsKey(model))
            {
                var docHandler = new DocumentHandlerWrapper<TDocHandler>(m_App, model, m_Logger);
                docHandler.DocumentDestroyed += OnDocumentDestroyed;

                m_Documents.Add(model, docHandler);

                HandlerCreated?.Invoke(docHandler.Handler);
            }
            else
            {
                m_Logger.Log($"Conflict. {model.GetTitle()} already registered");
                Debug.Assert(false, "Document was not unregistered");
            }
        }

        private int OnDocumentLoadNotify2(string docTitle, string docPath)
        {
            const int S_OK = 0;

            IModelDoc2 model;

            if (!string.IsNullOrEmpty(docPath))
            {
                model = m_App.GetOpenDocumentByName(docPath) as IModelDoc2;
            }
            else
            {
                model = (m_App.GetDocuments() as object[])?.FirstOrDefault(
                    d => string.Equals((d as IModelDoc2).GetTitle(), docTitle)) as IModelDoc2;
            }

            if (model == null)
            {
                throw new NullReferenceException($"Failed to find the loaded model: {docTitle} ({docPath})");
            }

            AttachDocument(model);

            return S_OK;
        }

        private void OnDocumentDestroyed(IModelDoc2 model)
        {
            var docHandler = m_Documents[model];
            docHandler.DocumentDestroyed -= OnDocumentDestroyed;
            m_Documents.Remove(model);
        }

        public void Dispose()
        {
            m_App.DocumentLoadNotify2 -= OnDocumentLoadNotify2;

            foreach (var docHandler in m_Documents.Values)
            {
                docHandler.Dispose();
            }

            m_Documents.Clear();
        }
    }
}
