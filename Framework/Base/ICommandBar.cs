using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace CodeStack.SwEx.AddIn.Base
{
    [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
    public interface ICommandBar : ICommandBase
    {
        int Id { get; }
        ICommand[] Commands { get; }
    }
}
