//**********************
//SwEx.AddIn - development tools for SOLIDWORKS add-ins
//Copyright(C) 2019 www.codestack.net
//License: https://github.com/codestack-net-dev/sw-dev-tools-addin/blob/master/LICENSE
//Product URL: https://www.codestack.net/labs/solidworks/swex/add-in/
//**********************

using System;
using System.IO;

namespace CodeStack.SwEx.AddIn.Base
{
    public interface IThirdPartyStreamHandler : IDisposable
    {
        Stream Stream { get; }
    }
}
