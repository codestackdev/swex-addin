//**********************
//SwEx.AddIn - development tools for SOLIDWORKS add-ins
//Copyright(C) 2019 www.codestack.net
//License: https://github.com/codestackdev/swex-addin/blob/master/LICENSE
//Product URL: https://www.codestack.net/labs/solidworks/swex/add-in/
//**********************

using System;
using System.IO;

namespace CodeStack.SwEx.AddIn.Base
{
    /// <summary>
    /// Disposable handler for SOLIDWORKS model 3rd party storage (stream)
    /// </summary>
    public interface IThirdPartyStreamHandler : IDisposable
    {
        /// <summary>
        /// Underlying stream
        /// </summary>
        Stream Stream { get; }
    }
}
