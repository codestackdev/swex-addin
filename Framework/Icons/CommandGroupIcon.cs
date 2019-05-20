//**********************
//SwEx.AddIn - development tools for SOLIDWORKS add-ins
//Copyright(C) 2019 www.codestack.net
//License: https://github.com/codestack-net-dev/sw-dev-tools-addin/blob/master/LICENSE
//Product URL: https://www.codestack.net/labs/solidworks/swex/add-in/
//**********************

using CodeStack.SwEx.Common.Icons;
using System.Collections.Generic;
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
