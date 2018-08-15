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
