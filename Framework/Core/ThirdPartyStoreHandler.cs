using CodeStack.SwEx.AddIn.Base;
using SolidWorks.Interop.sldworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CodeStack.SwEx.AddIn.Core
{
    public class ThirdPartyStoreHandler : IThirdPartyStoreHandler
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
