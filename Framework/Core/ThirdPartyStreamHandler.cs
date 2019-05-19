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
        private const char STREAM_PATH_SEPARATOR = '\\';

        private readonly IModelDoc2 m_Model;
        private readonly string m_Name;

        private readonly bool m_IsDirectStream;

        public Stream Stream { get; }

        private IThirdPartyStoreHandler m_Store;
        private readonly List<IComStorage> m_OpenStorages;
        
        internal ThirdPartyStreamHandler(IModelDoc2 model, string name, bool write)
        {
            m_Model = model;

            m_IsDirectStream = !name.Contains(STREAM_PATH_SEPARATOR);

            if (m_IsDirectStream)
            {
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
            else
            {
                var storageName = name.Substring(0, name.IndexOf(STREAM_PATH_SEPARATOR));

                var mode = STGM.STGM_SHARE_EXCLUSIVE;

                if (write)
                {
                    mode |= STGM.STGM_READWRITE;
                }

                m_Store = new ThirdPartyStoreHandler(model, storageName, write);

                if (m_Store.Storage != null)
                {
                    var streamPath = name.Substring(name.IndexOf(STREAM_PATH_SEPARATOR) + 1);

                    m_OpenStorages = new List<IComStorage>();
                    m_OpenStorages.Add(m_Store.Storage);

                    Stream = AccessStreamFromPath(m_Store.Storage, streamPath, write, write, mode);
                }
                else
                {
                    if (write)
                    {
                        throw new NullReferenceException("Stream cannot be accessed");
                    }
                }
            }
        }

        private ComStream AccessStreamFromPath(IComStorage storage, string path, bool writable,
            bool createIfNotExist, STGM mode)
        {
            if (storage == null)
            {
                throw new NullReferenceException("Storage is null. Make sure it is accessed from the correct event handler");
            }

            var parentIndex = path.IndexOf(STREAM_PATH_SEPARATOR);

            if (parentIndex == -1)
            {
                IStream stream = null;

                try
                {
                    stream = storage.OpenStream(path, mode);
                }
                catch
                {
                    if (createIfNotExist)
                    {
                        stream = storage.CreateStream(path);
                    }
                    else
                    {
                        return null;
                    }
                }

                return new ComStream(stream, writable);
            }
            else
            {
                var subStorageName = path.Substring(0, parentIndex);

                IStorage subStorage;

                try
                {
                    subStorage = storage.OpenStorage(subStorageName, mode);
                }
                catch
                {
                    if (createIfNotExist)
                    {
                        subStorage = storage.CreateStorage(subStorageName);
                    }
                    else
                    {
                        return null;
                    }
                }

                var subComStorage = new ComStorage(subStorage, writable);
                m_OpenStorages.Add(subComStorage);
                var nextLevelPath = path.Substring(parentIndex + 1);

                return AccessStreamFromPath(subComStorage, nextLevelPath, writable,
                    createIfNotExist, mode); //TODO: not sure about mode here
            }
        }

        public void Dispose()
        {
            if (m_IsDirectStream)
            {
                m_Model.IRelease3rdPartyStorage(m_Name);
            }
            else
            {
                if (m_OpenStorages?.Any() == true)
                {
                    for (int i = m_OpenStorages.Count - 1; i >= 0; i--)
                    {
                        m_OpenStorages[i].Dispose();
                    }

                    m_OpenStorages.Clear();
                }

                m_Store.Dispose();
            }
        }
    }
}
