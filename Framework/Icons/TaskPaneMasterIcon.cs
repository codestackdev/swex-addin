//**********************
//SwEx.AddIn - development tools for SOLIDWORKS add-ins
//Copyright(C) 2019 www.codestack.net
//License: https://github.com/codestackdev/swex-addin/blob/master/LICENSE
//Product URL: https://www.codestack.net/labs/solidworks/swex/add-in/
//**********************

using CodeStack.SwEx.Common.Icons;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;

namespace CodeStack.SwEx.AddIn.Icons
{
    [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
    internal class TaskPaneMasterIcon : MasterIcon
    {
        private readonly Image m_Icon;

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

        internal TaskPaneMasterIcon(Image icon) : base(icon)
        {
            m_Icon = icon;
        }
    }
}
