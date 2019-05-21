//**********************
//SwEx.AddIn - development tools for SOLIDWORKS add-ins
//Copyright(C) 2019 www.codestack.net
//License: https://github.com/codestack-net-dev/sw-dev-tools-addin/blob/master/LICENSE
//Product URL: https://www.codestack.net/labs/solidworks/swex/add-in/
//**********************

using CodeStack.SwEx.AddIn.Base;
using System;

namespace CodeStack.SwEx.AddIn.Exceptions
{
    /// <summary>
    /// Indicates that the command doesn't have either menu or toolbar option set
    /// </summary>
    public class InvalidMenuToolbarOptionsException : InvalidOperationException
    {
        internal InvalidMenuToolbarOptionsException(ICommandSpec cmd) 
            : base($"Neither toolbar nor menu option is specified for {cmd.Title} ({cmd.UserId}) command. Use")
        {
        }
    }
}
