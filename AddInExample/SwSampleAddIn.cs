//icons by: http://icons8.com

using CodeStack.SwEx.AddIn.Attributes;
using CodeStack.SwEx.AddIn.Core;
using CodeStack.SwEx.AddIn.Enums;
using CodeStack.SwEx.AddIn.Example.Properties;
using CodeStack.SwEx.AddIn.Helpers;
using SolidWorksTools;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using CodeStack.SwEx.AddIn.Base;
using System.Xml.Serialization;
using CodeStack.SwEx.Common.Attributes;
using CodeStack.SwEx.AddIn.Delegates;

namespace CodeStack.SwEx.AddIn.Example
{
    [Title("AddInEx Commands")]
    [Description("Sample commands")]
    [Icon(typeof(Resources), nameof(Resources.command_group_icon))]
    [CommandGroupInfo(0)]
    public enum Commands_e
    {
        [Title("Command One")]
        [Description("Sample Command 1")]
        [Icon(typeof(Resources), nameof(Resources.command1_icon))]
        Command1,

        [Title("Command Two")]
        [Description("Sample Command2")]
        [CommandIcon(typeof(Resources), nameof(Resources.command2_icon), nameof(Resources.command2_icon))]
        [CommandItemInfo(true, true, swWorkspaceTypes_e.All, true)]
        Command2,

        [CommandSpacer]
        [CommandItemInfo(true, true, swWorkspaceTypes_e.AllDocuments, true)]
        Command3,

        [CommandItemInfo(true, true, swWorkspaceTypes_e.AllDocuments, true)]
        Command4
    }

    [CommandGroupInfo(typeof(Commands_e))]
    [Title("Sub Menu Commands")]
    public enum SubCommands_e
    {
        [CommandItemInfo(true, true, swWorkspaceTypes_e.AllDocuments, true)]
        SubCommand1,

        [CommandItemInfo(true, true, swWorkspaceTypes_e.AllDocuments, true)]
        SubCommand2
    }

    public enum TaskPaneCommands_e
    {
        [Title("Task Pane Command 1")]
        [Icon(typeof(Resources), nameof(Resources.command1_icon))]
        Command1,

        Command2
    }

    public class DataStorageDocHandler : DocumentHandler
    {
        public class RevData
        {
            public int Revision { get; set; }
            public Guid RevisionStamp { get; set; }
        }

        private const string STREAM_NAME = "_CodeStackStream_";
        private const string SUB_STORAGE_PATH = "_CodeStackStorage1_\\SubStorage2";
        private const string TIME_STAMP_STREAM_NAME = "TimeStampStream";
        private const string USER_NAME_STREAM_NAME = "UserName";

        private RevData m_RevData;

        public override void OnInit()
        {
            this.Access3rdPartyData += OnAccess3rdPartyData;

            ShowMessage($"{Model.GetTitle()} document loaded");
        }

        private void OnAccess3rdPartyData(DocumentHandler docHandler, Access3rdPartyDataAction_e type)
        {
            switch (type)
            {
                case Access3rdPartyDataAction_e.StorageRead:
                    LoadFromStorageStore();
                    break;

                case Access3rdPartyDataAction_e.StorageWrite:
                    SaveToStorageStore();
                    break;

                case Access3rdPartyDataAction_e.StreamRead:
                    LoadFromStream();
                    break;

                case Access3rdPartyDataAction_e.StreamWrite:
                    SaveToStream();
                    break;

            }
        }

        public override void OnDestroy()
        {
            ShowMessage($"{Model.GetTitle()} document destroyed");
        }

        private void SaveToStream()
        {
            using (var streamHandler = Model.Access3rdPartyStream(STREAM_NAME, true))
            {
                using (var str = streamHandler.Stream)
                {
                    var xmlSer = new XmlSerializer(typeof(RevData));

                    if (m_RevData == null)
                    {
                        m_RevData = new RevData();
                    }

                    m_RevData.Revision = m_RevData.Revision + 1;
                    m_RevData.RevisionStamp = Guid.NewGuid();

                    xmlSer.Serialize(str, m_RevData);
                }
            }
        }

        private void LoadFromStream()
        {
            using (var streamHandler = Model.Access3rdPartyStream(STREAM_NAME, false))
            {
                if (streamHandler.Stream != null)
                {
                    using (var str = streamHandler.Stream)
                    {
                        var xmlSer = new XmlSerializer(typeof(RevData));
                        m_RevData = xmlSer.Deserialize(str) as RevData;
                        ShowMessage($"Revision data of {Model.GetTitle()}: {m_RevData.Revision} - {m_RevData.RevisionStamp}");
                    }
                }
                else
                {
                    ShowMessage($"No revision data stored in {Model.GetTitle()}");
                }
            }
        }

