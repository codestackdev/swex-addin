//**********************
//SwEx.AddIn - development tools for SOLIDWORKS add-ins
//Copyright(C) 2018 www.codestack.net
//License: https://github.com/codestack-net-dev/sw-dev-tools-addin/blob/master/LICENSE
//Product URL: https://www.codestack.net/labs/solidworks/swex/add-in/
//**********************

using CodeStack.SwEx.Common.Icons;
using System.Collections.Generic;
using System.Drawing;

namespace CodeStack.SwEx.AddIn.Icons
{
    internal class HighResIcon : CommandGroupIcon
    {
        internal Image Size20x20 { get; set; }
        internal Image Size32x32 { get; set; }
        internal Image Size40x40 { get; set; }
        internal Image Size64x64 { get; set; }
        internal Image Size96x96 { get; set; }
        internal Image Size128x128 { get; set; }

        public override IEnumerable<IconSizeInfo> GetHighResolutionIconSizes()
        {
            yield return new IconSizeInfo(Size20x20, new Size(20, 20));
            yield return new IconSizeInfo(Size32x32, new Size(32, 32));
            yield return new IconSizeInfo(Size40x40, new Size(40, 40));
            yield return new IconSizeInfo(Size64x64, new Size(64, 64));
            yield return new IconSizeInfo(Size96x96, new Size(96, 96));
            yield return new IconSizeInfo(Size128x128, new Size(128, 128));
        }

        public override IEnumerable<IconSizeInfo> GetIconSizes()
        {
            yield return new IconSizeInfo(Size20x20, new Size(16, 16));
            yield return new IconSizeInfo(Size32x32, new Size(24, 24));
        }
    }
}
