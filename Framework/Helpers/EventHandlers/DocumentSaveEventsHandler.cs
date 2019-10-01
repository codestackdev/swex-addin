//**********************
//SwEx.AddIn - development tools for SOLIDWORKS add-ins
//Copyright(C) 2019 www.codestack.net
//License: https://github.com/codestackdev/swex-addin/blob/master/LICENSE
//Product URL: https://www.codestack.net/labs/solidworks/swex/add-in/
//**********************

using CodeStack.SwEx.AddIn.Delegates;
using SolidWorks.Interop.sldworks;
using CodeStack.SwEx.AddIn.Core;
using CodeStack.SwEx.AddIn.Enums;

namespace CodeStack.SwEx.AddIn.Helpers.EventHandlers
{
    internal class DocumentSaveEventsHandler : EventsHandler<DocumentSaveDelegate>
    {
        public DocumentSaveEventsHandler(DocumentHandler docHandler) : base(docHandler)
        {
        }

        protected override void OnAttach(DocumentSaveDelegate del)
        {
            Delegate += del;
        }

        protected override void OnDetach(DocumentSaveDelegate del)
        {
            Delegate -= del;
        }

        protected override void SubscribeAssemblyEvents(AssemblyDoc assm)
        {
            assm.AutoSaveNotify += OnAutoSaveNotify;
            assm.FileSaveAsNotify2 += OnFileSaveAsNotify2;
            assm.FileSaveNotify += OnFileSaveNotify;
            assm.FileSavePostCancelNotify += OnFileSavePostCancelNotify;
            assm.FileSavePostNotify += OnFileSavePostNotify;
        }

        protected override void SubscribeDrawingEvents(DrawingDoc draw)
        {
            draw.AutoSaveNotify += OnAutoSaveNotify;
            draw.FileSaveAsNotify2 += OnFileSaveAsNotify2;
            draw.FileSaveNotify += OnFileSaveNotify;
            draw.FileSavePostCancelNotify += OnFileSavePostCancelNotify;
            draw.FileSavePostNotify += OnFileSavePostNotify;
        }

        protected override void SubscribePartEvents(PartDoc part)
        {
            part.AutoSaveNotify += OnAutoSaveNotify;
            part.FileSaveAsNotify2 += OnFileSaveAsNotify2;
            part.FileSaveNotify += OnFileSaveNotify;
            part.FileSavePostCancelNotify += OnFileSavePostCancelNotify;
            part.FileSavePostNotify += OnFileSavePostNotify;
        }

        protected override void UnsubscribeAssemblyEvents(AssemblyDoc assm)
        {
            assm.AutoSaveNotify -= OnAutoSaveNotify;
            assm.FileSaveAsNotify2 -= OnFileSaveAsNotify2;
            assm.FileSaveNotify -= OnFileSaveNotify;
            assm.FileSavePostCancelNotify -= OnFileSavePostCancelNotify;
            assm.FileSavePostNotify -= OnFileSavePostNotify;
        }

        protected override void UnsubscribeDrawingEvents(DrawingDoc draw)
        {
            draw.AutoSaveNotify -= OnAutoSaveNotify;
            draw.FileSaveAsNotify2 -= OnFileSaveAsNotify2;
            draw.FileSaveNotify -= OnFileSaveNotify;
            draw.FileSavePostCancelNotify -= OnFileSavePostCancelNotify;
            draw.FileSavePostNotify -= OnFileSavePostNotify;
        }

        protected override void UnsubscribePartEvents(PartDoc part)
        {
            part.AutoSaveNotify -= OnAutoSaveNotify;
            part.FileSaveAsNotify2 -= OnFileSaveAsNotify2;
            part.FileSaveNotify -= OnFileSaveNotify;
            part.FileSavePostCancelNotify -= OnFileSavePostCancelNotify;
            part.FileSavePostNotify -= OnFileSavePostNotify;
        }

        private int OnFileSavePostNotify(int saveType, string fileName)
        {
            return Delegate.Invoke(m_DocHandler, fileName, SaveAction_e.PostSave) ? S_OK : S_FALSE;
        }

        private int OnFileSavePostCancelNotify()
        {
            return Delegate.Invoke(m_DocHandler, "", SaveAction_e.PostCancel) ? S_OK : S_FALSE;
        }

        private int OnFileSaveNotify(string fileName)
        {
            return Delegate.Invoke(m_DocHandler, fileName, SaveAction_e.PreSave) ? S_OK : S_FALSE;
        }

        private int OnFileSaveAsNotify2(string fileName)
        {
            return Delegate.Invoke(m_DocHandler, fileName, SaveAction_e.SaveAs) ? S_OK : S_FALSE;
        }

        private int OnAutoSaveNotify(string fileName)
        {
            return Delegate.Invoke(m_DocHandler, fileName, SaveAction_e.AutoSave) ? S_OK : S_FALSE;
        }
    }
}
