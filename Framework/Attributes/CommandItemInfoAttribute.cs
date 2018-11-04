//**********************
//SwEx.AddIn - development tools for SOLIDWORKS add-ins
//Copyright(C) 2018 www.codestack.net
//License: https://github.com/codestack-net-dev/sw-dev-tools-addin/blob/master/LICENSE
//Product URL: https://www.codestack.net/labs/solidworks/swex/add-in/
//**********************

using CodeStack.SwEx.AddIn.Enums;
using SolidWorks.Interop.swconst;
using System;

namespace CodeStack.SwEx.AddIn.Attributes
{
    /// <summary>
    /// Provides additional information about the item command
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class CommandItemInfoAttribute : Attribute
    {
        /// <summary>
        /// Indicates if this command should be displayed in the toolbar
        /// </summary>
        public bool HasToolbar { get; private set; }

        /// <summary>
        /// Indicates if this command should be displayed in the menu
        /// </summary>
        public bool HasMenu { get; private set; }

        /// <summary>
        /// Indicates that this command should be added to command tab box in command manager (ribbon)
        /// </summary>
        public bool ShowInCommandTabBox { get; private set; }

        /// <summary>
        /// Text display type for command in command tab box as defined in <see href="https://help.solidworks.com/2012/English/api/swconst/SolidWorks.Interop.swconst~SolidWorks.Interop.swconst.swCommandTabButtonTextDisplay_e.html?id=3d6975f51c4648378ad4beaf4d3144ca"/>
        /// </summary>
        /// <remarks>This option is applicable when <see cref="ShowInCommandTabBox"/> is set to true</remarks>
        public swCommandTabButtonTextDisplay_e CommandTabBoxDisplayStyle { get; private set; }

        /// <summary>
        /// Indicates the workspaces where this command is enabled
        /// </summary>
        /// <remarks>This information is used in the default command enable handler</remarks>
        public swWorkspaceTypes_e SupportedWorkspaces { get; private set; }

        public CommandItemInfoAttribute(swWorkspaceTypes_e suppWorkspaces)
            : this(true, true, suppWorkspaces)
        {
        }

        public CommandItemInfoAttribute(bool hasMenu, bool hasToolbar, swWorkspaceTypes_e suppWorkspaces)
            : this (hasMenu, hasToolbar, suppWorkspaces, false)
        {
        }

        public CommandItemInfoAttribute(bool hasMenu, bool hasToolbar, swWorkspaceTypes_e suppWorkspaces,
            bool showInCmdTabBox, swCommandTabButtonTextDisplay_e textStyle = swCommandTabButtonTextDisplay_e.swCommandTabButton_TextBelow)
        {
            HasMenu = hasMenu;
            HasToolbar = hasToolbar;
            SupportedWorkspaces = suppWorkspaces;
            ShowInCommandTabBox = showInCmdTabBox;
            CommandTabBoxDisplayStyle = textStyle;
        }
    }
}
