//**********************
//SwEx.AddIn - development tools for SOLIDWORKS add-ins
//Copyright(C) 2018 www.codestack.net
//License: https://github.com/codestack-net-dev/sw-dev-tools-addin/blob/master/LICENSE
//Product URL: https://www.codestack.net/labs/solidworks/swex/add-in/
//**********************

using CodeStack.SwEx.Common.Reflection;
using System;
using System.ComponentModel;

namespace CodeStack.SwEx.AddIn.Attributes
{
    /// <summary>
    /// Decorates the display name for the element (e.g. command)
    /// </summary>
    /// <remarks>Can be applied to can be applied to command group (defined as enumeration) and items within the group (defined as enumeration value)</remarks>
    [AttributeUsage(AttributeTargets.All)]
    [Obsolete("Deprecated. Use CodeStack.SwEx.Common.Attributes.TitleAttribute instead")]
    [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
    public class TitleAttribute : CodeStack.SwEx.Common.Attributes.TitleAttribute
    {
        public TitleAttribute(string dispName) : base(dispName)
        {
        }

        public TitleAttribute(Type resType, string dispNameResName)
            : base(ResourceHelper.GetResource<string>(resType, dispNameResName))
        {
        }
    }
}
