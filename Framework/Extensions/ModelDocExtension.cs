using CodeStack.SwEx.AddIn.Base;
using CodeStack.SwEx.AddIn.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;

namespace SolidWorks.Interop.sldworks
{
    public static class ModelDocExtension
    {
        public static IThirdPartyStreamHandler Access3rdPartyStream(this IModelDoc2 model, string name, bool write)
        {
            return new ThirdPartyStreamHandler(model, name, write);
        }

        public static IThirdPartyStoreHandler Access3rdPartyStorageStore(this IModelDoc2 model, string name, bool write)
        {
            return new ThirdPartyStoreHandler(model, name, write);
        }
    }
}
