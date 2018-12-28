using CodeStack.SwEx.AddIn.Attributes;
using CodeStack.SwEx.AddIn.Enums;
using CodeStack.SwEx.AddIn.Icons;
using CodeStack.SwEx.AddIn.Properties;
using CodeStack.SwEx.Common.Reflection;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace CodeStack.SwEx.AddIn.Core
{
    internal class EnumCommand<TCmdEnum> : Command
            where TCmdEnum : IComparable, IFormattable, IConvertible
    {
        private readonly ISldWorks m_App;
        private readonly TCmdEnum m_Cmd;
        private readonly Action<TCmdEnum> m_Callback;
        private readonly EnableMethodDelegate<TCmdEnum> m_Enable;

        internal EnumCommand(ISldWorks app, TCmdEnum cmd, Action<TCmdEnum> callback,
            EnableMethodDelegate<TCmdEnum> enable)
        {
            if (!(typeof(TCmdEnum).IsEnum))
            {
                throw new ArgumentException($"{typeof(TCmdEnum)} must be an Enum");
            }

            if (callback == null)
            {
                throw new ArgumentNullException(nameof(callback));
            }

            m_App = app;
            m_Cmd = cmd;
            m_Callback = callback;
            m_Enable = enable;

            ExtractCommandInfo(cmd);
        }

        public override void OnClick()
        {
            m_Callback.Invoke(m_Cmd);
        }

        public override CommandItemEnableState_e OnEnable()
        {
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

            if (SupportedWorkspace.HasFlag(curSpace))
            {
                state = CommandItemEnableState_e.DeselectEnable;
            }
            else
            {
                state = CommandItemEnableState_e.DeselectDisable;
            }

            if (m_Enable != null)
            {
                m_Enable.Invoke(m_Cmd, ref state);
            }

            return state;
        }

        private void ExtractCommandInfo(TCmdEnum cmd)
        {
            var cmdEnum = cmd as Enum;

            UserId = Convert.ToInt32(cmdEnum);

            if (!cmdEnum.TryGetAttribute<CommandItemInfoAttribute>(
                att =>
                {
                    HasMenu = att.HasMenu;
                    HasToolbar = att.HasToolbar;
                    SupportedWorkspace = att.SupportedWorkspaces;
                    HasTabBox = att.ShowInCommandTabBox;
                    TabBoxStyle = att.CommandTabBoxDisplayStyle;
                }))
            {
                HasMenu = true;
                HasToolbar = true;
                SupportedWorkspace = swWorkspaceTypes_e.All;
                HasTabBox = false;
                TabBoxStyle = swCommandTabButtonTextDisplay_e.swCommandTabButton_TextBelow;
            }

            if (!cmdEnum.TryGetAttribute<DisplayNameAttribute>(
                att => Title = att.DisplayName))
            {
                Title = cmd.ToString();
            }

            if (!cmdEnum.TryGetAttribute<DescriptionAttribute>(
                att => Tooltip = att.Description))
            {
                Tooltip = cmd.ToString();
            }

            if (!cmdEnum.TryGetAttribute<CommandIconAttribute>(a => Icon = a.Icon))
            {
                var icon = cmdEnum.TryGetAttribute<Common.Attributes.IconAttribute>()?.Icon;

                if (icon == null)
                {
                    icon = Resources.swex_addin_default;
                }

                Icon = new MasterIcon(icon);
            }
        }
    }
}
