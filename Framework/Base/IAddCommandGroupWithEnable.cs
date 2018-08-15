using CodeStack.Dev.Sw.AddIn.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeStack.Dev.Sw.AddIn
{
    public interface IAddCommandGroupWithEnable<TCmdEnum> : IAddCommandGroup<TCmdEnum>
            where TCmdEnum : IComparable, IFormattable, IConvertible
    {
        void Enable(TCmdEnum cmd, ref CommandItemEnableState_e state);
    }
}
