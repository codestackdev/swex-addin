//**********************
//SwEx.AddIn - development tools for SOLIDWORKS add-ins
//Copyright(C) 2018 www.codestack.net
//License: https://github.com/codestack-net-dev/sw-dev-tools-addin/blob/master/LICENSE
//Product URL: https://www.codestack.net/labs/solidworks/swex/add-in/
//**********************

using CodeStack.SwEx.AddIn.Icons;
using CodeStack.SwEx.Common.Icons;
using CodeStack.SwEx.Common.Reflection;
using System;
using System.ComponentModel;
using System.Drawing;

namespace CodeStack.SwEx.AddIn.Attributes
{
    [Obsolete]
    [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
    public class IconAttribute : CommandIconAttribute
    {
        public IconAttribute(Type resType, string masterResName)
            : base(resType, masterResName)
        {
        }

        public IconAttribute(Type resType, string size16x16ResName, string size24x24ResName)
            : base(resType, size16x16ResName, size24x24ResName)
        {
        }

        public IconAttribute(Type resType, string size20x20ResName, string size32x32ResName,
            string size40x40ResName, string size64x64ResName, string size96x96ResName, string size128x128ResName)
            : base(resType, size20x20ResName, size32x32ResName, size40x40ResName, size64x64ResName, size96x96ResName, size128x128ResName)
        {
        }
    }

    /// <summary>
    /// Loads the icon information from the resources
    /// </summary>
    /// <remarks>This attribute can be applied to command group (defined as enumeration) and items within the group (defined as enumeration value)</remarks>
    [AttributeUsage(AttributeTargets.All)]
    public class CommandIconAttribute : Attribute
    {
        internal IIcon Icon { get; private set; }

        /// <param name="resType">Type of the static class (usually Resources)</param>
        /// <param name="masterResName">Resource name of the master icon</param>
        public CommandIconAttribute(Type resType, string masterResName)
            : this(ResourceHelper.GetResource<Image>(resType, masterResName))
        {
        }

        /// <param name="resType">Type of the static class (usually Resources)</param>
        /// <param name="size16x16ResName">Resource name of the small icon</param>
        /// <param name="size24x24ResName">Resource name of the large icon</param>
        public CommandIconAttribute(Type resType, string size16x16ResName, string size24x24ResName)
            : this(ResourceHelper.GetResource<Image>(resType, size16x16ResName),
                  ResourceHelper.GetResource<Image>(resType, size24x24ResName))
        {
        }

        /// <param name="resType">Type of the static class (usually Resources)</param>
        /// <param name="size20x20ResName">Resource name of the extra small icon</param>
        /// <param name="size32x32ResName">Resource name of the small icon</param>
        /// <param name="size40x40ResName">Resource name of the medium icon</param>
        /// <param name="size64x64ResName">Resource name of the large icon</param>
        /// <param name="size96x96ResName">Resource name of the extra large icon</param>
        /// <param name="size128x128ResName">Resource name of the high resolution icon</param>
        public CommandIconAttribute(Type resType, string size20x20ResName, string size32x32ResName,
            string size40x40ResName, string size64x64ResName, string size96x96ResName, string size128x128ResName)
            : this(ResourceHelper.GetResource<Image>(resType, size20x20ResName),
                  ResourceHelper.GetResource<Image>(resType, size32x32ResName),
                  ResourceHelper.GetResource<Image>(resType, size40x40ResName),
                  ResourceHelper.GetResource<Image>(resType, size64x64ResName),
                  ResourceHelper.GetResource<Image>(resType, size96x96ResName),
                  ResourceHelper.GetResource<Image>(resType, size128x128ResName))
        {
        }

        private CommandIconAttribute(Image icon)
        {
            Icon = new MasterIcon(icon);
        }

        private CommandIconAttribute(Image size16x16, Image size24x24)
        {
            Icon = new BasicIcon(size16x16, size24x24);
        }

        private CommandIconAttribute(Image size20x20, Image size32x32, 
            Image size40x40, Image size64x64, Image size96x96, Image size128x128)
        {
            Icon = new HighResIcon(
                size20x20, size32x32, size40x40,
                size64x64, size96x96, size128x128);
        }
    }
}
