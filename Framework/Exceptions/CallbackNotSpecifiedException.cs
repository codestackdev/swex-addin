//**********************
//SwEx.AddIn - development tools for SOLIDWORKS add-ins
//Copyright(C) 2019 www.codestack.net
//License: https://github.com/codestackdev/swex-addin/blob/master/LICENSE
//Product URL: https://www.codestack.net/labs/solidworks/swex/add-in/
//**********************

using System;

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
