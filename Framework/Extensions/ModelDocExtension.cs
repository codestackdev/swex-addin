//**********************
//SwEx.AddIn - development tools for SOLIDWORKS add-ins
//Copyright(C) 2019 www.codestack.net
//License: https://github.com/codestackdev/swex-addin/blob/master/LICENSE
//Product URL: https://www.codestack.net/labs/solidworks/swex/add-in/
//**********************

using CodeStack.SwEx.AddIn.Base;
using CodeStack.SwEx.AddIn.Core;

namespace SolidWorks.Interop.sldworks
{
    /// <summary>
    /// Provides extension methods for SOLIDWORKS document
    /// </summary>
    public static class ModelDocExtension
    {
        /// <summary>
        /// Access the 3rd party storage (stream) for reading or writing
        /// </summary>
        /// <param name="model">Pointer to document</param>
        /// <param name="name">Name of the stream</param>
        /// <param name="write">True to open for writing, false to open for reading</param>
        /// <returns>Pointer to the stream handler</returns>
        public static IThirdPartyStreamHandler Access3rdPartyStream(this IModelDoc2 model, string name, bool write)
        {
            return new ThirdPartyStreamHandler(model, name, write);
        }

        /// <summary>
        /// Access the 3rd party storage store for reading or writing
        /// </summary>
        /// <param name="model">Pointer to document</param>
        /// <param name="name">Name of the stream</param>
        /// <param name="write">True to open for writing, false to open for reading</param>
        /// <returns>Pointer to the store handler</returns>
        public static IThirdPartyStoreHandler Access3rdPartyStorageStore(this IModelDoc2 model, string name, bool write)
        {
            return new ThirdPartyStoreHandler(model, name, write);
        }
    }
}
