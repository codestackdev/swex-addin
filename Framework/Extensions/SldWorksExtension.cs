//**********************
//SwEx.AddIn - development tools for SOLIDWORKS add-ins
//Copyright(C) 2019 www.codestack.net
//License: https://github.com/codestackdev/swex-addin/blob/master/LICENSE
//Product URL: https://www.codestack.net/labs/solidworks/swex/add-in/
//**********************

using System.Diagnostics;

namespace SolidWorks.Interop.sldworks
{
    internal static class SldWorksExtension
    {
        internal enum HighResIconsScope_e
        {
            CommandManager,
            TaskPane
        }

        internal enum SolidWorksRevisions_e
        {
            Sw2016 = 24,
            Sw2017 = 25
        }

        internal static bool SupportsHighResIcons(this ISldWorks app, HighResIconsScope_e scope)
        {
            var majorRev = int.Parse(app.RevisionNumber().Split('.')[0]);

            switch (scope)
            {
                case HighResIconsScope_e.CommandManager:
                    return majorRev >= (int)SolidWorksRevisions_e.Sw2016;

                case HighResIconsScope_e.TaskPane:
                    return majorRev >= (int)SolidWorksRevisions_e.Sw2017;

                default:
                    Debug.Assert(false, "Not supported scope");
                    return false;
            }
        }
    }
}
