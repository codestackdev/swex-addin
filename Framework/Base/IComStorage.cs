//**********************
//SwEx.AddIn - development tools for SOLIDWORKS add-ins
//Copyright(C) 2019 www.codestack.net
//License: https://github.com/codestack-net-dev/sw-dev-tools-addin/blob/master/LICENSE
//Product URL: https://www.codestack.net/labs/solidworks/swex/add-in/
//**********************

using CodeStack.SwEx.AddIn.Core;
using System;
using System.Collections.Generic;

namespace CodeStack.SwEx.AddIn.Base
{
    public interface IComStorage : IDisposable
    {
        IStorage Storage { get; }
        IComStorage TryOpenStorage(string storageName, bool createIfNotExist);
        ComStream TryOpenStream(string streamName, bool createIfNotExist);
        IEnumerable<string> EnumSubStreamNames();
        IEnumerable<string> EnumSubStorageNames();
    }
}
