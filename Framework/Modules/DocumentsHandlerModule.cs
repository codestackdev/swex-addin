using CodeStack.SwEx.AddIn.Base;
using CodeStack.SwEx.AddIn.Core;
using CodeStack.SwEx.Common.Diagnostics;
using SolidWorks.Interop.sldworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CodeStack.SwEx.AddIn.Modules
{
    internal class DocumentsHandlerModule : IDisposable
    {
        private readonly ISldWorks m_App;
        private readonly ILogger m_Logger;
        private readonly List<IDisposable> m_DocsHandlers;

        internal DocumentsHandlerModule(ISldWorks app, ILogger logger)
        {
            m_App = app;
            m_Logger = logger;

            m_DocsHandlers = new List<IDisposable>();
        }

        internal IDocumentsHandler<TDocHandler> CreateDocumentsHandler<TDocHandler>()
            where TDocHandler : IDocumentHandler, new()
        {
            var docsHandler = new DocumentsHandler<TDocHandler>(m_App, m_Logger);

            m_DocsHandlers.Add(docsHandler);

            return docsHandler;
        }

        public void Dispose()
        {
            foreach (var docHandler in m_DocsHandlers)
            {
                docHandler.Dispose();
            }

            m_DocsHandlers.Clear();
        }
    }
}
