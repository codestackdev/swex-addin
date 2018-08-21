//**********************
//Development tools for SOLIDWORKS add-ins
//Copyright(C) 2018 www.codestack.net
//License: https://github.com/codestack-net-dev/sw-dev-tools-addin/blob/master/LICENSE
//Product URL: https://www.codestack.net/labs/solidworks/dev-tools-addin/
//**********************

using CodeStack.Dev.Sw.AddIn.Enums;
using System;

namespace CodeStack.Dev.Sw.AddIn.Attributes
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
        /// Indicates the workspaces where this command is enabled
        /// </summary>
        /// <remarks>This information is used in the default command enable handler</remarks>
        public swWorkspaceTypes_e SupportedWorkspaces { get; private set; }

        public CommandItemInfoAttribute(bool hasMenu, bool hasToolbar, swWorkspaceTypes_e suppWorkspaces)
        {
            HasMenu = hasMenu;
            HasToolbar = hasToolbar;
            SupportedWorkspaces = suppWorkspaces;
        }
    }
}
