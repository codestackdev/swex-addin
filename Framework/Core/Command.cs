using CodeStack.SwEx.AddIn.Base;
using CodeStack.SwEx.AddIn.Enums;
using CodeStack.SwEx.Common.Icons;
using SolidWorks.Interop.swconst;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace CodeStack.SwEx.AddIn.Core
{
    [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
    public class Command : ICommand
    {
        public string Title { get; protected set; }
        public string Tooltip { get; protected set; }
        public IIcon Icon { get; protected set; }

        public swWorkspaceTypes_e SupportedWorkspace { get; protected set; }
        public bool HasMenu { get; protected set; }
        public bool HasToolbar { get; protected set; }
        public bool HasTabBox { get; protected set; }
        public int UserId { get; protected set; }
        public swCommandTabButtonTextDisplay_e TabBoxStyle { get; protected set; }

        public virtual CommandItemEnableState_e OnEnable()
        {
            return CommandItemEnableState_e.DeselectEnable;
        }

        public virtual void OnClick()
        {
        }
    }
}
