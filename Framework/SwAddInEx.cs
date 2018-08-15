using CodeStack.Dev.Sw.AddIn.Attributes;
using CodeStack.Dev.Sw.AddIn.Enums;
using CodeStack.Dev.Sw.AddIn.Icons;
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

namespace SolidWorks.Interop.sldworks
{
    public interface IAddCommandGroup<TCmdEnum>
            where TCmdEnum : IComparable, IFormattable, IConvertible
    {
        void Callback(TCmdEnum cmd);
    }

    public interface IAddCommandGroupWithEnable<TCmdEnum> : IAddCommandGroup<TCmdEnum>
            where TCmdEnum : IComparable, IFormattable, IConvertible
    {
        void Enable(TCmdEnum cmd, ref CommandItemEnableState_e state);
    }

    [ComVisible(true)]
    public abstract class SwAddInEx : ISwAddin
    {
        protected ISldWorks m_App = null;
        protected ICommandManager m_CmdMgr = null;
        protected int m_AddInCookie = 0;

        private Dictionary<string, swWorkspaceTypes_e> m_CachedCmdsEnable;
        private Dictionary<string, Tuple<MethodInfo, Enum>> m_CallbacksParams;
        private Dictionary<string, Tuple<MethodInfo, Enum>> m_EnableParams;

        public bool ConnectToSW(object ThisSW, int cookie)
        {
            m_App = ThisSW as ISldWorks;
            m_AddInCookie = cookie;

            m_App.SetAddinCallbackInfo(0, this, m_AddInCookie);

            m_CmdMgr = m_App.GetCommandManager(cookie);

            m_CachedCmdsEnable = new Dictionary<string, swWorkspaceTypes_e>();
            m_CallbacksParams = new Dictionary<string, Tuple<MethodInfo, Enum>>();
            m_EnableParams = new Dictionary<string, Tuple<MethodInfo, Enum>>();

            var cmdGrpDefTypes = this.GetType().GetInterfaces().Select(
                t => t.TryFindGenericType(typeof(IAddCommandGroup<>))).Where(t => t != null);

            var processedGroups = new List<Type>();

            foreach (var cmdGrpDefType in cmdGrpDefTypes)
            {
                var cmdGrpType = cmdGrpDefType.GetArgumentsOfGenericType(
                    typeof(IAddCommandGroup<>)).First();

                if (!processedGroups.Contains(cmdGrpType))
                {
                    processedGroups.Add(cmdGrpType);

                    var enableType = typeof(IAddCommandGroupWithEnable<>).MakeGenericType(cmdGrpType);

                    MethodInfo enableMethod = null;
                    if (enableType.IsAssignableFrom(this.GetType()))
                    {
                        enableMethod = enableType.GetMethod(nameof(IAddCommandGroupWithEnable<Enum>.Enable));
                    }

                    CreateCommandGroup(cmdGrpType,
                        cmdGrpDefType.GetMethod(nameof(IAddCommandGroup<Enum>.Callback)), enableMethod);
                }
            }

            OnConnect();

            return true;
        }

        protected virtual bool OnConnect()
        {
            return true;
        }

        protected virtual bool OnDisconnect()
        {
            return true;
        }

        public bool DisconnectFromSW()
        {
            OnDisconnect();

            //TODO: record group ids
            m_CmdMgr.RemoveCommandGroup(0);

            Marshal.ReleaseComObject(m_CmdMgr);
            m_CmdMgr = null;
            Marshal.ReleaseComObject(m_App);
            m_App = null;

            GC.Collect();
            GC.WaitForPendingFinalizers();

            GC.Collect();
            GC.WaitForPendingFinalizers();

            return true;
        }

        public void OnCommandClick(string cmdId)
        {
            Tuple<MethodInfo, Enum> callbackData;

            if (m_CallbacksParams.TryGetValue(cmdId, out callbackData))
            {
                callbackData.Item1.Invoke(this, new object[] { callbackData.Item2 });
            }
            else
            {
                Debug.Assert(false, "All callbacks must be registered");
            }
        }

