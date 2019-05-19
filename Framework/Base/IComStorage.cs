using CodeStack.SwEx.AddIn.Core;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.ComTypes;

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
