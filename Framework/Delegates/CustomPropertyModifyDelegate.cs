//**********************
//SwEx.AddIn - development tools for SOLIDWORKS add-ins
//Copyright(C) 2019 www.codestack.net
//License: https://github.com/codestackdev/swex-addin/blob/master/LICENSE
//Product URL: https://www.codestack.net/labs/solidworks/swex/add-in/
//**********************

using CodeStack.SwEx.AddIn.Core;
using CodeStack.SwEx.AddIn.Enums;

namespace CodeStack.SwEx.AddIn.Delegates
{
    public class CustomPropertyModifyData
    {
        public CustomPropertyChangeAction_e Type { get; private set; }
        public string Name { get; private set; }
        public string Configuration { get; private set; }
        public string Value { get; private set; }

        internal CustomPropertyModifyData(CustomPropertyChangeAction_e type, string name, string conf, string val)
        {
            Type = type;
            Name = name;
            Configuration = conf;
            Value = val;
        }
    }

    public delegate void CustomPropertyModifyDelegate(DocumentHandler docHandler, CustomPropertyModifyData[] modifications);
}
