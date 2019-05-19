using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CodeStack.SwEx.AddIn.Base
{
    public interface IThirdPartyStreamHandler : IDisposable
    {
        Stream Stream { get; }
    }
}
