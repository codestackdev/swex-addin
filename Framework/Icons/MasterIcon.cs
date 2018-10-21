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
    internal class MasterIcon : CommandGroupIcon
    {
        internal Image Icon { get; set; }

        public override IEnumerable<IconSizeInfo> GetHighResolutionIconSizes()
        {
            yield return new IconSizeInfo(Icon, new Size(20, 20));
            yield return new IconSizeInfo(Icon, new Size(32, 32));
            yield return new IconSizeInfo(Icon, new Size(40, 40));
            yield return new IconSizeInfo(Icon, new Size(64, 64));
            yield return new IconSizeInfo(Icon, new Size(96, 96));
            yield return new IconSizeInfo(Icon, new Size(128, 128));
        }

        public override IEnumerable<IconSizeInfo> GetIconSizes()
        {
            yield return new IconSizeInfo(Icon, new Size(16, 16));
            yield return new IconSizeInfo(Icon, new Size(24, 24));
        }
    }
}
