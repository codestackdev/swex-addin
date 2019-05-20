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
using CodeStack.SwEx.AddIn.Properties;
using CodeStack.SwEx.Common.Attributes;
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
using System.Reflection;
using System.Runtime.InteropServices;
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

        private readonly List<ICommandGroupSpec> m_CommandGroups;
        private readonly Dictionary<string, ICommandSpec> m_Commands;
        private readonly List<ITaskPaneHandler> m_TaskPanes;

        private readonly ILogger m_Logger;

        public SwAddInEx()
        {
            m_Logger = LoggerFactory.Create(this);
            m_Commands = new Dictionary<string, ICommandSpec>();
            m_CommandGroups = new List<ICommandGroupSpec>();
            m_TaskPanes = new List<ITaskPaneHandler>();
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
                foreach (var grp in m_CommandGroups)
                {
                    Logger.Log($"Removing group: {grp.Id}");
                    CmdMgr.RemoveCommandGroup(grp.Id);
                }

                for (int i = m_TaskPanes.Count - 1; i >= 0; i--)
                {
                    m_TaskPanes[i].Delete();
                }

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
                new EnumCommandGroupSpec<TCmdEnum>(App, callback, enable, GetNextAvailableGroupId()));
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
                new EnumCommandGroupSpec<TCmdEnum>(App, callback, enable, GetNextAvailableGroupId()),
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
            return new DocumentsHandler<TDocHandler>(App, m_Logger);
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
                return m_CommandGroups.Max(g => g.Id) + 1;
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

            if (m_CommandGroups.FirstOrDefault(g=>g.Id == cmdBar.Id) == null)
            {
                m_CommandGroups.Add(cmdBar);
            }
            else
            {
                throw new GroupIdAlreadyExistsException(cmdBar);
            }

            bool isCmdsChanged;

            var cmdGroup = CreateCommandGroup(cmdBar.Id, cmdBar.Title, cmdBar.Tooltip,
                cmdBar.Commands.Select(c => c.UserId).ToArray(), isContextMenu,
                contextMenuSelectType, out isCmdsChanged);
            
            using (var iconsConv = new IconsConverter())
            {
                CreateIcons(cmdGroup, cmdBar, iconsConv);

                var createdCmds = CreateCommandItems(cmdGroup, cmdBar.Id, cmdBar.Commands);
                
                CreateCommandTabBox(cmdGroup, createdCmds, isCmdsChanged);
            }

            return cmdGroup;
        }
        
        private CommandGroup CreateCommandGroup(int groupId, string title, string toolTip,
            int[] knownCmdIDs, bool isContextMenu, swSelectType_e contextMenuSelectType, out bool isChanged)
        {
            int cmdGroupErr = 0;

            isChanged = false;

            object registryIDs;

            isChanged = true;

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

        private void CreateCommandTabBox(CommandGroup cmdGroup, Dictionary<ICommandSpec, int> commands, bool removePrevious)
        {
            Logger.Log($"Creating command tab box");

            var tabCommands = new List<Tuple<swDocumentTypes_e, int, swCommandTabButtonTextDisplay_e>>();

            var ignoredCmds = new List<int>();

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
                        t => new Tuple<swDocumentTypes_e, int, swCommandTabButtonTextDisplay_e>(
                            t, cmdId, cmd.TabBoxStyle)));
                }
                else
                {
                    ignoredCmds.Add(cmdId);
                }
            }

            foreach (var cmdGrp in tabCommands.GroupBy(c => c.Item1))
            {
                var docType = cmdGrp.Key;

                var cmdTab = CmdMgr.GetCommandTab((int)docType, cmdGroup.Name);

                //NOTE: checking if command group is changed or any of the commands has changed the
                //option to be added to command tab box - as this can be changed without changing the command group
                if (cmdTab != null &&
                    (removePrevious
                    || ContainsCommands(cmdTab, cmdGrp.Select(c => c.Item2)).Any(c => !c)
                    || ContainsCommands(cmdTab, ignoredCmds).Any(c => c)
                    ))
                {
                    if (!CmdMgr.RemoveCommandTab(cmdTab))
                    {
                        Debug.Assert(false, "Failed to remove command tab");
                    }

                    cmdTab = null;
                }

                if (cmdTab == null)
                {
                    cmdTab = CmdMgr.AddCommandTab((int)docType, cmdGroup.Name);

                    var cmdBox = cmdTab.AddCommandTabBox();

                    var cmdIds = cmdGrp.Select(c => c.Item2).ToArray();
                    var txtTypes = cmdGrp.Select(c => (int)c.Item3).ToArray();
                    if (!cmdBox.AddCommands(cmdIds, txtTypes))
                    {
                        Debug.Assert(false, "Failed to add commands to the command box");
                    }
                }
            }
        }

        private IEnumerable<bool> ContainsCommands(ICommandTab cmdTab, IEnumerable<int> cmdIds)
        {
            var cmdBoxesArr = cmdTab.CommandTabBoxes() as object[];

            if (cmdBoxesArr != null)
            {
                var cmdBoxes = cmdBoxesArr.Cast<ICommandTabBox>().ToArray();

                return cmdIds.Select(cmdId => cmdBoxes.Any(b =>
                {
                    object existingCmds;
                    object existingTextStyles;
                    b.GetCommands(out existingCmds, out existingTextStyles);

                    if (existingCmds is int[])
                    {
                        return ((int[])existingCmds).Contains(cmdId);
                    }

                    return false;
                }));
            }
            else
            {
                return Enumerable.Empty<bool>();
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
