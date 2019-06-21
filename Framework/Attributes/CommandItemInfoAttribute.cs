//**********************
//SwEx.AddIn - development tools for SOLIDWORKS add-ins
//Copyright(C) 2019 www.codestack.net
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
        internal bool HasToolbar { get; private set; }
        internal bool HasMenu { get; private set; }
        internal bool ShowInCommandTabBox { get; private set; }
        internal swCommandTabButtonTextDisplay_e CommandTabBoxDisplayStyle { get; private set; }
        internal swWorkspaceTypes_e SupportedWorkspaces { get; private set; }

        /// <summary>
        /// Constructor for specifying additional information about command item
        /// </summary>
        /// <param name="suppWorkspaces">Indicates the workspaces where this command is enabled. This information is used in the default command enable handler</param>
        public CommandItemInfoAttribute(swWorkspaceTypes_e suppWorkspaces)
            : this(true, true, suppWorkspaces)
        {
        }

        /// <inheritdoc cref="CommandItemInfoAttribute(swWorkspaceTypes_e)"/>
        /// <param name="hasMenu">Indicates if this command should be displayed in the menu</param>
        /// <param name="hasToolbar">Indicates if this command should be displayed in the toolbar</param>
        public CommandItemInfoAttribute(bool hasMenu, bool hasToolbar, swWorkspaceTypes_e suppWorkspaces)
            : this (hasMenu, hasToolbar, suppWorkspaces, false)
        {
        }

        /// <inheritdoc cref="CommandItemInfoAttribute(bool, bool, swWorkspaceTypes_e)"/>
        /// <param name="showInCmdTabBox">Indicates that this command should be added to command tab box in command manager (ribbon)</param>
        /// <param name="textStyle">Text display type for command in command tab box as defined in <see href="https://help.solidworks.com/2012/English/api/swconst/SolidWorks.Interop.swconst~SolidWorks.Interop.swconst.swCommandTabButtonTextDisplay_e.html?id=3d6975f51c4648378ad4beaf4d3144ca">swCommandTabButtonTextDisplay_e Enumeration</see>.
        /// This option is applicable when 'showInCmdTabBox' is set to true</param>
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
