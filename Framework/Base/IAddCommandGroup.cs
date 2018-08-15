using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeStack.Dev.Sw.AddIn
{
    public interface IAddCommandGroup<TCmdEnum>
            where TCmdEnum : IComparable, IFormattable, IConvertible
    {
        void Callback(TCmdEnum cmd);
    }
}
