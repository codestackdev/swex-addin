using SolidWorks.Interop.swconst;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CodeStack.SwEx.AddIn.Attributes
{
    [AttributeUsage(AttributeTargets.Field)]
    public class TaskPaneStandardButtonAttribute : Attribute
    {
        internal swTaskPaneBitmapsOptions_e Icon { get; private set; }

        public TaskPaneStandardButtonAttribute(swTaskPaneBitmapsOptions_e icon)
        {
            Icon = icon;
        }
    }
}
