using CodeStack.SwEx.AddIn.Icons;
using CodeStack.SwEx.Common.Icons;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace CodeStack.SwEx.AddIn.Base
{
    [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
    public interface ICommandBaseSpec
    {
        string Title { get; }
        string Tooltip { get; }
        CommandGroupIcon Icon { get; }
    }
}
