using SolidWorks.Interop.sldworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CodeStack.SwEx.AddIn.Base
{
    public interface IDocumentHandler : IDisposable
    {
        void Init(ISldWorks app, IModelDoc2 model);
    }
}
