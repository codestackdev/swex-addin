//**********************
//SwEx.AddIn - development tools for SOLIDWORKS add-ins
//Copyright(C) 2019 www.codestack.net
//License: https://github.com/codestackdev/swex-addin/blob/master/LICENSE
//Product URL: https://www.codestack.net/labs/solidworks/swex/add-in/
//**********************

using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using CodeStack.SwEx.Common.Icons;

namespace CodeStack.SwEx.AddIn.Icons
{
    [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
    internal class TaskPaneHighResIcon : HighResIcon
    {
        private readonly Image m_Icon;

        internal TaskPaneHighResIcon(Image size20x20, Image size32x32,
            Image size40x40, Image size64x64, Image size96x96, Image size128x128)
            : base(size20x20, size32x32, size40x40, size64x64, size96x96, size128x128)
        {
            m_Icon = size20x20;
        }

        public override Color TransparencyKey
        {
            get
            {
                return Color.White;
            }
        }

        public override IEnumerable<IconSizeInfo> GetIconSizes()
        {
            yield return new IconSizeInfo(m_Icon, new Size(16, 18));
        }
    }
}
