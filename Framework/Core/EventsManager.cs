using CodeStack.SwEx.AddIn.Base;
using SolidWorks.Interop.sldworks;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace CodeStack.SwEx.AddIn.Core
{
    public interface IDocumentHandler : IDisposable
    {
        void Init(IModelDoc2 model);
    }

    internal class DocumentEventHandler<TDocHandler>
        where TDocHandler : IDocumentHandler, new()
    {
        internal event Action<IModelDoc2> DocumentDestroyed;

        private readonly IModelDoc2 m_Model;
        private readonly TDocHandler m_DocHandler;

        internal DocumentEventHandler(IModelDoc2 model)
        {
            //TODO: add log

            m_Model = model;
            m_DocHandler = new TDocHandler();
            m_DocHandler.Init(model);

            AttachEvents();
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
            //TODO: add log

            const int S_OK = 0;

            m_DocHandler.Dispose();

            DocumentDestroyed?.Invoke(m_Model);

            DetachEvents();

            return S_OK;
        }
    }

    public class EventsManager<TDocHandler> : IEventsManager
        where TDocHandler : IDocumentHandler, new()
    {
        private readonly SldWorks m_App;
        private readonly Dictionary<IModelDoc2, DocumentEventHandler<TDocHandler>> m_Documents;

        internal EventsManager(ISldWorks app)
        {
            m_App = app as SldWorks;
            m_Documents = new Dictionary<IModelDoc2, DocumentEventHandler<TDocHandler>>();

            //TODO: attach to all already opened documents

            m_App.DocumentLoadNotify2 += OnDocumentLoadNotify2;
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

            if (!m_Documents.ContainsKey(model))
            {
                var docHandler = new DocumentEventHandler<TDocHandler>(model);
                docHandler.DocumentDestroyed += OnDocumentDestroyed;

                m_Documents.Add(model, docHandler);
            }
            else
            {
                //TODO: add log
                Debug.Assert(false, "Document was not unregistered");
            }

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
        }
    }
}
