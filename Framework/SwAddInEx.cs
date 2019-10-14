//**********************
//SwEx.AddIn - development tools for SOLIDWORKS add-ins
//Copyright(C) 2019 www.codestack.net
//License: https://github.com/codestackdev/swex-addin/blob/master/LICENSE
//Product URL: https://www.codestack.net/labs/solidworks/swex/add-in/
//**********************

using CodeStack.SwEx.AddIn.Attributes;
using CodeStack.SwEx.AddIn.Base;
using CodeStack.SwEx.AddIn.Core;
using CodeStack.SwEx.AddIn.Delegates;
using CodeStack.SwEx.AddIn.Enums;
using CodeStack.SwEx.AddIn.Exceptions;
using CodeStack.SwEx.AddIn.Helpers;
using CodeStack.SwEx.AddIn.Icons;
using CodeStack.SwEx.AddIn.Modules;
using CodeStack.SwEx.Common.Base;
using CodeStack.SwEx.Common.Diagnostics;
using CodeStack.SwEx.Common.Icons;
using CodeStack.SwEx.Common.Reflection;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using SolidWorks.Interop.swpublished;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace CodeStack.SwEx.AddIn
{
    /// <inheritdoc/>
    [ComVisible(true)]
    public abstract class SwAddInEx : ISwAddin, ISwAddInEx
    {
        #region Registration

        private static RegistrationHelper m_RegHelper;

        private static RegistrationHelper GetRegistrationHelper(Type moduleType)
        {
            return m_RegHelper ?? (m_RegHelper = new RegistrationHelper(LoggerFactory.Create(moduleType)));
        }

        /// <summary>
        /// COM Registration entry function
        /// </summary>
        /// <param name="t">Type</param>
        [ComRegisterFunction]
        public static void RegisterFunction(Type t)
        {
            if (t.TryGetAttribute<AutoRegisterAttribute>() != null)
            {
                GetRegistrationHelper(t).Register(t);
            }
        }

        /// <summary>
        /// COM Unregistration entry function
        /// </summary>
        /// <param name="t">Type</param>
        [ComUnregisterFunction]
        public static void UnregisterFunction(Type t)
        {
            if (t.TryGetAttribute<AutoRegisterAttribute>() != null)
            {
                GetRegistrationHelper(t).Unregister(t);
            }
        }

        #endregion

        /// <summary>
        /// Pointer to SOLIDWORKS application
        /// </summary>
        protected ISldWorks App { get; private set; }

        /// <summary>
        /// Pointer to command group which holding the add-in commands
        /// </summary>
        protected ICommandManager CmdMgr
        {
            get
            {
                return m_CmdMgrModule.CmdMgr;
            }
        }

        /// <summary>
        /// Add-ins cookie (id)
        /// </summary>
        protected int AddInCookie { get; private set; }

        public ILogger Logger { get; }

        private CommandManagerModule m_CmdMgrModule;
        private TaskPaneModule m_TaskPaneModule;
        private DocumentsHandlerModule m_DocsHandlerModule;

        public SwAddInEx()
        {
            Logger = LoggerFactory.Create(this);
        }

        /// <summary>SOLIDWORKS add-in entry function</summary>
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool ConnectToSW(object ThisSW, int cookie)
        {
            Logger.Log("Loading add-in");

            try
            {
                App = ThisSW as ISldWorks;
                AddInCookie = cookie;

                App.SetAddinCallbackInfo(0, this, AddInCookie);

                m_CmdMgrModule = new CommandManagerModule(App, AddInCookie, Logger);
                m_TaskPaneModule = new TaskPaneModule(App, Logger);
                m_DocsHandlerModule = new DocumentsHandlerModule(App, Logger);

                return OnConnect();
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
                throw;
            }
        }

        /// <summary>
        /// Command click callback
        /// </summary>
        /// <param name="cmdId">Command tag</param>
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void OnCommandClick(string cmdId)
        {
            m_CmdMgrModule.HandleCommandClick(cmdId);
        }

        /// <summary>
        /// Command enable callback
        /// </summary>
        /// <param name="cmdId">Command tag</param>
        /// <returns>State</returns>
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public int OnCommandEnable(string cmdId)
        {
            return m_CmdMgrModule.HandleCommandEnable(cmdId);
        }

        /// <summary>
        /// SOLIDWORKS unload add-in callback
        /// </summary>
        /// <returns></returns>
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool DisconnectFromSW()
        {
            Logger.Log("Unloading add-in");

            try
            {
                m_CmdMgrModule.Dispose();
                m_TaskPaneModule.Dispose();
                m_DocsHandlerModule.Dispose();
                
                var res = OnDisconnect();

                if (Marshal.IsComObject(App))
                {
                    Marshal.ReleaseComObject(App);
                }

                App = null;

                GC.Collect();
                GC.WaitForPendingFinalizers();

                GC.Collect();
                GC.WaitForPendingFinalizers();

                return res;
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
                throw;
            }
        }

        /// <inheritdoc/>
        public virtual bool OnConnect()
        {
            return true;
        }

        /// <inheritdoc/>
        public virtual bool OnDisconnect()
        {
            return true;
        }

        /// <inheritdoc/>
        /// <exception cref="GroupIdAlreadyExistsException"/>
        /// <exception cref="InvalidMenuToolbarOptionsException"/>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="CallbackNotSpecifiedException"/>
        public CommandGroup AddCommandGroup<TCmdEnum>(Action<TCmdEnum> callback,
            EnableMethodDelegate<TCmdEnum> enable = null)
            where TCmdEnum : IComparable, IFormattable, IConvertible
        {
            return m_CmdMgrModule.AddCommandGroupOrContextMenu(callback, false, 0, enable);
        }

        /// <inheritdoc/>
        /// <exception cref="GroupIdAlreadyExistsException"/>
        /// <exception cref="InvalidMenuToolbarOptionsException"/>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="CallbackNotSpecifiedException"/>
        public CommandGroup AddContextMenu<TCmdEnum>(Action<TCmdEnum> callback,
            swSelectType_e contextMenuSelectType = swSelectType_e.swSelEVERYTHING,
            EnableMethodDelegate<TCmdEnum> enable = null)
            where TCmdEnum : IComparable, IFormattable, IConvertible
        {
            return m_CmdMgrModule.AddCommandGroupOrContextMenu(callback, true, contextMenuSelectType, enable);
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public CommandGroup AddCommandGroup(ICommandGroupSpec cmdBar)
        {
            return m_CmdMgrModule.AddCommandGroupOrContextMenu(
               cmdBar, false, 0);
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public CommandGroup AddContextMenu(ICommandGroupSpec cmdBar,
            swSelectType_e contextMenuSelectType = swSelectType_e.swSelEVERYTHING)
        {
            return m_CmdMgrModule.AddCommandGroupOrContextMenu(
               cmdBar, true, contextMenuSelectType);
        }

        /// <inheritdoc/>
        public IDocumentsHandler<TDocHandler> CreateDocumentsHandler<TDocHandler>()
            where TDocHandler : IDocumentHandler, new()
        {
            return m_DocsHandlerModule.CreateDocumentsHandler<TDocHandler>();
        }

        /// <inheritdoc/>
        public IDocumentsHandler<DocumentHandler> CreateDocumentsHandler()
        {
            return CreateDocumentsHandler<DocumentHandler>();
        }

        /// <inheritdoc/>
        public ITaskpaneView CreateTaskPane<TControl>(out TControl ctrl)
            where TControl : UserControl, new()
        {
            return CreateTaskPane<TControl, EmptyTaskPaneCommands_e>(null, out ctrl);
        }

        /// <inheritdoc/>
        public ITaskpaneView CreateTaskPane<TControl, TCmdEnum>(Action<TCmdEnum> cmdHandler, out TControl ctrl)
            where TControl : UserControl, new()
            where TCmdEnum : IComparable, IFormattable, IConvertible
        {
            return m_TaskPaneModule.CreateTaskPane(cmdHandler, out ctrl);
        }
    }
}
