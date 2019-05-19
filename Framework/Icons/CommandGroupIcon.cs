using CodeStack.SwEx.Common.Icons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.ComponentModel;

namespace CodeStack.SwEx.AddIn.Icons
{
    [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
    public abstract class CommandGroupIcon : IIcon
    {
        private static readonly Color m_CommandTransparencyKey
            = Color.FromArgb(192, 192, 192);

        public virtual Color TransparencyKey
        {
            get
            {
                return m_CommandTransparencyKey;
            }
        }

        public abstract IEnumerable<IconSizeInfo> GetHighResolutionIconSizes();
        public abstract IEnumerable<IconSizeInfo> GetIconSizes();
    }
}
