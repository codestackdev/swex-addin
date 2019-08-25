[![Change Log](https://img.shields.io/badge/-Changelog-blue.svg)](https://docs.codestack.net/swex/add-in/html/version-history.htm)
[![Documentation](https://img.shields.io/badge/-Documentation-green.svg)](https://www.codestack.net/labs/solidworks/swex/add-in/)
[![NuGet](https://img.shields.io/nuget/v/CodeStack.SwEx.AddIn.svg)](https://www.nuget.org/packages/CodeStack.SwEx.AddIn/)
[![Issues](https://img.shields.io/github/issues/codestackdev/swex-addin.svg)](https://github.com/codestackdev/swex-addin/issues)

~~~
When updating nuget package 0.7.0 or newer check the Changelog for the list of members which were remove and their replacements.
~~~

# SwEx.AddIn
![SwEx.AddIn](https://www.codestack.net/labs/solidworks/swex/add-in/logo.png)
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

* Overload [OnConnect](https://docs.codestack.net/swex/add-in/html/M_CodeStack_SwEx_AddIn_SwAddInEx_OnConnect.htm) method to initiate the add-in. Access the pointer to the SLDWORKS application via [App](https://docs.codestack.net/swex/add-in/html/P_CodeStack_SwEx_AddIn_SwAddInEx_App.htm) field.

## Adding commands

![Command manager](https://www.codestack.net/labs/solidworks/swex/add-in/commands-manager/adding-command-group/commands-menu.png)

Framework allows to create commands and their handlers by simply defining them in the enumeration and providing the handler function via [AddCommandGroup](https://docs.codestack.net/swex/add-in/html/M_CodeStack_SwEx_AddIn_SwAddInEx_AddCommandGroup__1.htm) method.

Commands can be decorated with the attributes to provide their title and icon. Framework will automatically generate the icons in correct sizes and formats.

~~~ cs
public enum Commands_e
{
    CreateCylinder
}
...
this.AddCommandGroup<Commands_e>(OnButtonClick);
...
private void OnButtonClick(Commands_e cmd)
{
    switch (cmd)
    {
        case Commands_e.CreateCylinder:
            //TODO: do something
            break;
    }
}
~~~

## Managing Documents

Utility allows to handle documents lifecycle by providing the document handler type.

~~~ cs
private IDocumentsHandler<MyDocHandler> m_DocHandler;
...
m_DocHandler = CreateDocumentsHandler<MyDocHandler>();
...
public class MyDocHandler : DocumentHandler
{
    public override void OnInit()
    {
        //TODO: init
    }

    public override void OnDestroy()
    {
        //TODO: release
    }
}
~~~

## Accessing 3rd Party Storage Store And Stream

Utility to read and write data to 3rd party storage store and stream

~~~ cs
public override void OnSaveToStream()
{
    using (var streamHandler = Model.Access3rdPartyStream(STREAM_NAME, true))
    {
        using (var str = streamHandler.Stream)
        {
            var xmlSer = new XmlSerializer(typeof(RevData));

            xmlSer.Serialize(str, m_RevData);
        }
    }
}

public override void OnLoadFromStream()
{
    using (var streamHandler = Model.Access3rdPartyStream(STREAM_NAME, false))
    {
        if (streamHandler.Stream != null)
        {
            using (var str = streamHandler.Stream)
            {
                var xmlSer = new XmlSerializer(typeof(RevData));
                var data = xmlSer.Deserialize(str) as RevData;
            }
        }
    }
}

public override void OnLoadFromStorageStore()
{
    using (var storageHandler = Model.Access3rdPartyStorageStore(STORAGE_NAME, false))
    {
        if (storageHandler.Storage != null)
        {
            using (var str = storageHandler.Storage.TryOpenStream(STREAM_NAME, false))
            {
                var xmlSer = new XmlSerializer(typeof(RevData));
                var data = xmlSer.Deserialize(str) as RevData;
            }
        }
    }
}

public override void OnSaveToStorageStore()
{
    using (var storageHandler = Model.Access3rdPartyStorageStore(STORAGE_NAME, true))
    {
        using (var subStorage = storageHandler.Storage.TryOpenStorage(SUB_STORAGE_NAME, true))
        {
            using (var str = subStorage.TryOpenStreamSTREAM_NAME, true))
            {
                var buffer = Encoding.UTF8.GetBytes(DateTime.Now.ToString("yyyy-MM-dd-hh-mm-ss"));
                str.Write(buffer, 0, buffer.Length);
            }
        }
    }
}
~~~

## Creating Hosted Controls

### Task Pane

Create task pane and host User Control

~~~ cs
public enum TaskPaneCommands_e
{
    Command1
}

...
TaskPaneControl ctrl;
var taskPaneView = CreateTaskPane<TaskPaneControl, TaskPaneCommands_e>(OnTaskPaneCommandClick, out ctrl);
...

private void OnTaskPaneCommandClick(TaskPaneCommands_e cmd)
{
    switch (cmd)
    {
        case TaskPaneCommands_e.Command1:
            //TODO: handle command
            break;
    }
}
~~~