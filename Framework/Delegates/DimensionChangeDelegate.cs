//**********************
//SwEx.AddIn - development tools for SOLIDWORKS add-ins
//Copyright(C) 2019 www.codestack.net
//License: https://github.com/codestackdev/swex-addin/blob/master/LICENSE
//Product URL: https://www.codestack.net/labs/solidworks/swex/add-in/
//**********************

using CodeStack.SwEx.AddIn.Core;
using CodeStack.SwEx.AddIn.Enums;
using SolidWorks.Interop.sldworks;

namespace CodeStack.SwEx.AddIn.Delegates
{
    /// <summary>
    /// Delegate of <see cref="DocumentHandler.DimensionChange"/> event
    /// </summary>
    /// <param name="docHandler">Document Handler which sends this notification</param>
    /// <param name="dispDim">Pointer to the changed <see href="http://help.solidworks.com/2012/english/api/sldworksapi/solidworks.interop.sldworks~solidworks.interop.sldworks.idisplaydimension_properties.html">display dimension</see></param>
    public delegate void DimensionChangeDelegate(DocumentHandler docHandler, IDisplayDimension dispDim);
}