        private void LoadFromStorageStore()
        {
            var path = SUB_STORAGE_PATH.Split('\\');

            using (var storageHandler = Model.Access3rdPartyStorageStore(path[0], false))
            {
                if (storageHandler.Storage != null)
                {
                    using (var subStorage = storageHandler.Storage.TryOpenStorage(path[1], false))
                    {
                        foreach (var subStreamName in subStorage.GetSubStreamNames())
                        {
                            using (var str = subStorage.TryOpenStream(subStreamName, false))
                            {
                                if (str != null)
                                {
                                    var buffer = new byte[str.Length];

                                    str.Read(buffer, 0, buffer.Length);

                                    var timeStamp = Encoding.UTF8.GetString(buffer);

                                    ShowMessage($"Metadata stamp of {Model.GetTitle()}: {timeStamp}");
                                }
                                else
                                {
                                    ShowMessage($"No metadata stamp stream in {Model.GetTitle()}");
                                }
                            }
                        }
                    }
                }
                else
                {
                    ShowMessage($"No metadata storage in {Model.GetTitle()}");
                }
            }
        }

        private void SaveToStorageStore()
        {
            var path = SUB_STORAGE_PATH.Split('\\');

            using (var storageHandler = Model.Access3rdPartyStorageStore(path[0], true))
            {
                using (var subStorage = storageHandler.Storage.TryOpenStorage(path[1], true))
                {
                    using (var str = subStorage.TryOpenStream(TIME_STAMP_STREAM_NAME, true))
                    {
                        var buffer = Encoding.UTF8.GetBytes(DateTime.Now.ToString("yyyy-MM-dd-hh-mm-ss"));
                        str.Write(buffer, 0, buffer.Length);
                    }

                    using (var str = subStorage.TryOpenStream(USER_NAME_STREAM_NAME, true))
                    {
                        var buffer = Encoding.UTF8.GetBytes(System.Environment.UserName);
                        str.Write(buffer, 0, buffer.Length);
                    }
                }
            }
        }

        private void ShowMessage(string msg)
        {
            App.SendMsgToUser2(msg,
                (int)swMessageBoxIcon_e.swMbInformation, (int)swMessageBoxBtn_e.swMbOk);
        }
    }

    [Guid("86EA567D-79FA-4E3B-B66E-EAB660DB3D47"), ComVisible(true)]
    [AutoRegister("Sample AddInEx", "Sample AddInEx", true)]
    public class SwSampleAddIn : SwAddInEx
    {
        private IDocumentsHandler<DataStorageDocHandler> m_DocsHandler;
        private IDocumentsHandler<DocumentHandler> m_GenericDocsHandler;

        public override bool OnConnect()
        {
            AddCommandGroup<Commands_e>(OnCommandClick, OnCommandEnable);
            AddCommandGroup<SubCommands_e>(OnSubCommandClick);

            m_DocsHandler = CreateDocumentsHandler<DataStorageDocHandler>();

            m_GenericDocsHandler = CreateDocumentsHandler();
            m_GenericDocsHandler.HandlerCreated += OnHandlerCreated;
            TaskPaneControl ctrl;
            var taskPaneView = CreateTaskPane<TaskPaneControl, TaskPaneCommands_e>(OnTaskPaneCommandClick, out ctrl);

            return true;
        }

        private void OnHandlerCreated(DocumentHandler handler)
        {
            handler.Activated += OnActivated;
            handler.Initialized += OnInitialized;
            handler.ConfigurationChange += OnConfigurationChanged;
            handler.CustomPropertyModify += OnCustomPropertyModified;
            handler.ItemModify += OnItemModified;
            handler.Save += OnSave;
            handler.Selection += OnSelection;
            handler.Rebuild += OnRebuild;
            handler.Destroyed += OnDestroyed;
        }

        private bool OnRebuild(DocumentHandler docHandler, RebuildAction_e type)
        {
            return App.SendMsgToUser2($"'{docHandler.Model.GetTitle()}' rebuilt ({type}). Cancel?",
                        (int)swMessageBoxIcon_e.swMbQuestion, (int)swMessageBoxBtn_e.swMbYesNo) == (int)swMessageBoxResult_e.swMbHitNo;
        }

        private void OnInitialized(DocumentHandler docHandler)
        {
            App.SendMsgToUser2($"'{docHandler.Model.GetTitle()}' initialized",
                (int)swMessageBoxIcon_e.swMbInformation, (int)swMessageBoxBtn_e.swMbOk);
        }

        private void OnDestroyed(DocumentHandler handler)
        {
            handler.Activated -= OnActivated;
            handler.Initialized -= OnInitialized;
            handler.ConfigurationChange -= OnConfigurationChanged;
            handler.CustomPropertyModify -= OnCustomPropertyModified;
            handler.ItemModify -= OnItemModified;
            handler.Save -= OnSave;
            handler.Selection -= OnSelection;
            handler.Rebuild -= OnRebuild;
            handler.Destroyed -= OnDestroyed;

            App.SendMsgToUser2($"'{handler.Model.GetTitle()}' destroyed",
                (int)swMessageBoxIcon_e.swMbInformation, (int)swMessageBoxBtn_e.swMbOk);
        }

