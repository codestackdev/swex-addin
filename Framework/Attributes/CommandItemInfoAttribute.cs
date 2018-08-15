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
        public swCommandItemType_e MenuToolbarVisibility { get; private set; }
        public swWorkspaceTypes_e SupportedWorkspaces { get; private set; }

        public CommandItemInfoAttribute(swCommandItemType_e itemType, swWorkspaceTypes_e suppWorkspaces)
        {
            MenuToolbarVisibility = itemType;
            SupportedWorkspaces = suppWorkspaces;
        }
    }
}
