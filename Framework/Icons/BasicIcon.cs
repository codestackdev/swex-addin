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
    internal class BasicIcon : IIcon
    {
        public Image Size16x16 { get; set; }
        public Image Size24x24 { get; set; }

        public IEnumerable<IconSizeInfo> GetHighResolutionIconSizes()
        {
            yield return new IconSizeInfo(Size16x16, new Size(20, 20));
            yield return new IconSizeInfo(Size24x24, new Size(32, 32));
            yield return new IconSizeInfo(Size24x24, new Size(40, 40));
            yield return new IconSizeInfo(Size24x24, new Size(64, 64));
            yield return new IconSizeInfo(Size24x24, new Size(96, 96));
            yield return new IconSizeInfo(Size24x24, new Size(128, 128));
        }

        public IEnumerable<IconSizeInfo> GetIconSizes()
        {
            yield return new IconSizeInfo(Size16x16, new Size(16, 16));
            yield return new IconSizeInfo(Size24x24, new Size(24, 24));
        }
    }
}
