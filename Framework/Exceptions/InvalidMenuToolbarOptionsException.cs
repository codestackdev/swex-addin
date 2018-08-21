using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CodeStack.Dev.Sw.AddIn.Exceptions
{
    /// <summary>
    /// Indicates that the command doesn't have either menu or toolbar option set
    /// </summary>
    public class InvalidMenuToolbarOptionsException : InvalidOperationException
    {
        internal InvalidMenuToolbarOptionsException(Enum cmd) 
            : base($"Neither toolbar nor menu option is specified for {cmd} command. Use")
        {
        }
    }
}
