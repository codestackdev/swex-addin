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

namespace CodeStack.SwEx.AddIn.Example
{
    [Common.Attributes.Title("AddInEx Commands")]
    [Description("Sample commands")]
    [Common.Attributes.Icon(typeof(Resources), nameof(Resources.command_group_icon))]
    [CommandGroupInfo(0)]
    public enum Commands_e
    {
        [Common.Attributes.Title("Command1")]
        [Description("Sample Command 1")]
        [Common.Attributes.Icon(typeof(Resources), nameof(Resources.command1_icon))]
        [CommandItemInfo(true, true, swWorkspaceTypes_e.AllDocuments, true)]
        Command1,

        [Common.Attributes.Title("Command 2")]
        [Description("Sample Command2")]
        [CommandIcon(typeof(Resources), nameof(Resources.command2_icon), nameof(Resources.command2_icon))]
        [CommandItemInfo(true, true, swWorkspaceTypes_e.All, true)]
        Command2,

        Command3,
    }

    public class SimpleDocHandler : DocumentHandler
    {
        public class RevData
        {
            public int Revision { get; set; }
            public Guid RevisionStamp { get; set; }
        }

        private const string STREAM_NAME = "_CodeStackStream_";
        private const string SUB_STORAGE_STREAM = "_CodeStackStorage1_\\SubStorage2\\Stream1";

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
            using (var streamHandler = Model.Access3rdPartyStream(SUB_STORAGE_STREAM, false))
            {
                if (streamHandler.Stream != null)
                {
                    using (var str = streamHandler.Stream)
                    {
                        var buffer = new byte[str.Length];

                        str.Read(buffer, 0, buffer.Length);

                        var timeStamp = Encoding.UTF8.GetString(buffer);

                        ShowMessage($"Timestamp of {Model.GetTitle()}: {timeStamp}");
                    }
                }
                else
                {
                    ShowMessage($"No timestamp in {Model.GetTitle()}");
                }
            }
        }

        public override void OnSaveToStorageStore()
        {
            using (var streamHandler = Model.Access3rdPartyStream(SUB_STORAGE_STREAM, true))
            {
                using (var str = streamHandler.Stream)
                {
                    var buffer = Encoding.UTF8.GetBytes(DateTime.Now.ToString("yyyy-MM-dd-hh-mm-ss"));
                    str.Write(buffer, 0, buffer.Length);
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
        private IDocumentsHandler m_DocsHandler;

        public override bool OnConnect()
        {
            AddCommandGroup<Commands_e>(OnCommandClick, OnCommandEnable);

            m_DocsHandler = CreateDocumentsHandler<SimpleDocHandler>();

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
