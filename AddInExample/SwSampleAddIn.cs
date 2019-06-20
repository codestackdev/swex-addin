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
        [CommandItemInfo(true, true, swWorkspaceTypes_e.AllDocuments, true)]
        Command1,

        [CommandSpacer]
        [Title("Command Two")]
        [Description("Sample Command2")]
        [CommandIcon(typeof(Resources), nameof(Resources.command2_icon), nameof(Resources.command2_icon))]
        [CommandItemInfo(true, true, swWorkspaceTypes_e.All, true)]
        Command2,

        Command3
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

    public class SimpleDocHandler : DocumentHandler
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
            ShowMessage($"{Model.GetTitle()} document loaded");
        }

        public override void OnDestroy()
        {
            ShowMessage($"{Model.GetTitle()} document destroyed");
        }

        public override void OnSaveToStream()
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

        public override void OnLoadFromStream()
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

        public override void OnLoadFromStorageStore()
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

        public override void OnSaveToStorageStore()
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
        private IDocumentsHandler<SimpleDocHandler> m_DocsHandler;

        public override bool OnConnect()
        {
            AddCommandGroup<Commands_e>(OnCommandClick, OnCommandEnable);
            AddCommandGroup<SubCommands_e>(OnSubCommandClick);

            m_DocsHandler = CreateDocumentsHandler<SimpleDocHandler>();

            TaskPaneControl ctrl;
            var taskPaneView = CreateTaskPane<TaskPaneControl, TaskPaneCommands_e>(OnTaskPaneCommandClick, out ctrl);

            return true;
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
