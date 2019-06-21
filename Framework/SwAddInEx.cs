//**********************
//SwEx.AddIn - development tools for SOLIDWORKS add-ins
//Copyright(C) 2019 www.codestack.net
//License: https://github.com/codestack-net-dev/sw-dev-tools-addin/blob/master/LICENSE
//Product URL: https://www.codestack.net/labs/solidworks/swex/add-in/
//**********************

using CodeStack.SwEx.AddIn.Attributes;
using CodeStack.SwEx.AddIn.Base;
using CodeStack.SwEx.AddIn.Core;
using CodeStack.SwEx.AddIn.Enums;
using CodeStack.SwEx.AddIn.Exceptions;
using CodeStack.SwEx.AddIn.Helpers;
using CodeStack.SwEx.AddIn.Icons;
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

        private class TabCommandInfo
        {
            internal swDocumentTypes_e DocType { get; private set; }
            internal int CmdId { get; private set; }
            internal swCommandTabButtonTextDisplay_e TextType { get; private set; }

            internal TabCommandInfo(swDocumentTypes_e docType, int cmdId,
                swCommandTabButtonTextDisplay_e textType)
            {
                DocType = docType;
                CmdId = cmdId;
                TextType = textType;
            }
        }

        private const string SUB_GROUP_SEPARATOR = "\\";

        /// <summary>
        /// Pointer to SOLIDWORKS application
        /// </summary>
        protected ISldWorks App { get; private set; }

        /// <summary>
        /// Pointer to command group which holding the add-in commands
        /// </summary>
        protected ICommandManager CmdMgr { get; private set; }

        /// <summary>
        /// Add-ins cookie (id)
        /// </summary>
        protected int AddInCookie { get; private set; }

        public ILogger Logger
        {
            get
            {
                return m_Logger;
            }
        }

        private readonly Dictionary<ICommandGroupSpec, CommandGroup> m_CommandGroups;
        private readonly Dictionary<string, ICommandSpec> m_Commands;
        private readonly List<ITaskPaneHandler> m_TaskPanes;
        private readonly List<IDisposable> m_DocsHandlers;

        private readonly ILogger m_Logger;

        public SwAddInEx()
        {
            m_Logger = LoggerFactory.Create(this);
            m_Commands = new Dictionary<string, ICommandSpec>();
            m_CommandGroups = new Dictionary<ICommandGroupSpec, CommandGroup>();
            m_TaskPanes = new List<ITaskPaneHandler>();
            m_DocsHandlers = new List<IDisposable>();
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

                CmdMgr = App.GetCommandManager(AddInCookie);

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
            Logger.Log($"Command clicked: {cmdId}");

            ICommandSpec cmd;

            if (m_Commands.TryGetValue(cmdId, out cmd))
            {
                cmd.OnClick();
            }
            else
            {
                Debug.Assert(false, "All callbacks must be registered");
            }
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
            ICommandSpec cmd;

            if (m_Commands.TryGetValue(cmdId, out cmd))
            {
                return (int)cmd.OnEnable();
            }
            else
            {
                Debug.Assert(false, "All callbacks must be registered");
            }

            return (int)CommandItemEnableState_e.DeselectDisable;
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
                foreach (var grp in m_CommandGroups.Keys)
                {
                    Logger.Log($"Removing group: {grp.Id}");
                    CmdMgr.RemoveCommandGroup(grp.Id);
                }

                m_CommandGroups.Clear();

                for (int i = m_TaskPanes.Count - 1; i >= 0; i--)
                {
                    m_TaskPanes[i].Delete();
                }

                m_TaskPanes.Clear();

                foreach (var docHandler in m_DocsHandlers)
                {
                    docHandler.Dispose();
                }

                m_DocsHandlers.Clear();

                var res = OnDisconnect();

                if (Marshal.IsComObject(CmdMgr))
                {
                    Marshal.ReleaseComObject(CmdMgr);
                }

                CmdMgr = null;

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
            return AddCommandGroup(
                new EnumCommandGroupSpec<TCmdEnum>(App, callback, enable, GetNextAvailableGroupId(), m_CommandGroups.Keys));
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
            return AddContextMenu(
                new EnumCommandGroupSpec<TCmdEnum>(App, callback, enable, GetNextAvailableGroupId(), m_CommandGroups.Keys),
                contextMenuSelectType);
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public CommandGroup AddCommandGroup(ICommandGroupSpec cmdBar)
        {
            return AddCommandGroupOrContextMenu(
               cmdBar, false, 0);
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public CommandGroup AddContextMenu(ICommandGroupSpec cmdBar,
            swSelectType_e contextMenuSelectType = swSelectType_e.swSelEVERYTHING)
        {
            return AddCommandGroupOrContextMenu(
               cmdBar, true, contextMenuSelectType);
        }
        
        public IDocumentsHandler<TDocHandler> CreateDocumentsHandler<TDocHandler>()
            where TDocHandler : IDocumentHandler, new()
        {
            var docsHandler = new DocumentsHandler<TDocHandler>(App, m_Logger);

            m_DocsHandlers.Add(docsHandler);

            return docsHandler;
        }
        
        public ITaskpaneView CreateTaskPane<TControl>(out TControl ctrl)
            where TControl : UserControl, new()
        {
            return CreateTaskPane<TControl, EmptyTaskPaneCommands_e>(null, out ctrl);
        }

        public ITaskpaneView CreateTaskPane<TControl, TCmdEnum>(Action<TCmdEnum> cmdHandler, out TControl ctrl)
            where TControl : UserControl, new()
            where TCmdEnum : IComparable, IFormattable, IConvertible
        {
            var tooltip = "";
            CommandGroupIcon taskPaneIcon = null;

            var getTaskPaneDisplayData = new Action<Type, bool>((t,d) => 
            {
                if (taskPaneIcon == null)
                {
                    taskPaneIcon = DisplayInfoExtractor.ExtractCommandDisplayIcon<TaskPaneIconAttribute, CommandGroupIcon>(
                        t, i => new TaskPaneMasterIcon(i), a => a.Icon, d);
                }

                if (string.IsNullOrEmpty(tooltip))
                {
                    if (!t.TryGetAttribute<DisplayNameAttribute>(a => tooltip = a.DisplayName))
                    {
                        t.TryGetAttribute<DescriptionAttribute>(a => tooltip = a.Description);
                    }
                }
            });
            
            if (typeof(TCmdEnum) != typeof(EmptyTaskPaneCommands_e))
            {
                getTaskPaneDisplayData.Invoke(typeof(TCmdEnum), false);
            }

            getTaskPaneDisplayData.Invoke(typeof(TControl), true);

            ITaskpaneView taskPaneView = null;
            ITaskPaneHandler taskPaneHandler = null;

            m_Logger.Log($"Creating task pane for {typeof(TControl).FullName} type");

            using (var iconConv = new IconsConverter())
            {
                if (App.SupportsHighResIcons(SldWorksExtension.HighResIconsScope_e.TaskPane))
                {
                    var taskPaneIconImages = iconConv.ConvertIcon(taskPaneIcon, true);
                    taskPaneView = App.CreateTaskpaneView3(taskPaneIconImages, tooltip);
                }
                else
                {
                    var taskPaneIconImage = iconConv.ConvertIcon(taskPaneIcon, false)[0];
                    taskPaneView = App.CreateTaskpaneView2(taskPaneIconImage, tooltip);
                }

                taskPaneHandler = new TaskPaneHandler<TCmdEnum>(App, taskPaneView, cmdHandler, iconConv, m_Logger);
            }
            
            if (typeof(TControl).IsComVisible())
            {
                var progId = typeof(TControl).GetProgId();
                ctrl = taskPaneView.AddControl(progId, "") as TControl;

                if (ctrl == null)
                {
                    throw new NullReferenceException(
                        $"Failed to create COM control from {progId}. Make sure that COM component is properly registered");
                }
            }
            else
            {
                ctrl = new TControl();
                ctrl.CreateControl();
                var handle = ctrl.Handle;

                if (!taskPaneView.DisplayWindowFromHandle(handle.ToInt32()))
                {
                    throw new NullReferenceException($"Failed to host .NET control (handle {handle}) in task pane");
                }
            }

            taskPaneHandler.Disposed += OnTaskPaneHandlerDisposed;
            m_TaskPanes.Add(taskPaneHandler);
            
            return taskPaneView;
        }

        private void OnTaskPaneHandlerDisposed(ITaskPaneHandler handler)
        {
            handler.Disposed -= OnTaskPaneHandlerDisposed;
            m_TaskPanes.Remove(handler);
        }

        private int GetNextAvailableGroupId()
        {
            if (m_CommandGroups.Any())
            {
                return m_CommandGroups.Keys.Max(g => g.Id) + 1;
            }
            else
            {
                return 0;
            }
        }

        private CommandGroup AddCommandGroupOrContextMenu(ICommandGroupSpec cmdBar,
            bool isContextMenu, swSelectType_e contextMenuSelectType)
        {
            Logger.Log($"Creating command group: {cmdBar.Id}");

            if (m_CommandGroups.Keys.FirstOrDefault(g => g.Id == cmdBar.Id) != null)
            {
                throw new GroupIdAlreadyExistsException(cmdBar);
            }
            
            var title = GetMenuPath(cmdBar);

            var cmdGroup = CreateCommandGroup(cmdBar.Id, title, cmdBar.Tooltip,
                cmdBar.Commands.Select(c => c.UserId).ToArray(), isContextMenu,
                contextMenuSelectType);

            m_CommandGroups.Add(cmdBar, cmdGroup);

            using (var iconsConv = new IconsConverter())
            {
                CreateIcons(cmdGroup, cmdBar, iconsConv);

                var createdCmds = CreateCommandItems(cmdGroup, cmdBar.Id, cmdBar.Commands);

                var tabGroup = GetRootCommandGroup(cmdBar);

                try
                {
                    CreateCommandTabBox(tabGroup, createdCmds);
                }
                catch(Exception ex)
                {
                    Logger.Log(ex);
                    //not critical error - continue operation
                }
            }

            return cmdGroup;
        }

        private CommandGroup GetRootCommandGroup(ICommandGroupSpec cmdBar)
        {
            var root = cmdBar;

            while (root.Parent != null)
            {
                root = root.Parent;
            }

            return m_CommandGroups[root];
        }

        private string GetMenuPath(ICommandGroupSpec cmdBar)
        {
            var title = new StringBuilder();

            var parent = cmdBar.Parent;

            while (parent != null)
            {
                title.Insert(0, parent.Title + SUB_GROUP_SEPARATOR);
                parent = parent.Parent;
            }

            title.Append(cmdBar.Title);

            return title.ToString();
        }

        private CommandGroup CreateCommandGroup(int groupId, string title, string toolTip,
            int[] knownCmdIDs, bool isContextMenu, swSelectType_e contextMenuSelectType)
        {
            int cmdGroupErr = 0;
            
            object registryIDs;

            var isChanged = true;

            if (CmdMgr.GetGroupDataFromRegistry(groupId, out registryIDs))
            {
                isChanged = !CompareIDs(registryIDs as int[], knownCmdIDs);
            }

            Logger.Log($"Command ids changed: {isChanged}");

            CommandGroup cmdGroup;

            if (isContextMenu)
            {
                cmdGroup = CmdMgr.AddContextMenu(groupId, title);
                cmdGroup.SelectType = (int)contextMenuSelectType;
            }
            else
            {
                cmdGroup = CmdMgr.CreateCommandGroup2(groupId, title, toolTip,
                    toolTip, -1, isChanged, ref cmdGroupErr);

                Logger.Log($"Command group creation result: {(swCreateCommandGroupErrors)cmdGroupErr}");

                Debug.Assert(cmdGroupErr == (int)swCreateCommandGroupErrors.swCreateCommandGroup_Success);
            }
            
            return cmdGroup;
        }

        private void CreateIcons(CommandGroup cmdGroup, ICommandGroupSpec cmdBar, IconsConverter iconsConv)
        {
            var mainIcon = cmdBar.Icon;

            CommandGroupIcon[] iconList = null;

            if (cmdBar.Commands != null)
            {
                iconList = cmdBar.Commands.Select(c => c.Icon).ToArray();
            }

            //NOTE: if commands are not used, main icon will fail if toolbar commands image list is not specified, so it is required to specify it explicitly

            if (App.SupportsHighResIcons(SldWorksExtension.HighResIconsScope_e.CommandManager))
            {
                var iconsList = iconsConv.ConvertIcon(mainIcon, true);
                cmdGroup.MainIconList = iconsList;

                if (iconList != null && iconList.Any())
                {
                    cmdGroup.IconList = iconsConv.ConvertIconsGroup(iconList, true);
                }
                else
                {
                    cmdGroup.IconList = iconsList;
                }
            }
            else
            {
                var mainIconPath = iconsConv.ConvertIcon(mainIcon, false);

                var smallIcon = mainIconPath[0];
                var largeIcon = mainIconPath[1];

                cmdGroup.SmallMainIcon = smallIcon;
                cmdGroup.LargeMainIcon = largeIcon;

                if (iconList != null && iconList.Any())
                {
                    var iconListPath = iconsConv.ConvertIconsGroup(iconList, true);
                    var smallIconList = iconListPath[0];
                    var largeIconList = iconListPath[1];

                    cmdGroup.SmallIconList = smallIconList;
                    cmdGroup.LargeIconList = largeIconList;
                }
                else
                {
                    cmdGroup.SmallIconList = smallIcon;
                    cmdGroup.LargeIconList = largeIcon;
                }
            }
        }

        private Dictionary<ICommandSpec, int> CreateCommandItems(CommandGroup cmdGroup, int groupId, ICommandSpec[] cmds)
        {
            var createdCmds = new Dictionary<ICommandSpec, int>();

            var callbackMethodName = nameof(OnCommandClick);
            var enableMethodName = nameof(OnCommandEnable);

            for (int i = 0; i < cmds.Length; i++)
            {
                var cmd = cmds[i];
                
                swCommandItemType_e menuToolbarOpts = 0;
                
                if (cmd.HasMenu)
                {
                    menuToolbarOpts |= swCommandItemType_e.swMenuItem;
                }

                if (cmd.HasToolbar)
                {
                    menuToolbarOpts |= swCommandItemType_e.swToolbarItem;
                }

                if (menuToolbarOpts == 0)
                {
                    throw new InvalidMenuToolbarOptionsException(cmd);
                }
                
                var cmdName = $"{groupId}.{cmd.UserId}";

                m_Commands.Add(cmdName, cmd);
                
                var callbackFunc = $"{callbackMethodName}({cmdName})";
                var enableFunc = $"{enableMethodName}({cmdName})";
                
                if (cmd.HasSpacer)
                {
                    cmdGroup.AddSpacer2(-1, (int)menuToolbarOpts);
                }

                var cmdIndex = cmdGroup.AddCommandItem2(cmd.Title, -1, cmd.Tooltip,
                    cmd.Title, i, callbackFunc, enableFunc, cmd.UserId,
                    (int)menuToolbarOpts);
                
                createdCmds.Add(cmd, cmdIndex);

                Logger.Log($"Created command {cmdIndex} for {cmd}");
            }

            cmdGroup.HasToolbar = true;
            cmdGroup.HasMenu = true;
            cmdGroup.Activate();

            return createdCmds.ToDictionary(p => p.Key, p => cmdGroup.CommandID[p.Value]);
        }

        private void CreateCommandTabBox(CommandGroup cmdGroup, Dictionary<ICommandSpec, int> commands)
        {
            Logger.Log($"Creating command tab box");

            var tabCommands = new List<TabCommandInfo>();
            
            foreach (var cmdData in commands)
            {
                var cmd = cmdData.Key;
                var cmdId = cmdData.Value;
                
                if (cmd.HasTabBox)
                {
                    var docTypes = new List<swDocumentTypes_e>();

                    if (cmd.SupportedWorkspace.HasFlag(swWorkspaceTypes_e.Part))
                    {
                        docTypes.Add(swDocumentTypes_e.swDocPART);
                    }

                    if (cmd.SupportedWorkspace.HasFlag(swWorkspaceTypes_e.Assembly))
                    {
                        docTypes.Add(swDocumentTypes_e.swDocASSEMBLY);
                    }

                    if (cmd.SupportedWorkspace.HasFlag(swWorkspaceTypes_e.Drawing))
                    {
                        docTypes.Add(swDocumentTypes_e.swDocDRAWING);
                    }

                    tabCommands.AddRange(docTypes.Select(
                        t => new TabCommandInfo(
                            t, cmdId, cmd.TabBoxStyle)));
                }
            }

            foreach (var cmdGrp in tabCommands.GroupBy(c => c.DocType))
            {
                var docType = cmdGrp.Key;

                var cmdTab = CmdMgr.GetCommandTab((int)docType, cmdGroup.Name);

                if (cmdTab == null)
                {
                    cmdTab = CmdMgr.AddCommandTab((int)docType, cmdGroup.Name);
                }
                
                if (cmdTab != null)
                {
                    var cmdIds = cmdGrp.Select(c => c.CmdId).ToArray();
                    var txtTypes = cmdGrp.Select(c => (int)c.TextType).ToArray();

                    var cmdBox = TryFindCommandTabBox(cmdTab, cmdIds);

                    if (cmdBox == null)
                    {
                        cmdBox = cmdTab.AddCommandTabBox();
                    }
                    else
                    {
                        if (!IsCommandTabBoxChanged(cmdBox, cmdIds, txtTypes))
                        {
                            continue;
                        }
                        else
                        {
                            ClearCommandTabBox(cmdBox);
                        }
                    }
                    
                    if (!cmdBox.AddCommands(cmdIds, txtTypes))
                    {
                        throw new InvalidOperationException("Failed to add commands to commands tab box");
                    }
                }
                else
                {
                    throw new NullReferenceException("Failed to create command tab box");
                }
            }
        }

        private CommandTabBox TryFindCommandTabBox(ICommandTab cmdTab, int[] cmdIds)
        {
            var cmdBoxesArr = cmdTab.CommandTabBoxes() as object[];

            if (cmdBoxesArr != null)
            {
                var cmdBoxes = cmdBoxesArr.Cast<CommandTabBox>().ToArray();

                var cmdBoxGroup = cmdBoxes.GroupBy(b =>
                {
                    object existingCmds;
                    object existingTextStyles;
                    b.GetCommands(out existingCmds, out existingTextStyles);

                    if (existingCmds is int[])
                    {
                        return ((int[])existingCmds).Intersect(cmdIds).Count();
                    }
                    else
                    {
                        return 0;
                    }
                }).OrderByDescending(g => g.Key).FirstOrDefault();

                if (cmdBoxGroup != null)
                {
                    if (cmdBoxGroup.Key > 0)
                    {
                        return cmdBoxGroup.FirstOrDefault();
                    }
                }

                return null;
            }

            return null;
        }

        private bool IsCommandTabBoxChanged(ICommandTabBox cmdBox, int[] cmdIds, int[] txtTypes)
        {
            object existingCmds;
            object existingTextStyles;
            cmdBox.GetCommands(out existingCmds, out existingTextStyles);

            if (existingCmds != null && existingTextStyles != null)
            {
                return !(existingCmds as int[]).SequenceEqual(cmdIds)
                    || !(existingTextStyles as int[]).SequenceEqual(txtTypes);
            }

            return true;
        }

        private void ClearCommandTabBox(ICommandTabBox cmdBox)
        {
            object existingCmds;
            object existingTextStyles;
            cmdBox.GetCommands(out existingCmds, out existingTextStyles);

            if (existingCmds != null)
            {
                cmdBox.RemoveCommands(existingCmds as int[]);
            }
        }
        
        private bool CompareIDs(IEnumerable<int> storedIDs, IEnumerable<int> addinIDs)
        {
            var storedList = storedIDs.ToList();
            var addinList = addinIDs.ToList();

            addinList.Sort();
            storedList.Sort();

            return addinList.SequenceEqual(storedIDs);
        }
    }
}
