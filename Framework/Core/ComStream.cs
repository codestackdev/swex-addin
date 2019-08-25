//**********************
//SwEx.AddIn - development tools for SOLIDWORKS add-ins
//Copyright(C) 2019 www.codestack.net
//License: https://github.com/codestackdev/swex-addin/blob/master/LICENSE
//Product URL: https://www.codestack.net/labs/solidworks/swex/add-in/
//**********************

using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices.ComTypes;

namespace CodeStack.SwEx.AddIn.Core
{
    [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
    public class ComStream : Stream
    {
        private readonly IStream m_ComStream;
        private readonly bool m_Commit;

        private bool m_IsWritable;

        public IStream Stream
        {
            get
            {
                return m_ComStream;
            }
        }

        public override bool CanRead
        {
            get
            {
                return true;
            }
        }

        public override bool CanSeek
        {
            get
            {
                return true;
            }
        }

        public override bool CanWrite
        {
            get
            {
                return m_IsWritable;
            }
        }

        public override long Length
        {
            get
            {
                const int STATSFLAG_NONAME = 1;

                STATSTG statstg;

                m_ComStream.Stat(out statstg, STATSFLAG_NONAME);

                return statstg.cbSize;
            }
        }

        public override long Position
        {
            get
            {
                return Seek(0, SeekOrigin.Current);
            }
            set
            {
                Seek(value, SeekOrigin.Begin);
            }
        }

        public ComStream(IStream comStream, bool writable, bool commit = true)
        {
            if (comStream == null)
            {
                throw new ArgumentNullException(nameof(comStream));
            }

            m_ComStream = comStream;
            m_Commit = commit;
            m_IsWritable = writable;
        }

        public override void Flush()
        {
            if (m_Commit)
            {
                const int STGC_DEFAULT = 0;

                m_ComStream.Commit(STGC_DEFAULT);
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int bytesRead = 0;
            object boxBytesRead = bytesRead; //must be boxed otherwise - will fail
            var hObject = default(System.Runtime.InteropServices.GCHandle);

            try
            {
                hObject = System.Runtime.InteropServices.GCHandle.Alloc(boxBytesRead,
                    System.Runtime.InteropServices.GCHandleType.Pinned);

                var pBytesRead = hObject.AddrOfPinnedObject();

                if (offset != 0)
                {
                    var tmpBuffer = new byte[count];
                    m_ComStream.Read(tmpBuffer, count, pBytesRead);
                    bytesRead = Convert.ToInt32(boxBytesRead);
                    Array.Copy(tmpBuffer, 0, buffer, offset, bytesRead);
                }
                else
                {
                    m_ComStream.Read(buffer, count, pBytesRead);
                    bytesRead = Convert.ToInt32(boxBytesRead);
                }
            }
            finally
            {
                if (hObject.IsAllocated)
                {
                    hObject.Free();
                }
            }

            return bytesRead;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            long curPosition = 0;
            var boxCurPosition = curPosition; //must be boxed otherwise - will fail
            var hObject = default(System.Runtime.InteropServices.GCHandle);

            try
            {
                hObject = System.Runtime.InteropServices.GCHandle.Alloc(
                    boxCurPosition, System.Runtime.InteropServices.GCHandleType.Pinned);

                var pCurPosition = hObject.AddrOfPinnedObject();

                m_ComStream.Seek(offset, (int)origin, pCurPosition);
                curPosition = Convert.ToInt64(boxCurPosition);
            }
            finally
            {
                if (hObject.IsAllocated)
                {
                    hObject.Free();
                }
            }

            return curPosition;
        }

        public override void SetLength(long value)
        {
            m_ComStream.SetSize(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (offset != 0)
            {
                var bufferSize = buffer.Length - offset;
                var tmpBuffer = new byte[bufferSize];
                Array.Copy(buffer, offset, tmpBuffer, 0, bufferSize);
                m_ComStream.Write(tmpBuffer, bufferSize, IntPtr.Zero);
            }
            else
            {
                m_ComStream.Write(buffer, count, IntPtr.Zero);
            }
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing)
                {
                    m_IsWritable = false;
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        ~ComStream()
        {
            Dispose(false);
        }
    }
}
