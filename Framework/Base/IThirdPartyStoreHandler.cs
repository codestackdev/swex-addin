using CodeStack.SwEx.AddIn.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CodeStack.SwEx.AddIn.Base
{
    public interface IThirdPartyStoreHandler : IDisposable
    {
        IComStorage Storage { get; }
    }
}
