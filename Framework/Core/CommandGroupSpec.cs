using CodeStack.SwEx.AddIn.Base;
using CodeStack.SwEx.AddIn.Icons;
using CodeStack.SwEx.Common.Icons;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace CodeStack.SwEx.AddIn.Core
{
    [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
    public class CommandGroupSpec : ICommandGroupSpec
    {
        public string Title { get; protected set; }
        public string Tooltip { get; protected set; }
        public CommandGroupIcon Icon { get; protected set; }

        public int Id { get; protected set; }
        public ICommandSpec[] Commands { get; protected set; }
    }
}
