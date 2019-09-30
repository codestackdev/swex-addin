//**********************
//SwEx.AddIn - development tools for SOLIDWORKS add-ins
//Copyright(C) 2019 www.codestack.net
//License: https://github.com/codestackdev/swex-addin/blob/master/LICENSE
//Product URL: https://www.codestack.net/labs/solidworks/swex/add-in/
//**********************

using SolidWorks.Interop.sldworks;

namespace CodeStack.SwEx.AddIn.Enums
{
    public enum Access3rdPartyDataAction_e
    {
        /// <summary>
        /// Read the data from the third party storage via <see cref="ModelDocExtension.Access3rdPartyStorageStore(IModelDoc2, string, bool)"/> method
        /// </summary>
        StorageRead,

        /// <summary>
        /// Save the data from the third party storage via <see cref="ModelDocExtension.Access3rdPartyStorageStore(IModelDoc2, string, bool)"/> method
        /// </summary>
        StorageWrite,

        /// <summary>
        /// Read the data from the 3rd party stream via <see cref="ModelDocExtension.Access3rdPartyStream(IModelDoc2, string, bool)"/>
        /// </summary>
        StreamRead,

        /// <summary>
        /// Save the data from the 3rd party stream via <see cref="ModelDocExtension.Access3rdPartyStream(IModelDoc2, string, bool)"/>
        /// </summary>
        StreamWrite
    }
}