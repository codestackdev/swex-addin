using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace CodeStack.SwEx.AddIn.Base
{
    [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
    public interface ICommandGroupSpec : ICommandBaseSpec
    {
        int Id { get; }
        ICommandSpec[] Commands { get; }
    }
}
