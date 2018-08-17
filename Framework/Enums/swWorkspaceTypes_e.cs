//**********************
//Development tools for SOLIDWORKS add-ins
//Copyright(C) 2018 www.codestack.net
//License: https://github.com/codestack-net-dev/sw-dev-tools-addin/blob/master/LICENSE
//Product URL: https://www.codestack.net/labs/solidworks/dev-tools-addin/
//**********************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeStack.Dev.Sw.AddIn.Enums
{
    [Flags]
    public enum swWorkspaceTypes_e
    {
        NoDocuments = 1,
        Part = 2 << 0,
        Assembly = 2 << 1,
        Drawing = 2 << 2,
        AllDocuments = Part | Assembly | Drawing,
        All = AllDocuments | NoDocuments
    }
}
