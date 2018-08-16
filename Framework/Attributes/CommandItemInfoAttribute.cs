using CodeStack.Dev.Sw.AddIn.Enums;
using SolidWorks.Interop.swconst;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
