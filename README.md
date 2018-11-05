[![Documentation](https://img.shields.io/badge/-Documentation-green.svg)](https://www.codestack.net/labs/solidworks/swex/add-in/)
[![NuGet](https://img.shields.io/nuget/v/CodeStack.SwEx.AddIn.svg)](https://www.nuget.org/packages/CodeStack.SwEx.AddIn/)
[![Issues](https://img.shields.io/github/issues/codestack-net-dev/sw-dev-tools-addin.svg)](https://github.com/codestack-net-dev/sw-dev-tools-addin/issues)

# SwEx.AddIn
SwEx.AddIn enables SOLIDWORKS add-in developers to develop robust applications using SOLIDWORKS API significantly simplifying the add-in creation process

## Getting started

* Create public com-visible class which inherits [SwAddInEx](https://docs.codestack.net/swex/add-in/html/T_CodeStack_SwEx_AddIn_SwAddInEx.htm) class
* Add-in registration process can be simplified by adding the [AutoRegister](https://docs.codestack.net/swex/add-in/html/T_CodeStack_SwEx_AddIn_Attributes_AutoRegisterAttribute.htm) attribute to add-in class. Check *Register for COM Interop* option in the project settings

~~~ cs
[AutoRegister("My C# SOLIDWORKS Add-In", "Sample SOLIDWORKS add-in in C#", true)]
[ComVisible(true)]
public class SwExportComponentAddIn : SwAddInEx
{
}
~~~

~~~ vb
<AutoRegister("My VB.NET SOLIDWORKS Add-In", "Sample SOLIDWORKS add-in in VB.NET", True)>
<ComVisible(True)>
Public Class SwExportComponentAddIn
    Inherits SwAddInEx
End Class
~~~

* Overload [OnConnect](https://docs.codestack.net/swex/add-in/html/M_CodeStack_SwEx_AddIn_SwAddInEx_OnConnect.htm) method to initiate the add-in. Access the pointer to the SLDWORKS application via [m_App](https://docs.codestack.net/swex/add-in/html/F_CodeStack_SwEx_AddIn_SwAddInEx_m_App.htm) field.

## Adding commands

![Command manager](https://www.codestack.net/labs/solidworks/swex/add-in/commands-manager/adding-command-group/commands-menu.png)

Framework allows to create commands and their handlers by simply defining them in the enumeration and providing the handler function via [AddCommandGroup](https://docs.codestack.net/swex/add-in/html/M_CodeStack_SwEx_AddIn_SwAddInEx_AddCommandGroup__1.htm) method.

Commands can be decorated with the attributes to provide their title and icon. Framework will automatically generate the icons in correct sizes and formats.