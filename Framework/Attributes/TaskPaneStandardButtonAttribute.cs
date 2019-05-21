//**********************
//SwEx.AddIn - development tools for SOLIDWORKS add-ins
//Copyright(C) 2019 www.codestack.net
//License: https://github.com/codestack-net-dev/sw-dev-tools-addin/blob/master/LICENSE
//Product URL: https://www.codestack.net/labs/solidworks/swex/add-in/
//**********************

using SolidWorks.Interop.swconst;
using System;

namespace CodeStack.SwEx.AddIn.Attributes
{
    /// <summary>
    /// Allows to assign the standard icon for the task pane command
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class TaskPaneStandardButtonAttribute : Attribute
    {
        internal swTaskPaneBitmapsOptions_e Icon { get; private set; }

        /// <param name="icon">Standard task pane icon</param>
        public TaskPaneStandardButtonAttribute(swTaskPaneBitmapsOptions_e icon)
        {
            Icon = icon;
        }
    }
}
