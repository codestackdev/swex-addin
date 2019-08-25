//**********************
//SwEx.AddIn - development tools for SOLIDWORKS add-ins
//Copyright(C) 2019 www.codestack.net
//License: https://github.com/codestackdev/swex-addin/blob/master/LICENSE
//Product URL: https://www.codestack.net/labs/solidworks/swex/add-in/
//**********************

using CodeStack.SwEx.AddIn.Icons;
using CodeStack.SwEx.Common.Reflection;
using System;
using System.Drawing;

namespace CodeStack.SwEx.AddIn.Attributes
{
    /// <summary>
    /// Allows to assign task pane specific icon
    /// </summary>
    [AttributeUsage(AttributeTargets.All)]
    public class TaskPaneIconAttribute : Attribute
    {
        internal CommandGroupIcon Icon { get; private set; }

        /// <param name="resType">Type of the static class (usually Resources)</param>
        /// <param name="masterResName">Resource name of the master icon</param>
        public TaskPaneIconAttribute(Type resType, string masterResName)
            : this(ResourceHelper.GetResource<Image>(resType, masterResName))
        {
        }

        /// <param name="resType">Type of the static class (usually Resources)</param>
        /// <param name="size20x20ResName">Resource name of the extra small icon</param>
        /// <param name="size32x32ResName">Resource name of the small icon</param>
        /// <param name="size40x40ResName">Resource name of the medium icon</param>
        /// <param name="size64x64ResName">Resource name of the large icon</param>
        /// <param name="size96x96ResName">Resource name of the extra large icon</param>
        /// <param name="size128x128ResName">Resource name of the high resolution icon</param>
        public TaskPaneIconAttribute(Type resType, string size20x20ResName, string size32x32ResName,
            string size40x40ResName, string size64x64ResName, string size96x96ResName, string size128x128ResName)
            : this(ResourceHelper.GetResource<Image>(resType, size20x20ResName),
                  ResourceHelper.GetResource<Image>(resType, size32x32ResName),
                  ResourceHelper.GetResource<Image>(resType, size40x40ResName),
                  ResourceHelper.GetResource<Image>(resType, size64x64ResName),
                  ResourceHelper.GetResource<Image>(resType, size96x96ResName),
                  ResourceHelper.GetResource<Image>(resType, size128x128ResName))
        {
        }

        private TaskPaneIconAttribute(Image icon)
        {
            Icon = new TaskPaneMasterIcon(icon);
        }

        private TaskPaneIconAttribute(Image size20x20, Image size32x32,
            Image size40x40, Image size64x64, Image size96x96, Image size128x128)
        {
            Icon = new TaskPaneHighResIcon(
                size20x20, size32x32, size40x40,
                size64x64, size96x96, size128x128);
        }
    }
}
