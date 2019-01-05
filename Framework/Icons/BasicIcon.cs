//**********************
//SwEx.AddIn - development tools for SOLIDWORKS add-ins
//Copyright(C) 2018 www.codestack.net
//License: https://github.com/codestack-net-dev/sw-dev-tools-addin/blob/master/LICENSE
//Product URL: https://www.codestack.net/labs/solidworks/swex/add-in/
//**********************

using CodeStack.SwEx.Common.Icons;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;

namespace CodeStack.SwEx.AddIn.Icons
{
    [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
    internal class BasicIcon : CommandGroupIcon
    {
        private readonly Image m_Size16x16;
        private readonly Image m_Size24x24;

        public BasicIcon(Image size16x16, Image size24x24)
        {
            m_Size16x16 = size16x16;
            m_Size24x24 = size24x24;
        }

        public override IEnumerable<IconSizeInfo> GetHighResolutionIconSizes()
        {
            yield return new IconSizeInfo(m_Size16x16, new Size(20, 20));
            yield return new IconSizeInfo(m_Size24x24, new Size(32, 32));
            yield return new IconSizeInfo(m_Size24x24, new Size(40, 40));
            yield return new IconSizeInfo(m_Size24x24, new Size(64, 64));
            yield return new IconSizeInfo(m_Size24x24, new Size(96, 96));
            yield return new IconSizeInfo(m_Size24x24, new Size(128, 128));
        }

        public override IEnumerable<IconSizeInfo> GetIconSizes()
        {
            yield return new IconSizeInfo(m_Size16x16, new Size(16, 16));
            yield return new IconSizeInfo(m_Size24x24, new Size(24, 24));
        }
    }
}
