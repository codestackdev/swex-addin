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
    public class CommandItemInfoAttribute : Attribute
    {
        public bool HasToolbar { get; private set; }
        public bool HasMenu { get; private set; }
        public swWorkspaceTypes_e SupportedWorkspaces { get; private set; }

        public CommandItemInfoAttribute(bool hasMenu, bool hasToolbar, swWorkspaceTypes_e suppWorkspaces)
        {
            HasMenu = hasMenu;
            HasToolbar = hasToolbar;
            SupportedWorkspaces = suppWorkspaces;
        }
    }
}
