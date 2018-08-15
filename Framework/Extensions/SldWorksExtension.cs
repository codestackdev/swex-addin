using CodeStack.Dev.Sw.AddIn.Attributes;
using CodeStack.Dev.Sw.AddIn.Enums;
using CodeStack.Dev.Sw.AddIn.Icons;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolidWorks.Interop.sldworks
{
    internal static class SldWorksExtension
    {
        internal static bool SupportsHighResIcons(this ISldWorks app)
        {
            const int SW_2016_REV = 24;

            var majorRev = int.Parse(app.RevisionNumber().Split('.')[0]);

            return majorRev >= SW_2016_REV;
        }
    }
}
