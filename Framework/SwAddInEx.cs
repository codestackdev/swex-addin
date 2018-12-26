//**********************
//SwEx.AddIn - development tools for SOLIDWORKS add-ins
//Copyright(C) 2018 www.codestack.net
//License: https://github.com/codestack-net-dev/sw-dev-tools-addin/blob/master/LICENSE
//Product URL: https://www.codestack.net/labs/solidworks/swex/add-in/
//**********************

using CodeStack.SwEx.AddIn.Attributes;
using CodeStack.SwEx.AddIn.Base;
using CodeStack.SwEx.AddIn.Enums;
using CodeStack.SwEx.AddIn.Exceptions;
using CodeStack.SwEx.AddIn.Helpers;
using CodeStack.SwEx.AddIn.Icons;
using CodeStack.SwEx.AddIn.Properties;
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
            return m_RegHelper ?? (m_RegHelper = new RegistrationHelper(new ModuleLogger(moduleType)));
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
        /// Deprecated. Use App property instead
        /// </summary>
        [Obsolete("Deprecated. Use App property instead")]
        protected ISldWorks m_App = null;

        /// <summary>
        /// Deprecated. Use CmdMgr property instead
        /// </summary>
        [Obsolete("Deprecated. Use CmdMgr property instead")]
        protected ICommandManager m_CmdMgr = null;

        /// <summary>
        /// Deprecated. Use AddInCookie property instead
        /// </summary>
        [Obsolete("Deprecated. Use AddInCookie property instead")]
        protected int m_AddInCookie = 0;

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

        private Dictionary<string, swWorkspaceTypes_e> m_CachedCmdsEnable;
        private Dictionary<string, Tuple<Delegate, Enum>> m_CallbacksParams;
        private Dictionary<string, Tuple<Delegate, Enum>> m_EnableParams;

        private List<int> m_CommandGroupIds;

        private readonly ModuleLogger m_Logger;

        public SwAddInEx()
        {
            m_Logger = new ModuleLogger(this);
        }

        /// <summary>SOLIDWORKS add-in entry function</summary>
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool ConnectToSW(object ThisSW, int cookie)
        {
            m_Logger.Log("Loading add-in");

            try
            {
                App = ThisSW as ISldWorks;
                AddInCookie = cookie;

                App.SetAddinCallbackInfo(0, this, AddInCookie);

                CmdMgr = App.GetCommandManager(AddInCookie);

                //TODO: legacy - to be removed
#pragma warning disable CS0618
                m_App = App;
                m_CmdMgr = CmdMgr;
                m_AddInCookie = AddInCookie;
#pragma warning restore CS0618
                //----

                m_CachedCmdsEnable = new Dictionary<string, swWorkspaceTypes_e>();
                m_CallbacksParams = new Dictionary<string, Tuple<Delegate, Enum>>();
                m_EnableParams = new Dictionary<string, Tuple<Delegate, Enum>>();
                m_CommandGroupIds = new List<int>();

                return OnConnect();
            }
            catch(Exception ex)
            {
                m_Logger.LogException(ex);
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
            m_Logger.Log($"Command clicked: {cmdId}");

            Tuple<Delegate, Enum> callbackData;

            if (m_CallbacksParams.TryGetValue(cmdId, out callbackData))
            {
                callbackData.Item1.DynamicInvoke(callbackData.Item2);
            }
            else
            {
                System.Diagnostics.Debug.Assert(false, "All callbacks must be registered");
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
            var supportedSpaces = m_CachedCmdsEnable[cmdId];

            var curSpace = swWorkspaceTypes_e.NoDocuments;

            if (App.IActiveDoc2 == null)
            {
                curSpace = swWorkspaceTypes_e.NoDocuments;
            }
            else
            {
                switch ((swDocumentTypes_e)App.IActiveDoc2.GetType())
                {
                    case swDocumentTypes_e.swDocPART:
                        curSpace = swWorkspaceTypes_e.Part;
                        break;

                    case swDocumentTypes_e.swDocASSEMBLY:
                        curSpace = swWorkspaceTypes_e.Assembly;
                        break;

                    case swDocumentTypes_e.swDocDRAWING:
                        curSpace = swWorkspaceTypes_e.Drawing;
                        break;
                }
            }

            CommandItemEnableState_e state;

            if (supportedSpaces.HasFlag(curSpace))
            {
                state = CommandItemEnableState_e.DeselectEnable;
            }
            else
            {
                state = CommandItemEnableState_e.DeselectDisable;
            }

            Tuple<Delegate, Enum> enable;
            if (m_EnableParams.TryGetValue(cmdId, out enable))
            {
                var args = new object[] { enable.Item2, state };
                enable.Item1.DynamicInvoke(args);
                state = (CommandItemEnableState_e)args[1];
            }

            return (int)state;
        }

        /// <summary>
        /// SOLIDWORKS unload add-in callback
        /// </summary>
        /// <returns></returns>
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool DisconnectFromSW()
        {
            m_Logger.Log("Unloading add-in");

            try
            {
                foreach (var grpId in m_CommandGroupIds)
                {
                    m_Logger.Log($"Removing group: {grpId}");
                    CmdMgr.RemoveCommandGroup(grpId);
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
            catch(Exception ex)
            {
                m_Logger.LogException(ex);
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
            return AddCommandGroupOrContextMenu(callback, enable, false, 0);
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
            return AddCommandGroupOrContextMenu(callback, enable, true, contextMenuSelectType);
        }

        private CommandGroup AddCommandGroupOrContextMenu<TCmdEnum>(Action<TCmdEnum> callback,
            EnableMethodDelegate<TCmdEnum> enable, bool isContextMenu, swSelectType_e contextMenuSelectType)
            where TCmdEnum : IComparable, IFormattable, IConvertible
        {
            m_Logger.Log("Creating command group");

            if (!(typeof(TCmdEnum).IsEnum))
            {
                throw new ArgumentException($"{typeof(TCmdEnum)} must be an Enum");
            }

            if (callback == null)
            {
                throw new CallbackNotSpecifiedException();
            }

            var cmdGroupType = typeof(TCmdEnum);

            int groupId;
            string title;
            string toolTip;

            GetCommandGroupAttribution(cmdGroupType, out groupId, out title, out toolTip);

            var cmds = Enum.GetValues(cmdGroupType).Cast<Enum>().ToArray();

            bool isCmdsChanged;

            m_Logger.Log($"Creating group: {groupId}");

            var cmdGroup = CreateCommandGroup(groupId, title, toolTip, cmds, isContextMenu,
                contextMenuSelectType, out isCmdsChanged);
            
            using (var iconsConv = new IconsConverter())
            {
                CreateIcons(cmdGroup, cmdGroupType, cmds, iconsConv);

                var createdCmds = CreateCommandItems(cmdGroup, groupId, cmds, callback, enable);
                
                CreateCommandTabBox(cmdGroup, createdCmds, isCmdsChanged);
            }

            return cmdGroup;
        }

        private void GetCommandGroupAttribution(Type cmdGroupType, out int groupId,
            out string title, out string toolTip)
        {
            groupId = -1;

            CommandGroupInfoAttribute grpInfoAtt;

            if (cmdGroupType.TryGetAttribute(out grpInfoAtt))
            {
                groupId = grpInfoAtt.UserId;
            }
            else
            {
                if (m_CommandGroupIds.Any())
                {
                    groupId = m_CommandGroupIds.Max() + 1;
                }
                else
                {
                    groupId = 0;
                }
            }

            if (!m_CommandGroupIds.Contains(groupId))
            {
                m_CommandGroupIds.Add(groupId);
            }
            else
            {
                throw new GroupIdAlreadyExistsException(groupId);
            }
            
            DisplayNameAttribute dispNameAtt;

            if (cmdGroupType.TryGetAttribute(out dispNameAtt))
            {
                title = dispNameAtt.DisplayName;
            }
            else
            {
                title = cmdGroupType.ToString();
            }

            DescriptionAttribute descAtt;

            if (cmdGroupType.TryGetAttribute(out descAtt))
            {
                toolTip = descAtt.Description;
            }
            else
            {
                toolTip = cmdGroupType.ToString();
            }
        }

        private CommandGroup CreateCommandGroup(int groupId, string title, string toolTip,
            Enum[] cmds, bool isContextMenu, swSelectType_e contextMenuSelectType, out bool isChanged)
        {
            int cmdGroupErr = 0;

            isChanged = false;

            object registryIDs;

            isChanged = true;

            if (CmdMgr.GetGroupDataFromRegistry(groupId, out registryIDs))
            {
                var knownIDs = cmds.Select(c => Convert.ToInt32(c));

                isChanged = !CompareIDs(registryIDs as int[], knownIDs);
            }

            m_Logger.Log($"Command ids changed: {isChanged}");

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

                m_Logger.Log($"Command group creation result: {(swCreateCommandGroupErrors)cmdGroupErr}");

                Debug.Assert(cmdGroupErr == (int)swCreateCommandGroupErrors.swCreateCommandGroup_Success);
            }
            
            return cmdGroup;
        }

        private void CreateIcons(CommandGroup cmdGroup, Type cmdGroupType, Enum[] cmds, IconsConverter iconsConv)
        {
            IIcon mainIcon = null;

            if (!cmdGroupType.TryGetAttribute<CommandIconAttribute>(a => mainIcon = a.Icon))
            {
                var icon = cmdGroupType.TryGetAttribute<Common.Attributes.IconAttribute>()?.Icon;

                if (icon == null)
                {
                    icon = Resources.swex_addin_default;
                }

                mainIcon = new MasterIcon() { Icon = icon };
            }

            var iconList = cmds.Select(c =>
            {
                IIcon cmdIcon = null;
                if (!c.TryGetAttribute<CommandIconAttribute>(a => cmdIcon = a.Icon))
                {
                    var icon = c.TryGetAttribute<Common.Attributes.IconAttribute>()?.Icon;

                    if (icon == null)
                    {
                        icon = Resources.swex_addin_default;
                    }

                    cmdIcon = new MasterIcon() { Icon = icon };
                }
                return cmdIcon;
            }).ToArray();

            if (App.SupportsHighResIcons())
            {
                var iconsList = iconsConv.ConvertIcon(mainIcon, true);
                cmdGroup.MainIconList = iconsList;
                cmdGroup.IconList = iconsConv.ConvertIconsGroup(iconList, true);
            }
            else
            {
                var mainIconPath = iconsConv.ConvertIcon(mainIcon, false);

                var smallIcon = mainIconPath[0];
                var largeIcon = mainIconPath[1];

                cmdGroup.SmallMainIcon = smallIcon;
                cmdGroup.LargeMainIcon = largeIcon;

                var iconListPath = iconsConv.ConvertIconsGroup(iconList, true);
                var smallIconList = iconListPath[0];
                var largeIconList = iconListPath[1];

                cmdGroup.SmallIconList = smallIconList;
                cmdGroup.LargeIconList = largeIconList;
            }
        }

        private Dictionary<Enum, int> CreateCommandItems(CommandGroup cmdGroup, int groupId, Enum[] cmds, 
            Delegate callbackMethod, Delegate enableMethod)
        {
            var createdCmds = new Dictionary<Enum, int>();

            var callbackMethodName = nameof(OnCommandClick);
            var enableMethodName = nameof(OnCommandEnable);

            for (int i = 0; i < cmds.Length; i++)
            {
                var cmd = cmds[i];

                var cmdTitle = "";
                var cmdToolTip = "";
                swCommandItemType_e menuToolbarOpts = 0;
                swWorkspaceTypes_e suppWorkSpaces = 0;

                if (!cmd.TryGetAttribute<DisplayNameAttribute>(
                    att => cmdTitle = att.DisplayName))
                {
                    cmdTitle = cmd.ToString();
                }
                
                if (!cmd.TryGetAttribute<DescriptionAttribute>(
                    att=> cmdToolTip = att.Description))
                {
                    cmdToolTip = cmd.ToString();
                }

                bool hasMenu;
                bool hasToolbar;

                GetCommandInfo(cmd, out hasMenu, out hasToolbar, out suppWorkSpaces);

                if (hasMenu)
                {
                    menuToolbarOpts |= swCommandItemType_e.swMenuItem;
                }

                if (hasToolbar)
                {
                    menuToolbarOpts |= swCommandItemType_e.swToolbarItem;
                }

                if (menuToolbarOpts == 0)
                {
                    throw new InvalidMenuToolbarOptionsException(cmd);
                }
                                
                var cmdUserId = Convert.ToInt32(cmd);

                var cmdName = $"{groupId}.{cmdUserId}";

                m_CachedCmdsEnable.Add(cmdName, suppWorkSpaces);
                m_CallbacksParams.Add(cmdName, new Tuple<Delegate, Enum>(callbackMethod, cmd));

                if (enableMethod != null)
                {
                    m_EnableParams.Add(cmdName, new Tuple<Delegate, Enum>(enableMethod, cmd));
                }

                var callbackFunc = $"{callbackMethodName}({cmdName})";
                var enableFunc = $"{enableMethodName}({cmdName})";

                var cmdIndex = cmdGroup.AddCommandItem2(cmdTitle, -1, cmdToolTip,
                    cmdTitle, i, callbackFunc, enableFunc, cmdUserId,
                    (int)menuToolbarOpts);

                createdCmds.Add(cmd, cmdIndex);

                m_Logger.Log($"Created command {cmdIndex} for {cmd}");
            }

            cmdGroup.HasToolbar = true;
            cmdGroup.HasMenu = true;
            cmdGroup.Activate();

            return createdCmds.ToDictionary(p => p.Key, p => cmdGroup.CommandID[p.Value]);
        }

        private void CreateCommandTabBox(CommandGroup cmdGroup, Dictionary<Enum, int> commands, bool removePrevious)
        {
            m_Logger.Log($"Creating command tab box");

            var tabCommands = new List<Tuple<swDocumentTypes_e, int, swCommandTabButtonTextDisplay_e>>();

            var ignoredCmds = new List<int>();

            foreach (var cmd in commands)
            {
                var cmdId = cmd.Value;
                swWorkspaceTypes_e workSpace;
                swCommandTabButtonTextDisplay_e style;
                bool hasTabBox;
                GetCommandInfo(cmd.Key, out workSpace, out hasTabBox, out style);

                if (hasTabBox)
                {
                    var docTypes = new List<swDocumentTypes_e>();

                    if (workSpace.HasFlag(swWorkspaceTypes_e.Part))
                    {
                        docTypes.Add(swDocumentTypes_e.swDocPART);
                    }

                    if (workSpace.HasFlag(swWorkspaceTypes_e.Assembly))
                    {
                        docTypes.Add(swDocumentTypes_e.swDocASSEMBLY);
                    }

                    if (workSpace.HasFlag(swWorkspaceTypes_e.Drawing))
                    {
                        docTypes.Add(swDocumentTypes_e.swDocDRAWING);
                    }

                    tabCommands.AddRange(docTypes.Select(
                        t => new Tuple<swDocumentTypes_e, int, swCommandTabButtonTextDisplay_e>(
                            t, cmdId, style)));
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
                        System.Diagnostics.Debug.Assert(false, "Failed to remove command tab");
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
                        System.Diagnostics.Debug.Assert(false, "Failed to add commands to the command box");
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

        private void GetCommandInfo(Enum cmd, out bool hasMenu, out bool hasToolbar,
            out swWorkspaceTypes_e suppWorkSpaces)
        {
            bool hasTabBox;
            swCommandTabButtonTextDisplay_e tabBoxStyle;

            GetCommandInfo(cmd, out hasMenu, out hasToolbar, out suppWorkSpaces, out hasTabBox, out tabBoxStyle);
        }

        private void GetCommandInfo(Enum cmd,
            out swWorkspaceTypes_e suppWorkSpaces, out bool hasTabBox, out swCommandTabButtonTextDisplay_e tabBoxStyle)
        {
            bool hasMenu;
            bool hasToolbar;

            GetCommandInfo(cmd, out hasMenu, out hasToolbar, out suppWorkSpaces, out hasTabBox, out tabBoxStyle);
        }

        private void GetCommandInfo(Enum cmd, out bool hasMenu, out bool hasToolbar,
            out swWorkspaceTypes_e suppWorkSpaces, out bool hasTabBox, out swCommandTabButtonTextDisplay_e tabBoxStyle)
        {
            var localHasMenu = true;
            var localHasToolbar = true;
            var localSuppWorkSpaces = swWorkspaceTypes_e.All;
            var localHasTabBox = false;
            var localTabBoxStyle = swCommandTabButtonTextDisplay_e.swCommandTabButton_TextBelow;

            cmd.TryGetAttribute<CommandItemInfoAttribute>(
                att =>
                {
                    localHasMenu = att.HasMenu;
                    localHasToolbar = att.HasToolbar;
                    localSuppWorkSpaces = att.SupportedWorkspaces;
                    localHasTabBox = att.ShowInCommandTabBox;
                    localTabBoxStyle = att.CommandTabBoxDisplayStyle;
                });

            hasMenu = localHasMenu;
            hasToolbar = localHasToolbar;
            suppWorkSpaces = localSuppWorkSpaces;
            hasTabBox = localHasTabBox;
            tabBoxStyle = localTabBoxStyle;
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
