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
using System.Drawing;

namespace CodeStack.SwEx.AddIn.Attributes
{
    /// <summary>
    /// Loads the icon information from the resources
    /// </summary>
    /// <remarks>This attribute can be applied to command group (defined as enumeration) and items within the group (defined as enumeration value)</remarks>
    [AttributeUsage(AttributeTargets.All)]
    public class IconAttribute : Attribute
    {
        internal IIcon Icon { get; private set; }

        /// <param name="resType">Type of the static class (usually Resources)</param>
        /// <param name="masterResName">Resource name of the master icon</param>
        public IconAttribute(Type resType, string masterResName)
            : this(ResourceHelper.GetResource<Image>(resType, masterResName))
        {
        }

        /// <param name="resType">Type of the static class (usually Resources)</param>
        /// <param name="size16x16ResName">Resource name of the small icon</param>
        /// <param name="size24x24ResName">Resource name of the large icon</param>
        public IconAttribute(Type resType, string size16x16ResName, string size24x24ResName)
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
        public IconAttribute(Type resType, string size20x20ResName, string size32x32ResName,
            string size40x40ResName, string size64x64ResName, string size96x96ResName, string size128x128ResName)
            : this(ResourceHelper.GetResource<Image>(resType, size20x20ResName),
                  ResourceHelper.GetResource<Image>(resType, size32x32ResName),
                  ResourceHelper.GetResource<Image>(resType, size40x40ResName),
                  ResourceHelper.GetResource<Image>(resType, size64x64ResName),
                  ResourceHelper.GetResource<Image>(resType, size96x96ResName),
                  ResourceHelper.GetResource<Image>(resType, size128x128ResName))
        {
        }

        private IconAttribute(Image icon)
        {
            Icon = new MasterIcon()
            {
                Icon = icon
            };
        }

        private IconAttribute(Image size16x16, Image size24x24)
        {
            Icon = new BasicIcon()
            {
                Size16x16 = size16x16,
                Size24x24 = size24x24
            };
        }

        private IconAttribute(Image size20x20, Image size32x32, 
            Image size40x40, Image size64x64, Image size96x96, Image size128x128)
        {
            Icon = new HighResIcon()
            {
                Size20x20 = size20x20,
                Size32x32 = size32x32,
                Size40x40 = size40x40,
                Size64x64 = size64x64,
                Size96x96 = size96x96, 
                Size128x128 = size128x128
            };
        }
    }
}
