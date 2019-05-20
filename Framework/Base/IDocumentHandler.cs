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
    /// Document handler to be used in <see cref="IDocumentsHandler{TDocHandler}"/> documents manager
    /// </summary>
    public interface IDocumentHandler : IDisposable
    {
        /// <summary>
        /// Called when model document is initialized (created)
        /// </summary>
        /// <param name="app">Pointer to SOLIDWORKS application</param>
        /// <param name="model">Pointer to this model document</param>
        void Init(ISldWorks app, IModelDoc2 model);
    }
}
