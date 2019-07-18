//**********************
//SwEx.AddIn - development tools for SOLIDWORKS add-ins
//Copyright(C) 2019 www.codestack.net
//License: https://github.com/codestack-net-dev/sw-dev-tools-addin/blob/master/LICENSE
//Product URL: https://www.codestack.net/labs/solidworks/swex/add-in/
//**********************

using CodeStack.SwEx.AddIn.Base;
using CodeStack.SwEx.AddIn.Enums;
using CodeStack.SwEx.AddIn.Icons;
using SolidWorks.Interop.swconst;
using System.ComponentModel;
using System;

namespace CodeStack.SwEx.AddIn.Core
{
    [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
    public class CommandSpec : ICommandSpec
    {
        public string Title { get; protected set; }
        public string Tooltip { get; protected set; }
        public CommandGroupIcon Icon { get; protected set; }

        public swWorkspaceTypes_e SupportedWorkspace { get; protected set; }
        public bool HasMenu { get; protected set; }
        public bool HasToolbar { get; protected set; }
        public bool HasTabBox { get; protected set; }
        public int UserId { get; protected set; }
        public swCommandTabButtonTextDisplay_e TabBoxStyle { get; protected set; }
        public bool HasSpacer { get; protected set; }

        public virtual CommandItemEnableState_e OnEnable()
        {
            return CommandItemEnableState_e.DeselectEnable;
        }

        public virtual void OnClick()
        {
        }
    }
}
