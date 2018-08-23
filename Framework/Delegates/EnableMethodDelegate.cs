using CodeStack.SwEx.AddIn.Enums;
using System;

namespace CodeStack.SwEx.AddIn
{
    public delegate void EnableMethodDelegate<TCmdEnum>(TCmdEnum cmd, ref CommandItemEnableState_e state)
                where TCmdEnum : IComparable, IFormattable, IConvertible;
}
