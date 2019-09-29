//**********************
//SwEx.AddIn - development tools for SOLIDWORKS add-ins
//Copyright(C) 2019 www.codestack.net
//License: https://github.com/codestackdev/swex-addin/blob/master/LICENSE
//Product URL: https://www.codestack.net/labs/solidworks/swex/add-in/
//**********************

using CodeStack.SwEx.AddIn.Core;
using SolidWorks.Interop.sldworks;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace CodeStack.SwEx.AddIn.Helpers
{
    internal class CustomPropertiesEventsHandler : IDisposable
    {
        private class PropertiesList : Dictionary<string, string>
        {
            internal PropertiesList(ICustomPropertyManager prpsMgr) : base(StringComparer.CurrentCultureIgnoreCase)
            {
                var prpNames = prpsMgr.GetNames() as string[];

                if (prpNames != null)
                {
                    foreach (var prpName in prpNames)
                    {
                        string val;
                        string resVal;
                        bool wasRes;
                        prpsMgr.Get5(prpName, true, out val, out resVal, out wasRes);
                        Add(prpName, val);
                    }
                }
            }
        }

        private class PropertiesSet : Dictionary<string, PropertiesList>
        {
            internal PropertiesSet(IModelDoc2 model) : base(StringComparer.CurrentCultureIgnoreCase)
            {
                Add("", new PropertiesList(model.Extension.CustomPropertyManager[""]));

                var confNames = model.GetConfigurationNames() as string[];

                if (confNames != null)
                {
                    foreach (var confName in confNames)
                    {
                        Add(confName, new PropertiesList(model.Extension.CustomPropertyManager[confName]));
                    }
                }
            }
        }

        public event DocumentCustomPropertyModifiedDelegate PropertyChanged;

        #region WinAPI

        private delegate bool EnumWindowProc(IntPtr handle, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int GetWindowTextLength(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool EnumThreadWindows(uint threadId, EnumWindowProc enumProc, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool IsWindow(IntPtr hWnd);

        #endregion

        private readonly DocumentHandler m_DocHandler;
        private readonly ISldWorks m_App;
        private readonly IModelDoc2 m_Model;

        private IntPtr m_CurrentSummaryHandle;

        private PropertiesSet m_CurPrpsSet;

        public CustomPropertiesEventsHandler(DocumentHandler docHandler, ISldWorks app, IModelDoc2 model)
        {
            m_DocHandler = docHandler;
            m_App = app;
            m_Model = model;

            (m_App as SldWorks).CommandCloseNotify += OnCommandCloseNotify;
            (m_App as SldWorks).OnIdleNotify += OnIdleNotify;

            if (model is PartDoc)
            {
                (model as PartDoc).AddCustomPropertyNotify += OnAddCustomPropertyNotify;
                (model as PartDoc).DeleteCustomPropertyNotify += OnDeleteCustomPropertyNotify;
                (model as PartDoc).ChangeCustomPropertyNotify += OnChangeCustomPropertyNotify;
            }
            else if (model is AssemblyDoc)
            {
                (model as AssemblyDoc).AddCustomPropertyNotify += OnAddCustomPropertyNotify;
                (model as AssemblyDoc).DeleteCustomPropertyNotify += OnDeleteCustomPropertyNotify;
                (model as AssemblyDoc).ChangeCustomPropertyNotify += OnChangeCustomPropertyNotify;
            }
            else if (model is DrawingDoc)
            {
                (model as DrawingDoc).AddCustomPropertyNotify += OnAddCustomPropertyNotify;
                (model as DrawingDoc).DeleteCustomPropertyNotify += OnDeleteCustomPropertyNotify;
                (model as DrawingDoc).ChangeCustomPropertyNotify += OnChangeCustomPropertyNotify;
            }
            else
            {
                throw new NotSupportedException();
            }

            CaptureCurrentProperties();
        }

        private int OnIdleNotify()
        {
            if (m_CurrentSummaryHandle != IntPtr.Zero)
            {
                if (!IsWindow(m_CurrentSummaryHandle))
                {
                    FindDifferences(m_CurPrpsSet, new PropertiesSet(m_Model));
                    m_CurrentSummaryHandle = IntPtr.Zero;
                    m_CurPrpsSet = null;
                }
            }

            return 0;
        }

        private void FindDifferences(PropertiesSet oldSet, PropertiesSet newSet)
        {
            foreach (var conf in oldSet.Keys)
            {
                var oldPrsList = oldSet[conf];
                var newPrsList = newSet[conf];

                var addedPrpNames = newPrsList.Keys.Except(oldPrsList.Keys);

                foreach (var newPrpName in addedPrpNames)
                {
                    PropertyChanged?.Invoke(m_DocHandler, CustomPropertyChangeAction_e.Added, newPrpName, conf, newPrsList[newPrpName]);
                }

                var removedPrpNames = oldPrsList.Keys.Except(newPrsList.Keys);

                foreach (var deletedPrpName in removedPrpNames)
                {
                    PropertyChanged?.Invoke(m_DocHandler, CustomPropertyChangeAction_e.Deleted, deletedPrpName, conf, oldPrsList[deletedPrpName]);
                }

                var commonPrpNames = oldPrsList.Keys.Intersect(newPrsList.Keys);

                foreach (var prpName in commonPrpNames)
                {
                    if (newPrsList[prpName] != oldPrsList[prpName])
                    {
                        PropertyChanged?.Invoke(m_DocHandler, CustomPropertyChangeAction_e.Modified, prpName, conf, newPrsList[prpName]);
                    }
                }
            }
        }

        private int OnAddCustomPropertyNotify(string propName, string Configuration, string Value, int valueType)
        {
            PropertyChanged?.Invoke(m_DocHandler, CustomPropertyChangeAction_e.Added, propName, Configuration, Value);
            return 0;
        }

        private int OnDeleteCustomPropertyNotify(string propName, string Configuration, string Value, int valueType)
        {
            PropertyChanged?.Invoke(m_DocHandler, CustomPropertyChangeAction_e.Deleted, propName, Configuration, Value);
            return 0;
        }

        private int OnChangeCustomPropertyNotify(string propName, string Configuration, string oldValue, string NewValue, int valueType)
        {
            PropertyChanged?.Invoke(m_DocHandler, CustomPropertyChangeAction_e.Modified, propName, Configuration, NewValue);
            return 0;
        }

        private int OnCommandCloseNotify(int Command, int reason)
        {
            const int swCommands_File_Summaryinfo = 963;

            if (Command == swCommands_File_Summaryinfo)
            {
                if (!CaptureCurrentProperties())
                {
                    throw new Exception("Failed to find the summary information dialog");
                }
            }

            return 0;
        }

        private bool CaptureCurrentProperties()
        {
            var handle = GetSummaryInfoDialogHandle();

            if (handle != IntPtr.Zero)
            {
                m_CurPrpsSet = new PropertiesSet(m_Model);
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool FindSymmaryInfoDialog(IntPtr handle, IntPtr lParam)
        {
            var captionLength = GetWindowTextLength(handle) + 1;
            var caption = new StringBuilder(captionLength);

            if (GetWindowText(handle, caption, captionLength) > 0)
            {
                //TODO: implement support for other languages
                if (caption.ToString() == "Summary Information")
                {
                    var clsName = new StringBuilder(260);

                    GetClassName(handle, clsName, clsName.Capacity);

                    if (clsName.ToString() == "#32770")
                    {
                        m_CurrentSummaryHandle = handle;
                    }
                }
            }

            return true;
        }

        private IntPtr GetSummaryInfoDialogHandle()
        {
            m_CurrentSummaryHandle = IntPtr.Zero;

            var prc = Process.GetProcessById(m_App.GetProcessID());

            for (int i = 0; i < prc.Threads.Count; i++)
            {
                var threadId = (uint)prc.Threads[i].Id;
                EnumThreadWindows(threadId, FindSymmaryInfoDialog, IntPtr.Zero);
            }

            return m_CurrentSummaryHandle;
        }

        public void Dispose()
        {
        }
    }
}