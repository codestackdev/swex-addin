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
