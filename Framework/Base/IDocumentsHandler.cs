using SolidWorks.Interop.sldworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CodeStack.SwEx.AddIn.Base
{
    public interface IDocumentsHandler<TDocHandler>
        where TDocHandler : IDocumentHandler, new()
    {
    }
}
