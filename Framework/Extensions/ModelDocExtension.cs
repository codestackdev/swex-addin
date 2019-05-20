//**********************
//SwEx.AddIn - development tools for SOLIDWORKS add-ins
//Copyright(C) 2019 www.codestack.net
//License: https://github.com/codestack-net-dev/sw-dev-tools-addin/blob/master/LICENSE
//Product URL: https://www.codestack.net/labs/solidworks/swex/add-in/
//**********************

using CodeStack.SwEx.AddIn.Base;
using CodeStack.SwEx.AddIn.Core;

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
