//**********************
//SwEx.AddIn - development tools for SOLIDWORKS add-ins
//Copyright(C) 2018 www.codestack.net
//License: https://github.com/codestack-net-dev/sw-dev-tools-addin/blob/master/LICENSE
//Product URL: https://www.codestack.net/labs/solidworks/swex/add-in/
//**********************

using SolidWorksTools;
using System;
using System.ComponentModel;

namespace CodeStack.SwEx.AddIn.Attributes
{
    /// <summary>
    /// Automatically adds the information about the add-in into the registry
    /// </summary>
    /// <remarks>The registration is triggered when the add-in is registered as COM assembly using the regasm utility.
    /// If <see cref="Title"/> or <see cref="Description"/> are not specified (empty string) than
    /// title and description will be read from <see cref="SwAddinAttribute"/>. If this attribute is not 
    /// specified than title will be assigned from the <see cref="DisplayNameAttribute"/> and description will be assined from
    /// <see cref="DescriptionAttribute"/>
    /// </remarks>
    /// <example>
    /// <code language="c#" title="Add-in title specified via AutoRegisterAttribute">
    /// [Guid("GUID"), ComVisible(true)]
    /// [AutoRegister("Sample AddInEx", "Sample AddInEx", true)]
    /// public class SwSampleAddIn : SwAddInEx { }
    /// </code>
    /// <code language="c#" title="Add-in title specified via SwAddinAttribute">
    /// [Guid("GUID"), ComVisible(true)]
    /// [SwAddin(Title = "Sample AddInEx", Description = "Sample AddInEx", LoadAtStartup = true)]
    /// [AutoRegister]
    /// public class SwSampleAddIn : SwAddInEx { }
    /// </code>
    /// <code language="c#" title="Add-in title specified via DisplayNameAttribute and DescriptionAttribute">
    /// [Guid("GUID"), ComVisible(true)]
    /// [DisplayName("Sample AddInEx")]
    /// [Description("Sample AddInEx")]
    /// [AutoRegister]
    /// public class SwSampleAddIn : SwAddInEx { }
    /// </code>
    /// </example>
    [AttributeUsage(AttributeTargets.Class)]
    public class AutoRegisterAttribute : Attribute
    {
        /// <summary>
        /// Title of the add-in.
        /// </summary>
        public string Title { get; private set; }

        /// <summary>
        /// Description of the add-in
        /// </summary>
        public string Description { get; private set; }

        /// <summary>
        /// Indicates if the add-in should be loaded at startup
        /// </summary>
        public bool LoadAtStartup { get; private set; }

        public AutoRegisterAttribute(string title = "", string desc = "", bool loadAtStartup = true)
        {
            Title = title;
            Description = desc;
            LoadAtStartup = loadAtStartup;
        }
    }
}
