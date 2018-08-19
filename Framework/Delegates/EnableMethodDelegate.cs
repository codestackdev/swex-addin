using CodeStack.Dev.Sw.AddIn.Enums;
using System;

namespace CodeStack.Dev.Sw.AddIn
{
    public delegate void EnableMethodDelegate<TCmdEnum>(TCmdEnum cmd, ref CommandItemEnableState_e state)
                where TCmdEnum : IComparable, IFormattable, IConvertible;
}
