//**********************
//Development tools for SOLIDWORKS add-ins
//Copyright(C) 2018 www.codestack.net
//License: https://github.com/codestack-net-dev/sw-dev-tools-addin/blob/master/LICENSE
//Product URL: https://www.codestack.net/labs/solidworks/dev-tools-addin/
//**********************

using CodeStack.SwEx.AddIn.Attributes;
using CodeStack.SwEx.AddIn.Base;
using CodeStack.SwEx.AddIn.Enums;
using CodeStack.SwEx.AddIn.Exceptions;
using CodeStack.SwEx.AddIn.Helpers;
using CodeStack.SwEx.AddIn.Icons;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using SolidWorks.Interop.swpublished;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace CodeStack.SwEx.AddIn
{
    /// <inheritdoc/>
    [ComVisible(true)]
    public abstract class SwAddInEx : ISwAddin, ISwAddInEx
    {
        #region Registration

        [ComRegisterFunction]
        public static void RegisterFunction(Type t)
        {
            if (t.TryGetAttribute<AutoRegisterAttribute>())
            {
                RegistrationHelper.Register(t);
            }
        }

        [ComUnregisterFunction]
        public static void UnregisterFunction(Type t)
        {
            if (t.TryGetAttribute<AutoRegisterAttribute>())
            {
                RegistrationHelper.Unregister(t);
            }
        }

        #endregion

        protected ISldWorks m_App = null;
        protected ICommandManager m_CmdMgr = null;
        protected int m_AddInCookie = 0;

        private Dictionary<string, swWorkspaceTypes_e> m_CachedCmdsEnable;
        private Dictionary<string, Tuple<Delegate, Enum>> m_CallbacksParams;
        private Dictionary<string, Tuple<Delegate, Enum>> m_EnableParams;

        private List<int> m_CommandGroupIds;

        public bool ConnectToSW(object ThisSW, int cookie)
        {
            m_App = ThisSW as ISldWorks;
            m_AddInCookie = cookie;

            m_App.SetAddinCallbackInfo(0, this, m_AddInCookie);

            m_CmdMgr = m_App.GetCommandManager(cookie);

            m_CachedCmdsEnable = new Dictionary<string, swWorkspaceTypes_e>();
            m_CallbacksParams = new Dictionary<string, Tuple<Delegate, Enum>>();
            m_EnableParams = new Dictionary<string, Tuple<Delegate, Enum>>();
            m_CommandGroupIds = new List<int>();

            return OnConnect();
        }

        public void OnCommandClick(string cmdId)
        {
            Tuple<Delegate, Enum> callbackData;

            if (m_CallbacksParams.TryGetValue(cmdId, out callbackData))
            {
                callbackData.Item1.DynamicInvoke(callbackData.Item2);
            }
            else
            {
                Debug.Assert(false, "All callbacks must be registered");
            }
        }

        public int OnCommandEnable(string cmdId)
        {
            var supportedSpaces = m_CachedCmdsEnable[cmdId];

            var curSpace = swWorkspaceTypes_e.NoDocuments;

            if (m_App.IActiveDoc2 == null)
            {
                curSpace = swWorkspaceTypes_e.NoDocuments;
            }
            else
            {
                switch ((swDocumentTypes_e)m_App.IActiveDoc2.GetType())
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

        public bool DisconnectFromSW()
        {
            foreach (var grpId in m_CommandGroupIds)
            {
                m_CmdMgr.RemoveCommandGroup(0);
            }

            var res = OnDisconnect();

            if (Marshal.IsComObject(m_CmdMgr))
            {
                Marshal.ReleaseComObject(m_CmdMgr);
            }
            m_CmdMgr = null;

            if (Marshal.IsComObject(m_App))
            {
                Marshal.ReleaseComObject(m_App);
            }

            m_App = null;

            GC.Collect();
            GC.WaitForPendingFinalizers();

            GC.Collect();
            GC.WaitForPendingFinalizers();

            return res;
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

            var cmdGroup = CreateCommandGroup(groupId, title, toolTip, cmds, isContextMenu, contextMenuSelectType);

            using (var iconsConv = new IconsConverter())
            {
                CreateIcons(cmdGroup, cmdGroupType, cmds, iconsConv);

                CreateCommandItems(cmdGroup, groupId, cmds, callback, enable);

                cmdGroup.HasToolbar = true;
                cmdGroup.HasMenu = true;
                cmdGroup.Activate();
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
            Enum[] cmds, bool isContextMenu, swSelectType_e contextMenuSelectType)
        {
            int cmdGroupErr = 0;

            bool ignorePrevious = false;

            object registryIDs;

            bool getDataResult = m_CmdMgr.GetGroupDataFromRegistry(groupId, out registryIDs);

            var knownIDs = new int[cmds.Length];

            for (int i = 0; i < cmds.Length; i++)
            {
                knownIDs[i] = (int)cmds.GetValue(i);
            }

            if (getDataResult)
            {
                ignorePrevious = !CompareIDs(registryIDs as int[], knownIDs);
            }

            CommandGroup cmdGroup;

            if (isContextMenu)
            {
                cmdGroup = m_CmdMgr.AddContextMenu(groupId, title);
                cmdGroup.SelectType = (int)contextMenuSelectType;
            }
            else
            {
                cmdGroup = m_CmdMgr.CreateCommandGroup2(groupId, title, toolTip,
                    toolTip, -1, ignorePrevious, ref cmdGroupErr);
            }
            
            return cmdGroup;
        }

        private void CreateIcons(CommandGroup cmdGroup, Type cmdGroupType, Enum[] cmds, IconsConverter iconsConv)
        {
            IIcon mainIcon = null;

            if (!cmdGroupType.TryGetAttribute<IconAttribute>(a => mainIcon = a.Icon))
            {
                //TODO: add default icon
                mainIcon = new MasterIcon() { Icon = new System.Drawing.Bitmap(12, 12) };
            }

            var iconList = cmds.Select(c =>
            {
                IIcon cmdIcon = null;
                if (!c.TryGetAttribute<IconAttribute>(a => cmdIcon = a.Icon))
                {
                    //TODO: add default icon
                    cmdIcon = new MasterIcon() { Icon = new System.Drawing.Bitmap(12, 12) };
                }
                return cmdIcon;
            }).ToArray();

            if (m_App.SupportsHighResIcons())
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

        private void CreateCommandItems(CommandGroup cmdGroup, int groupId, Enum[] cmds, 
            Delegate callbackMethod, Delegate enableMethod)
        {
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
                
                if (!cmd.TryGetAttribute<CommandItemInfoAttribute>(
                    att => 
                    {
                        if (att.HasMenu)
                        {
                            menuToolbarOpts |= swCommandItemType_e.swMenuItem;
                        }

                        if (att.HasToolbar)
                        {
                            menuToolbarOpts |= swCommandItemType_e.swToolbarItem;
                        }

                        suppWorkSpaces = att.SupportedWorkspaces;
                    }))
                {
                    menuToolbarOpts = swCommandItemType_e.swMenuItem | swCommandItemType_e.swToolbarItem;
                    suppWorkSpaces = swWorkspaceTypes_e.All;
                }

                if (menuToolbarOpts == 0)
                {
                    throw new InvalidMenuToolbarOptionsException(cmd);
                }
                                
                var cmdId = Convert.ToInt32(cmd);

                var cmdName = $"{groupId}.{cmdId}";

                m_CachedCmdsEnable.Add(cmdName, suppWorkSpaces);
                m_CallbacksParams.Add(cmdName, new Tuple<Delegate, Enum>(callbackMethod, cmd));

                if (enableMethod != null)
                {
                    m_EnableParams.Add(cmdName, new Tuple<Delegate, Enum>(enableMethod, cmd));
                }

                var callbackFunc = $"{callbackMethodName}({cmdName})";
                var enableFunc = $"{enableMethodName}({cmdName})";

                cmdGroup.AddCommandItem2(cmdTitle, -1, cmdToolTip,
                    cmdTitle, i, callbackFunc, enableFunc, cmdId,
                    (int)menuToolbarOpts);
            }
        }
        
        private bool CompareIDs(int[] storedIDs, int[] addinIDs)
        {
            var storedList = storedIDs.ToList();
            var addinList = addinIDs.ToList();

            addinList.Sort();
            storedList.Sort();

            return addinList.SequenceEqual(storedIDs);
        }
    }
}
