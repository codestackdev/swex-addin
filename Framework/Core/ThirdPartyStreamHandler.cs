using CodeStack.SwEx.AddIn.Base;
using SolidWorks.Interop.sldworks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;

namespace CodeStack.SwEx.AddIn.Core
{
    public class ThirdPartyStreamHandler : IThirdPartyStreamHandler
    {
        private readonly IModelDoc2 m_Model;
        private readonly string m_Name;
        
        public Stream Stream { get; }
                        
        internal ThirdPartyStreamHandler(IModelDoc2 model, string name, bool write)
        {
            m_Model = model;

            m_Name = name;
            var stream = model.IGet3rdPartyStorage(name, write) as IStream;

            if (stream != null)
            {
                Stream = new ComStream(stream, write, false);
                Stream.Seek(0, SeekOrigin.Begin);
            }
            else
            {
                //TODO: add: log
                Stream = null;
            }
        }
        
        public void Dispose()
        {
            Stream?.Dispose();
            m_Model.IRelease3rdPartyStorage(m_Name);
        }
    }
}
