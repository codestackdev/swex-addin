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
        private const char STREAM_PATH_SEPARATOR = '\\';

        public static bool Access3rdPartyStream(this IModelDoc2 model, string name, bool write, Action<Stream> action)
        {
            if (name.Contains(STREAM_PATH_SEPARATOR))
            {
                var storageName = name.Substring(0, name.IndexOf(STREAM_PATH_SEPARATOR));

                var mode = STGM.STGM_SHARE_EXCLUSIVE;

                if (write)
                {
                    mode |= STGM.STGM_READWRITE;
                }

                if (Access3rdPartyStorageStore(model, storageName, write, 
                    storage => 
                    {
                        AccessStreamFromPath(storage, name.Substring(name.IndexOf(STREAM_PATH_SEPARATOR) + 1), write, write, action, mode);
                    }))
                {
                }
                else
                {
                    return false;
                }
            }
            else
            {
                try
                {
                    var stream = model.IGet3rdPartyStorage(name, write) as IStream;

                    if (stream != null)
                    {
                        using (var comStr = new ComStream(stream, write))
                        {
                            comStr.Seek(0, SeekOrigin.Begin);
                            action.Invoke(comStr);
                        }

                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                finally
                {
                    model.IRelease3rdPartyStorage(name);
                }
            }

            return false;
        }

        public static bool Access3rdPartyStorageStore(this IModelDoc2 model, string name, bool write, Action<ComStorage> action)
        {
            try
            {
                var storage = model.Extension.IGet3rdPartyStorageStore(name, write) as IStorage;

                if (storage != null)
                {
                    using (var comStorage = new ComStorage(storage, write))
                    {
                        action.Invoke(comStorage);
                    }

                    return true;
                }
                else
                {
                    return false;
                }
            }
            finally
            {
                model.Extension.IRelease3rdPartyStorageStore(name);
            }
        }

        private static void AccessStreamFromPath(ComStorage storage, string path, bool writable,
            bool createIfNotExist, Action<ComStream> action, STGM mode)
        {
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
                        throw;
                    }
                }

                using (var comStream = new ComStream(stream, writable))
                {
                    action.Invoke(comStream);
                }
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
                        throw;
                    }
                }

                using (var subComStorage = new ComStorage(subStorage, writable))
                {
                    var nextLevelPath = path.Substring(parentIndex + 1);
                    AccessStreamFromPath(subComStorage, nextLevelPath, writable,
                        createIfNotExist, action, mode); //TODO: not sure about mode here
                }
            }
        }
    }
}
