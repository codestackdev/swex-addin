//**********************
//SwEx.AddIn - development tools for SOLIDWORKS add-ins
//Copyright(C) 2019 www.codestack.net
//License: https://github.com/codestackdev/swex-addin/blob/master/LICENSE
//Product URL: https://www.codestack.net/labs/solidworks/swex/add-in/
//**********************

using CodeStack.SwEx.AddIn.Core;
using CodeStack.SwEx.AddIn.Delegates;
using CodeStack.SwEx.AddIn.Enums;
using CodeStack.SwEx.Common.Enums;
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
            internal PropertiesList(ICustomPropertyManager prpsMgr, ISldWorks app) : base(StringComparer.CurrentCultureIgnoreCase)
            {
                var prpNames = prpsMgr.GetNames() as string[];

                if (prpNames != null)
                {
                    foreach (var prpName in prpNames)
                    {
                        string val;
                        string resVal;
                        if (app.GetVersion() >= SwVersion_e.Sw2014)
                        {
                            bool wasRes;
                            prpsMgr.Get5(prpName, true, out val, out resVal, out wasRes);
                        }
                        else
                        {
                            prpsMgr.Get4(prpName, true, out val, out resVal);
                        }
                        Add(prpName, val);
                    }
                }
            }
        }

        private class PropertiesSet : Dictionary<string, PropertiesList>
        {
            internal PropertiesSet(ISldWorks app, IModelDoc2 model) : base(StringComparer.CurrentCultureIgnoreCase)
            {
                Add("", new PropertiesList(model.Extension.CustomPropertyManager[""], app));

                var confNames = model.GetConfigurationNames() as string[];

                if (confNames != null)
                {
                    foreach (var confName in confNames)
                    {
                        Add(confName, new PropertiesList(model.Extension.CustomPropertyManager[confName], app));
                    }
                }
            }
        }

        public event CustomPropertyModifyDelegate CustomPropertiesModified;

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
                    FindDifferences(m_CurPrpsSet, new PropertiesSet(m_App, m_Model));
                    m_CurrentSummaryHandle = IntPtr.Zero;
                    m_CurPrpsSet = null;
                }
            }

            return 0;
        }

        private void FindDifferences(PropertiesSet oldSet, PropertiesSet newSet)
        {
            var modData = new List<CustomPropertyModifyData>();

            foreach (var conf in oldSet.Keys)
            {
                var oldPrsList = oldSet[conf];
                var newPrsList = newSet[conf];
                
                var addedPrpNames = newPrsList.Keys.Except(oldPrsList.Keys);
                
                modData.AddRange(addedPrpNames
                    .Select(newPrpName => new CustomPropertyModifyData(
                        CustomPropertyChangeAction_e.Add, newPrpName, conf, newPrsList[newPrpName])));

                var removedPrpNames = oldPrsList.Keys.Except(newPrsList.Keys);

                modData.AddRange(removedPrpNames
                    .Select(deletedPrpName => new CustomPropertyModifyData(
                        CustomPropertyChangeAction_e.Delete, deletedPrpName, conf, oldPrsList[deletedPrpName])));

                var commonPrpNames = oldPrsList.Keys.Intersect(newPrsList.Keys);

                modData.AddRange(commonPrpNames.Where(prpName => newPrsList[prpName] != oldPrsList[prpName])
                    .Select(prpName => new CustomPropertyModifyData(
                        CustomPropertyChangeAction_e.Modify, prpName, conf, newPrsList[prpName])));
            }

            if (modData.Any())
            {
                CustomPropertiesModified?.Invoke(m_DocHandler, modData.ToArray());
            }
        }

        private int OnAddCustomPropertyNotify(string propName, string configuration, string value, int valueType)
        {
            CustomPropertiesModified?.Invoke(m_DocHandler, 
                new CustomPropertyModifyData[]
                {
                    new CustomPropertyModifyData(CustomPropertyChangeAction_e.Add, propName, configuration, value)
                });

            return 0;
        }

        private int OnDeleteCustomPropertyNotify(string propName, string configuration, string value, int valueType)
        {
            CustomPropertiesModified?.Invoke(m_DocHandler, new CustomPropertyModifyData[] 
            {
                new CustomPropertyModifyData(CustomPropertyChangeAction_e.Delete, propName, configuration, value)
            });

            return 0;
        }

        private int OnChangeCustomPropertyNotify(string propName, string configuration, string oldValue, string newValue, int valueType)
        {
            CustomPropertiesModified?.Invoke(m_DocHandler, new CustomPropertyModifyData[] 
            {
                new CustomPropertyModifyData(CustomPropertyChangeAction_e.Modify, propName, configuration, newValue)
            });

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
                m_CurPrpsSet = new PropertiesSet(m_App, m_Model);
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