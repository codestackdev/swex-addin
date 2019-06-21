//**********************
//SwEx.AddIn - development tools for SOLIDWORKS add-ins
//Copyright(C) 2019 www.codestack.net
//License: https://github.com/codestack-net-dev/sw-dev-tools-addin/blob/master/LICENSE
//Product URL: https://www.codestack.net/labs/solidworks/swex/add-in/
//**********************

using System;

namespace CodeStack.SwEx.AddIn.Attributes
{
    /// <summary>
    /// Marks the command to be separated by the spacer (separator) in the menu and the toolbar
    /// </summary>
    /// <remarks>Spacer is added before the command marked with this attribute</remarks>
    [AttributeUsage(AttributeTargets.Field)]
    public class CommandSpacerAttribute : Attribute
    {
    }
}
