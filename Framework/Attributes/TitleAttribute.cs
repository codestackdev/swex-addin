//**********************
//Development tools for SOLIDWORKS add-ins
//Copyright(C) 2018 www.codestack.net
//License: https://github.com/codestack-net-dev/sw-dev-tools-addin/blob/master/LICENSE
//Product URL: https://www.codestack.net/labs/solidworks/dev-tools-addin/
//**********************

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeStack.Dev.Sw.AddIn.Attributes
{
    [AttributeUsage(AttributeTargets.All)]
    public class TitleAttribute : DisplayNameAttribute
    {
        public TitleAttribute(string dispName) : base(dispName)
        {
        }
    }
}
