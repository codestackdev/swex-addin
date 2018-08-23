//**********************
//SwEx.AddIn - development tools for SOLIDWORKS add-ins
//Copyright(C) 2018 www.codestack.net
//License: https://github.com/codestack-net-dev/sw-dev-tools-addin/blob/master/LICENSE
//Product URL: https://www.codestack.net/labs/solidworks/dev-tools-addin/
//**********************

using CodeStack.SwEx.AddIn.Enums;
using System;

namespace CodeStack.SwEx.AddIn
{
    public delegate void EnableMethodDelegate<TCmdEnum>(TCmdEnum cmd, ref CommandItemEnableState_e state)
                where TCmdEnum : IComparable, IFormattable, IConvertible;
}
