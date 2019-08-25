//**********************
//SwEx.AddIn - development tools for SOLIDWORKS add-ins
//Copyright(C) 2019 www.codestack.net
//License: https://github.com/codestackdev/swex-addin/blob/master/LICENSE
//Product URL: https://www.codestack.net/labs/solidworks/swex/add-in/
//**********************

using CodeStack.SwEx.AddIn.Base;
using CodeStack.SwEx.AddIn.Icons;
using System.ComponentModel;

namespace CodeStack.SwEx.AddIn.Core
{
    [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
    public class CommandGroupSpec : ICommandGroupSpec
    {
        public ICommandGroupSpec Parent { get; protected set; }
        public string Title { get; protected set; }
        public string Tooltip { get; protected set; }
        public CommandGroupIcon Icon { get; protected set; }

        public int Id { get; protected set; }
        public ICommandSpec[] Commands { get; protected set; }
    }
}
