//**********************
//SwEx.AddIn - development tools for SOLIDWORKS add-ins
//Copyright(C) 2019 www.codestack.net
//License: https://github.com/codestackdev/swex-addin/blob/master/LICENSE
//Product URL: https://www.codestack.net/labs/solidworks/swex/add-in/
//**********************

using CodeStack.SwEx.AddIn.Core;
using SolidWorks.Interop.sldworks;
using System;
using System.Runtime.Serialization;

namespace CodeStack.SwEx.AddIn.Helpers
{
    /// <summary>
    /// Wrapper for lazy handling the events
    /// </summary>
    /// <typeparam name="TDel">Event delegate</typeparam>
    internal abstract class EventsHandler<TDel> : IDisposable
                where TDel : class, ICloneable, ISerializable
    {
        protected const int S_OK = 0;
        protected const int S_FALSE = 1;

        public TDel Delegate { get; set; }

        protected readonly DocumentHandler m_DocHandler;

        private bool m_IsSubscribed;

        internal EventsHandler(DocumentHandler docHandler)
        {
            if (!typeof(Delegate).IsAssignableFrom(typeof(TDel)))
            {
                throw new InvalidCastException($"{typeof(TDel).FullName} is not a delegate");
            }

            m_DocHandler = docHandler;

            m_IsSubscribed = false;
        }

        private void SubscribeIfNeeded()
        {
            if (!m_IsSubscribed)
            {
                if (m_DocHandler.Model is PartDoc)
                {
                    SubscribePartEvents(m_DocHandler.Model as PartDoc);
                }
                else if (m_DocHandler.Model is AssemblyDoc)
                {
                    SubscribeAssemblyEvents(m_DocHandler.Model as AssemblyDoc);
                }
                else if (m_DocHandler.Model is DrawingDoc)
                {
                    SubscribeDrawingEvents(m_DocHandler.Model as DrawingDoc);
                }
                else
                {
                    throw new InvalidCastException("Model type is null or invalid");
                }

                m_IsSubscribed = true;
            }
        }

        private void UnsubscribeIfNeeded()
        {
            if (m_IsSubscribed)
            {
                if (m_DocHandler.Model is PartDoc)
                {
                    UnsubscribePartEvents(m_DocHandler.Model as PartDoc);
                }
                else if (m_DocHandler.Model is AssemblyDoc)
                {
                    UnsubscribeAssemblyEvents(m_DocHandler.Model as AssemblyDoc);
                }
                else if (m_DocHandler.Model is DrawingDoc)
                {
                    UnsubscribeDrawingEvents(m_DocHandler.Model as DrawingDoc);
                }
                else
                {
                    throw new InvalidCastException("Model type is null or invalid");
                }

                m_IsSubscribed = false;
            }
        }

        internal void Attach(TDel del)
        {
            SubscribeIfNeeded();
            OnAttach(del);
        }

        internal void Detach(TDel del)
        {
            OnDetach(del);

            if (Delegate == null)
            {
                UnsubscribeIfNeeded();
            }
        }

        public void Dispose()
        {
            UnsubscribeIfNeeded();
        }

        protected abstract void SubscribePartEvents(PartDoc part);
        protected abstract void SubscribeAssemblyEvents(AssemblyDoc assm);
        protected abstract void SubscribeDrawingEvents(DrawingDoc draw);
        protected abstract void UnsubscribePartEvents(PartDoc part);
        protected abstract void UnsubscribeAssemblyEvents(AssemblyDoc assm);
        protected abstract void UnsubscribeDrawingEvents(DrawingDoc draw);
        
        protected abstract void OnAttach(TDel del);
        protected abstract void OnDetach(TDel del);
    }
}
