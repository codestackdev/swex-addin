//icons by: http://icons8.com

using CodeStack.SwEx.AddIn.Attributes;
using CodeStack.SwEx.AddIn.Enums;
using CodeStack.SwEx.AddIn.Example.Properties;
using CodeStack.SwEx.AddIn.Helpers;
using SolidWorksTools;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace CodeStack.SwEx.AddIn.Example
{
    [Common.Attributes.Title("AddInEx Commands")]
    [Description("Sample commands")]
    [Common.Attributes.Icon(typeof(Resources), nameof(Resources.command_group_icon))]
    [CommandGroupInfo(0)]
    public enum Commands_e
    {
        [Common.Attributes.Title("Command1")]
        [Description("Sample Command 1")]
        [Common.Attributes.Icon(typeof(Resources), nameof(Resources.command1_icon))]
        [CommandItemInfo(true, true, swWorkspaceTypes_e.AllDocuments, true)]
        Command1,

        [Common.Attributes.Title("Command 2")]
        [Description("Sample Command2")]
        [CommandIcon(typeof(Resources), nameof(Resources.command2_icon), nameof(Resources.command2_icon))]
        [CommandItemInfo(true, true, swWorkspaceTypes_e.All, true)]
        Command2,

        Command3,
    }

    [Guid("86EA567D-79FA-4E3B-B66E-EAB660DB3D47"), ComVisible(true)]
    [AutoRegister("Sample AddInEx", "Sample AddInEx", true)]
    public class SwSampleAddIn : SwAddInEx
    {
        public override bool OnConnect()
        {
            AddCommandGroup<Commands_e>(OnCommandClick, OnCommandEnable);
            
            return true;
        }

        public override bool OnDisconnect()
        {
            return base.OnDisconnect();
        }

        private void OnCommandClick(Commands_e cmd)
        {
            switch (cmd)
            {
                case Commands_e.Command1:
                    App.SendMsgToUser("Command1 clicked!");
                    break;

                case Commands_e.Command2:
                    App.SendMsgToUser("Command2 clicked!");
                    break;

                case Commands_e.Command3:
                    App.SendMsgToUser("Command3 clicked!");
                    break;
            }
        }

        private void OnCommandEnable(Commands_e cmd, ref CommandItemEnableState_e state)
        {
            if (cmd == Commands_e.Command1)
            {
                if (state == CommandItemEnableState_e.DeselectEnable)
                {
                    if (App.IActiveDoc2?.ISelectionManager?.GetSelectedObjectCount2(-1) == 0)
                    {
                        state = CommandItemEnableState_e.DeselectDisable;
                    }
                }
            }
        }
    }
}
