# SwEx.AddIn
SwEx.AddIn enables SOLIDWORKS add-in developers to develop robust applications using SOLIDWORKS API significantly simplifying the add-in creation process

## Links
[NuGet](https://www.nuget.org/packages/CodeStack.SwEx.AddIn)
[Documentation](https://www.codestack.net/labs/solidworks/swex/add-in/)

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

* Add command manager with commands and handlers via [AddCommandGroup](https://docs.codestack.net/swex/add-in/html/M_CodeStack_SwEx_AddIn_SwAddInEx_AddCommandGroup__1.htm) method by defining the enumeration of commands.