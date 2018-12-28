using CodeStack.SwEx.AddIn.Enums;
using SolidWorks.Interop.swconst;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace CodeStack.SwEx.AddIn.Base
{
    [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
    public interface ICommand : ICommandBase
    {
        swWorkspaceTypes_e SupportedWorkspace { get; }
        bool HasMenu { get; }
        bool HasToolbar { get; }
        bool HasTabBox { get; }
        int UserId { get; }
        swCommandTabButtonTextDisplay_e TabBoxStyle { get; }
        CommandItemEnableState_e OnEnable();
        void OnClick();
    }
}
