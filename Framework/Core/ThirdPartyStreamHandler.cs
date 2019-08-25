//**********************
//SwEx.AddIn - development tools for SOLIDWORKS add-ins
//Copyright(C) 2019 www.codestack.net
//License: https://github.com/codestackdev/swex-addin/blob/master/LICENSE
//Product URL: https://www.codestack.net/labs/solidworks/swex/add-in/
//**********************

using CodeStack.SwEx.AddIn.Base;
using SolidWorks.Interop.sldworks;
using System.IO;
using System.Runtime.InteropServices.ComTypes;

namespace CodeStack.SwEx.AddIn.Core
{
    internal class ThirdPartyStreamHandler : IThirdPartyStreamHandler
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