        public int OnCommandEnable(string cmdId)
        {
            var supportedSpaces = m_CachedCmdsEnable[cmdId];

            swWorkspaceTypes_e curSpace = swWorkspaceTypes_e.NoDocuments;

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

            Tuple<MethodInfo, Enum> enable;
            if (m_EnableParams.TryGetValue(cmdId, out enable))
            {
                var args = new object[] { enable.Item2, state };
                enable.Item1.Invoke(this, args);
                state = (CommandItemEnableState_e)args[1];
            }

            return (int)state;
        }

        private void CreateCommandGroup(Type cmdGroupType, MethodInfo callbackMethod, MethodInfo enableMethod = null)
        {
            var groupId = 0; //TODO: create attribute and read
            
            if (!cmdGroupType.IsEnum)
            {
                throw new ArgumentException($"{nameof(cmdGroupType)} must be an Enum");
            }

            var title = cmdGroupType.GetAttribute<DisplayNameAttribute>().DisplayName;

            var toolTip = cmdGroupType.GetAttribute<DescriptionAttribute>().Description;

            int cmdGroupErr = 0;
            bool ignorePrevious = false;

            object registryIDs;

            bool getDataResult = m_CmdMgr.GetGroupDataFromRegistry(groupId, out registryIDs);

            var cmds = Enum.GetValues(cmdGroupType).Cast<Enum>().ToArray();

            var knownIDs = new int[cmds.Length];

            for (int i = 0; i < cmds.Length; i++)
            {
                knownIDs[i] = (int)cmds.GetValue(i);
            }

            if (getDataResult)
            {
                ignorePrevious = !CompareIDs(registryIDs as int[], knownIDs);
            }

            var cmdGroup = m_CmdMgr.CreateCommandGroup2(groupId, title, toolTip,
                toolTip, -1, ignorePrevious, ref cmdGroupErr);

            using (var iconsConv = new IconsConverter())
            {
                var mainIcon = cmdGroupType.GetAttribute<IconAttribute>().Icon;
                var iconList = cmds.Select(c => c.GetAttribute<IconAttribute>().Icon).ToArray();

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
                
                var callbackMethodName = nameof(OnCommandClick);
                var enableMethodName = nameof(OnCommandEnable);
                
                for (int i = 0; i < cmds.Length; i++)
                {
                    var cmd = cmds[i];

                    var cmdTitle = cmd.GetAttribute<DisplayNameAttribute>().DisplayName;
                    var cmdToolTip = cmd.GetAttribute<DescriptionAttribute>().Description;
                    var cmdInfoAtt = cmd.GetAttribute<CommandItemInfoAttribute>();
                    var tbOpts = cmdInfoAtt.MenuToolbarVisibility;

                    var cmdId = Convert.ToInt32(cmd);

                    var cmdName = $"{groupId}.{cmdId}";

                    m_CachedCmdsEnable.Add(cmdName, cmdInfoAtt.SupportedWorkspaces);
                    m_CallbacksParams.Add(cmdName, new Tuple<MethodInfo, Enum>(callbackMethod, cmd));

                    if (enableMethod != null)
                    {
                        m_EnableParams.Add(cmdName, new Tuple<MethodInfo, Enum>(enableMethod, cmd));
                    }

                    var callbackFunc = $"{callbackMethodName}({cmdName})";
                    var enableFunc = $"{enableMethodName}({cmdName})";

                    cmdGroup.AddCommandItem2(cmdTitle, -1, cmdToolTip,
                        cmdTitle, i, callbackFunc, enableFunc, cmdId,
                        (int)tbOpts);
                }

                cmdGroup.HasToolbar = true;
                cmdGroup.HasMenu = true;
                cmdGroup.Activate();
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
