//**********************
//SwEx.AddIn - development tools for SOLIDWORKS add-ins
//Copyright(C) 2019 www.codestack.net
//License: https://github.com/codestack-net-dev/sw-dev-tools-addin/blob/master/LICENSE
//Product URL: https://www.codestack.net/labs/solidworks/swex/add-in/
//**********************

using SolidWorks.Interop.sldworks;
using System;

namespace CodeStack.SwEx.AddIn.Base
{
    /// <summary>
    /// Manages the lifecycle of documents
    /// </summary>
    /// <typeparam name="TDocHandler">Custom document handler to wrap models</typeparam>
    public interface IDocumentsHandler<TDocHandler> : IDisposable
        where TDocHandler : IDocumentHandler, new()
    {
        /// <summary>
        /// Event raised when new document handler is created
        /// </summary>
        event Action<TDocHandler> HandlerCreated;

        /// <summary>
        /// Accesses the document handler by pointer to model
        /// </summary>
        /// <param name="model">Pointer to model</param>
        /// <returns>Corresponding model handler</returns>
        /// <exception cref="System.Collections.Generic.KeyNotFoundException"/>
        TDocHandler this[IModelDoc2 model] { get; }
    }
}
