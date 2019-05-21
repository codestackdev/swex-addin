//**********************
//SwEx.AddIn - development tools for SOLIDWORKS add-ins
//Copyright(C) 2019 www.codestack.net
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
        internal string Title { get; private set; }
        internal string Description { get; private set; }
        internal bool LoadAtStartup { get; private set; }

        /// <summary>
        /// Constructor for adding the parameters for add-in registration
        /// </summary>
        /// <param name="title">Title of the add-in</param>
        /// <param name="desc">Description of the add-in</param>
        /// <param name="loadAtStartup">Indicates if the add-in should be loaded at startup</param>
        public AutoRegisterAttribute(string title = "", string desc = "", bool loadAtStartup = true)
        {
            Title = title;
            Description = desc;
            LoadAtStartup = loadAtStartup;
        }
    }
}
