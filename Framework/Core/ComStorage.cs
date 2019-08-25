//**********************
//SwEx.AddIn - development tools for SOLIDWORKS add-ins
//Copyright(C) 2019 www.codestack.net
//License: https://github.com/codestack-net-dev/sw-dev-tools-addin/blob/master/LICENSE
//Product URL: https://www.codestack.net/labs/solidworks/swex/add-in/
//**********************

using CodeStack.SwEx.AddIn.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;

namespace CodeStack.SwEx.AddIn.Core
{
    #region WinAPI

    //TODO: check if possible to do internal interfaces

    [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
    [Flags]
    public enum STGM : int
    {
        STGM_READ = 0x0,
        STGM_WRITE = 0x1,
        STGM_READWRITE = 0x2,
        STGM_SHARE_DENY_NONE = 0x40,
        STGM_SHARE_DENY_READ = 0x30,
        STGM_SHARE_DENY_WRITE = 0x20,
        STGM_SHARE_EXCLUSIVE = 0x10,
        STGM_PRIORITY = 0x40000,
        STGM_CREATE = 0x1000,
        STGM_CONVERT = 0x20000,
        STGM_FAILIFTHERE = 0x0,
        STGM_DIRECT = 0x0,
        STGM_TRANSACTED = 0x10000,
        STGM_NOSCRATCH = 0x100000,
        STGM_NOSNAPSHOT = 0x200000,
        STGM_SIMPLE = 0x8000000,
        STGM_DIRECT_SWMR = 0x400000,
        STGM_DELETEONRELEASE = 0x4000000
    }

    [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
    public enum STGTY : int
    {
        STGTY_STORAGE = 1,
        STGTY_STREAM = 2,
        STGTY_LOCKBYTES = 3,
        STGTY_PROPERTY = 4
    };

    [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
    [ComImport]
    [Guid("0000000d-0000-0000-C000-000000000046")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IEnumSTATSTG
    {
        [PreserveSig]
        uint Next(uint celt,
        [MarshalAs(UnmanagedType.LPArray), Out]
        System.Runtime.InteropServices.ComTypes.STATSTG[] rgelt,
        out uint pceltFetched
        );

        void Skip(uint celt);

        void Reset();

        [return: MarshalAs(UnmanagedType.Interface)]
        IEnumSTATSTG Clone();
    }

    [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
    [ComImport]
    [Guid("0000000b-0000-0000-C000-000000000046")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IStorage
    {
        void CreateStream(string pwcsName, uint grfMode, uint reserved1, uint reserved2, out IStream ppstm);
        void OpenStream(string pwcsName, IntPtr reserved1, uint grfMode, uint reserved2, out IStream ppstm);
        void CreateStorage(string pwcsName, uint grfMode, uint reserved1, uint reserved2, out IStorage ppstg);
        void OpenStorage(string pwcsName, IStorage pstgPriority, uint grfMode, IntPtr snbExclude, uint reserved, out IStorage ppstg);
        void CopyTo(uint ciidExclude, Guid rgiidExclude, IntPtr snbExclude, IStorage pstgDest);
        void MoveElementTo(string pwcsName, IStorage pstgDest, string pwcsNewName, uint grfFlags);
        void Commit(uint grfCommitFlags);
        void Revert();
        void EnumElements(uint reserved1, IntPtr reserved2, uint reserved3, out IEnumSTATSTG ppenum);
        void DestroyElement(string pwcsName);
        void RenameElement(string pwcsOldName, string pwcsNewName);
        void SetElementTimes(string pwcsName, System.Runtime.InteropServices.ComTypes.FILETIME pctime, System.Runtime.InteropServices.ComTypes.FILETIME patime, System.Runtime.InteropServices.ComTypes.FILETIME pmtime);
        void SetClass(Guid clsid);
        void SetStateBits(uint grfStateBits, uint grfMask);
        void Stat(out System.Runtime.InteropServices.ComTypes.STATSTG pstatstg, uint grfStatFlag);
    }

    #endregion

    internal class ComStorage : IComStorage
    {
        private IStorage m_Storage;
        private bool m_IsWritable;
        
        public IStorage Storage
        {
            get
            {
                return m_Storage;
            }
        }

        public ComStorage(IStorage storage, bool writable)
        {
            if (storage == null)
            {
                throw new ArgumentNullException(nameof(storage));
            }

            m_IsWritable = writable;
            m_Storage = storage;
        }

        public IComStorage TryOpenStorage(string storageName, bool createIfNotExist)
        {
            try
            {
                IStorage storage;
                
                m_Storage.OpenStorage(storageName, null,
                    (uint)Mode, IntPtr.Zero, 0, out storage);

                return new ComStorage(storage, m_IsWritable);
            }
            catch
            {
                if (createIfNotExist)
                {
                    return CreateStorage(storageName);
                }
                else
                {
                    return null;
                }
            }
        }

        public Stream TryOpenStream(string streamName, bool createIfNotExist)
        {
            try
            {
                IStream stream = null;

                m_Storage.OpenStream(streamName,
                    IntPtr.Zero, (uint)Mode, 0, out stream);

                return new ComStream(stream, m_IsWritable);
            }
            catch
            {
                if (createIfNotExist)
                {
                    return CreateStream(streamName);
                }
                else
                {
                    return null;
                }
            }
        }

        public string[] GetSubStreamNames()
        {
            return EnumElements()
                .Where(e => e.type == (int)STGTY.STGTY_STREAM)
                .Select(e => e.pwcsName).ToArray();
        }

        public string[] GetSubStorageNames()
        {
            return EnumElements()
                .Where(e => e.type == (int)STGTY.STGTY_STORAGE)
                .Select(e => e.pwcsName).ToArray();
        }
        
        private ComStream CreateStream(string streamName)
        {
            IStream stream = null;

            m_Storage.CreateStream(streamName,
                (uint)STGM.STGM_CREATE | (uint)STGM.STGM_SHARE_EXCLUSIVE | (uint)STGM.STGM_WRITE,
                0, 0, out stream);

            return new ComStream(stream, m_IsWritable);
        }

        private IComStorage CreateStorage(string storageName)
        {
            IStorage storage = null;

            m_Storage.CreateStorage(storageName,
                (uint)STGM.STGM_CREATE | (uint)STGM.STGM_SHARE_EXCLUSIVE | (uint)STGM.STGM_WRITE,
                0, 0, out storage);

            return new ComStorage(storage, m_IsWritable);
        }

        private IEnumerable<System.Runtime.InteropServices.ComTypes.STATSTG> EnumElements()
        {
            IEnumSTATSTG ssenum = null;

            m_Storage.EnumElements(0, IntPtr.Zero, 0, out ssenum);
            
            var ssstruct = new System.Runtime.InteropServices.ComTypes.STATSTG[1];

            uint numReturned;

            do
            {
                ssenum.Next(1, ssstruct, out numReturned);

                if (numReturned != 0)
                {
                    yield return ssstruct[0];
                }
            } while (numReturned > 0);
        }

        public void Close()
        {
            if (m_Storage != null)
            {
                if (m_IsWritable)
                {
                    m_Storage.Commit(0);
                }

                Marshal.ReleaseComObject(m_Storage);
                m_Storage = null;
                GC.SuppressFinalize(this);
            }
        }

        private STGM Mode
        {
            get
            {
                var mode = STGM.STGM_SHARE_EXCLUSIVE;

                if (m_IsWritable)
                {
                    mode |= STGM.STGM_READWRITE;
                }

                return mode;
            }
        }

        public void RemoveSubElement(string name)
        {
            m_Storage.DestroyElement(name);
        }

        public void Dispose()
        {
            Close();
        }
    }
}
