//**********************
//SwEx.AddIn - development tools for SOLIDWORKS add-ins
//Copyright(C) 2019 www.codestack.net
//License: https://github.com/codestack-net-dev/sw-dev-tools-addin/blob/master/LICENSE
//Product URL: https://www.codestack.net/labs/solidworks/swex/add-in/
//**********************

using System;

namespace CodeStack.SwEx.AddIn.Base
{
    /// <summary>
    /// Disposable handler for SOLIDWORKS model 3rd party storage store
    /// </summary>
    public interface IThirdPartyStoreHandler : IDisposable
    {
        /// <summary>
        /// Underlying COM storage
        /// </summary>
        IComStorage Storage { get; }
    }
}
