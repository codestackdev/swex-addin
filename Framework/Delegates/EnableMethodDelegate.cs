//**********************
//SwEx.AddIn - development tools for SOLIDWORKS add-ins
//Copyright(C) 2019 www.codestack.net
//License: https://github.com/codestack-net-dev/sw-dev-tools-addin/blob/master/LICENSE
//Product URL: https://www.codestack.net/labs/solidworks/swex/add-in/
//**********************

using CodeStack.SwEx.AddIn.Attributes;
using CodeStack.SwEx.AddIn.Enums;
using System;

namespace CodeStack.SwEx.AddIn
{
    /// <summary>
    /// Method handler to provide the custom enable command behavior
    /// </summary>
    /// <typeparam name="TCmdEnum">Command defined in the enumerator</typeparam>
    /// <param name="cmd">Command id</param>
    /// <param name="state">State of the command</param>
    /// <remarks>State passed to this method is already assigned based on the value of <see cref="CommandItemInfoAttribute.SupportedWorkspaces"/> options,
    /// However this method allows to reset the state based on custom logic (e.g. disable if no elements selected in the graphics view)</remarks>
    public delegate void EnableMethodDelegate<TCmdEnum>(TCmdEnum cmd, ref CommandItemEnableState_e state)
                where TCmdEnum : IComparable, IFormattable, IConvertible;
}
