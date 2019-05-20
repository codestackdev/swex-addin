//**********************
//SwEx.AddIn - development tools for SOLIDWORKS add-ins
//Copyright(C) 2019 www.codestack.net
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
    public class HighResIcon : CommandGroupIcon
    {
        private readonly Image m_Size20x20;
        private readonly Image m_Size32x32;
        private readonly Image m_Size40x40;
        private readonly Image m_Size64x64;
        private readonly Image m_Size96x96;
        private readonly Image m_Size128x128;

        internal HighResIcon(Image size20x20, Image size32x32,
            Image size40x40, Image size64x64, Image size96x96, Image size128x128)
        {
            m_Size20x20 = size20x20;
            m_Size32x32 = size32x32;
            m_Size40x40 = size40x40;
            m_Size64x64 = size64x64;
            m_Size96x96 = size96x96;
            m_Size128x128 = size128x128;
        }

        public override IEnumerable<IconSizeInfo> GetHighResolutionIconSizes()
        {
            yield return new IconSizeInfo(m_Size20x20, new Size(20, 20));
            yield return new IconSizeInfo(m_Size32x32, new Size(32, 32));
            yield return new IconSizeInfo(m_Size40x40, new Size(40, 40));
            yield return new IconSizeInfo(m_Size64x64, new Size(64, 64));
            yield return new IconSizeInfo(m_Size96x96, new Size(96, 96));
            yield return new IconSizeInfo(m_Size128x128, new Size(128, 128));
        }

        public override IEnumerable<IconSizeInfo> GetIconSizes()
        {
            yield return new IconSizeInfo(m_Size20x20, new Size(16, 16));
            yield return new IconSizeInfo(m_Size32x32, new Size(24, 24));
        }
    }
}
