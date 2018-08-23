//**********************
//Development tools for SOLIDWORKS add-ins
//Copyright(C) 2018 www.codestack.net
//License: https://github.com/codestack-net-dev/sw-dev-tools-addin/blob/master/LICENSE
//Product URL: https://www.codestack.net/labs/solidworks/dev-tools-addin/
//**********************

using CodeStack.SwEx.AddIn.Attributes;
using CodeStack.SwEx.AddIn.Enums;
using CodeStack.SwEx.AddIn.Icons;
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
