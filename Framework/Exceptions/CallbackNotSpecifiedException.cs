//**********************
//SwEx.AddIn - development tools for SOLIDWORKS add-ins
//Copyright(C) 2018 www.codestack.net
//License: https://github.com/codestack-net-dev/sw-dev-tools-addin/blob/master/LICENSE
//Product URL: https://www.codestack.net/labs/solidworks/dev-tools-addin/
//**********************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CodeStack.SwEx.AddIn.Exceptions
{
    /// <summary>
    /// Indicates that the callback is not specified for the commands group
    /// </summary>
    public class CallbackNotSpecifiedException : NullReferenceException
    {
        internal CallbackNotSpecifiedException() 
            : base("Callback must be specified")
        {
        }
    }
}
