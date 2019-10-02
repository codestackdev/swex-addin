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
    /// <summary>
    /// Custom Property modification data
    /// </summary>
    public class CustomPropertyModifyData
    {
        /// <summary>
        /// Type of the modification
        /// </summary>
        public CustomPropertyChangeAction_e Action { get; private set; }

        /// <summary>
        /// Name of the custom property
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Configuration of custom property. Empty string for the file specific (generic) custom property
        /// </summary>
        public string Configuration { get; private set; }

        /// <summary>
        /// Value of the custom property
        /// </summary>
        public string Value { get; private set; }

        internal CustomPropertyModifyData(CustomPropertyChangeAction_e type, string name, string conf, string val)
        {
            Action = type;
            Name = name;
            Configuration = conf;
            Value = val;
        }
    }

    /// <summary>
    /// Delegate of <see cref="DocumentHandler.CustomPropertyModify"/> event
    /// </summary>
    /// <param name="docHandler">Document Handler which sends this notification</param>
    /// <param name="modifications">Array of all modifications in custom properties</param>
    public delegate void CustomPropertyModifyDelegate(DocumentHandler docHandler, CustomPropertyModifyData[] modifications);
}
