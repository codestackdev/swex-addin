using CodeStack.SwEx.Common.Icons;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;

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
