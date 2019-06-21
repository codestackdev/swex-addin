//**********************
//SwEx.AddIn - development tools for SOLIDWORKS add-ins
//Copyright(C) 2019 www.codestack.net
//License: https://github.com/codestack-net-dev/sw-dev-tools-addin/blob/master/LICENSE
//Product URL: https://www.codestack.net/labs/solidworks/swex/add-in/
//**********************

using CodeStack.SwEx.AddIn.Enums;
using SolidWorks.Interop.swconst;
using System.ComponentModel;

namespace CodeStack.SwEx.AddIn.Base
{
    [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
    public interface ICommandSpec : ICommandBaseSpec
    {
        swWorkspaceTypes_e SupportedWorkspace { get; }
        bool HasMenu { get; }
        bool HasToolbar { get; }
        bool HasTabBox { get; }
        int UserId { get; }
        bool HasSpacer { get; }
        swCommandTabButtonTextDisplay_e TabBoxStyle { get; }
        CommandItemEnableState_e OnEnable();
        void OnClick();
    }
}
