//**********************
//SwEx.AddIn - development tools for SOLIDWORKS add-ins
//Copyright(C) 2019 www.codestack.net
//License: https://github.com/codestack-net-dev/sw-dev-tools-addin/blob/master/LICENSE
//Product URL: https://www.codestack.net/labs/solidworks/swex/add-in/
//**********************

using CodeStack.SwEx.AddIn.Base;
using SolidWorks.Interop.sldworks;
using System;

namespace CodeStack.SwEx.AddIn.Core
{
    internal class ThirdPartyStoreHandler : IThirdPartyStoreHandler
    {
        private readonly IModelDoc2 m_Model;
        private readonly string m_Name;

        public IComStorage Storage { get; }

        internal ThirdPartyStoreHandler(IModelDoc2 model, string name, bool write)
        {
            m_Model = model;
            m_Name = name;

            var storage = model.Extension.IGet3rdPartyStorageStore(name, write) as IStorage;

            if (storage != null)
            {
                Storage = new ComStorage(storage, write);
            }
            else
            {
                Storage = null;
            }
        }
        
        public void Dispose()
        {
            Storage?.Dispose();

            if (!m_Model.Extension.IRelease3rdPartyStorageStore(m_Name))
            {
                if (Storage != null)//returns false when storage didn't exist on read
                {
                    throw new InvalidOperationException("Failed to release 3rd party storage store");
                }
            }
        }
    }
}