        private bool m_ShowSelectionEvents = false;

        private bool OnSelection(DocumentHandler docHandler, swSelectType_e selType, SelectionAction_e type)
        {
            if (m_ShowSelectionEvents)
            {
                if (type != SelectionAction_e.UserPreSelect)//dynamic selection
                {
                    return App.SendMsgToUser2($"'{docHandler.Model.GetTitle()}' selection ({type}) of {selType}. Cancel?",
                        (int)swMessageBoxIcon_e.swMbQuestion, (int)swMessageBoxBtn_e.swMbYesNo) == (int)swMessageBoxResult_e.swMbHitNo;
                }
                else
                {
                    return selType != swSelectType_e.swSelFACES;
                }
            }
            else
            {
                return true;
            }
        }

        private bool OnSave(DocumentHandler docHandler, string fileName, SaveAction_e type)
        {
            return App.SendMsgToUser2($"'{docHandler.Model.GetTitle()}' saving ({type}). Cancel?",
                (int)swMessageBoxIcon_e.swMbQuestion, (int)swMessageBoxBtn_e.swMbYesNo) == (int)swMessageBoxResult_e.swMbHitNo;
        }

        private void OnItemModified(DocumentHandler docHandler, ItemModificationAction_e type, swNotifyEntityType_e entType, string name, string oldName = "")
        {
            App.SendMsgToUser2($"'{docHandler.Model.GetTitle()}' item modified ({type}) of {entType}. Name: {name} (from {oldName}). Cancel?",
                (int)swMessageBoxIcon_e.swMbInformation, (int)swMessageBoxBtn_e.swMbOk);
        }

        private void OnCustomPropertyModified(DocumentHandler docHandler, CustomPropertyModifyData[] modifications)
        {
            foreach (var mod in modifications)
            {
                App.SendMsgToUser2($"'{docHandler.Model.GetTitle()}' custom property '{mod.Name}' changed ({mod.Type}) in '{mod.Configuration}' to '{mod.Value}'",
                    (int)swMessageBoxIcon_e.swMbInformation, (int)swMessageBoxBtn_e.swMbOk);
            }
        }

        private void OnConfigurationChanged(DocumentHandler docHandler, ConfigurationChangeAction_e type, string confName)
        {
            App.SendMsgToUser2($"'{docHandler.Model.GetTitle()}' configuration {confName} changed ({type}). Cancel?",
                (int)swMessageBoxIcon_e.swMbInformation, (int)swMessageBoxBtn_e.swMbOk);
        }

        private void OnActivated(DocumentHandler docHandler)
        {
            App.SendMsgToUser2($"'{docHandler.Model.GetTitle()}' activated",
                (int)swMessageBoxIcon_e.swMbInformation, (int)swMessageBoxBtn_e.swMbOk);
        }

        public override bool OnDisconnect()
        {
            return base.OnDisconnect();
        }

        private void OnCommandClick(Commands_e cmd)
        {
            switch (cmd)
            {
                case Commands_e.Command1:
                    App.SendMsgToUser("Command1 clicked!");
                    break;

                case Commands_e.Command2:
                    App.SendMsgToUser("Command2 clicked!");
                    break;

                case Commands_e.Command3:
                    App.SendMsgToUser("Command3 clicked!");
                    break;
            }
        }

        private void OnSubCommandClick(SubCommands_e cmd)
        {
            switch (cmd)
            {
                case SubCommands_e.SubCommand1:
                    App.SendMsgToUser("SubCommand1 clicked!");
                    break;

                case SubCommands_e.SubCommand2:
                    App.SendMsgToUser("SubCommand2 clicked!");
                    break;
            }
        }

        private void OnTaskPaneCommandClick(TaskPaneCommands_e cmd)
        {
            switch (cmd)
            {
                case TaskPaneCommands_e.Command1:
                    App.SendMsgToUser("TaskPane Command1 clicked!");
                    break;

                case TaskPaneCommands_e.Command2:
                    App.SendMsgToUser("TaskPane Command2 clicked!");
                    break;
            }
        }

        private void OnCommandEnable(Commands_e cmd, ref CommandItemEnableState_e state)
        {
            if (cmd == Commands_e.Command1)
            {
                if (state == CommandItemEnableState_e.DeselectEnable)
                {
                    if (App.IActiveDoc2?.ISelectionManager?.GetSelectedObjectCount2(-1) == 0)
                    {
                        state = CommandItemEnableState_e.DeselectDisable;
                    }
                }
            }
        }
    }
}
