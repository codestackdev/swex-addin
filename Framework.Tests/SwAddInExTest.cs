using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Protected;
using CodeStack.SwEx.AddIn;
using SolidWorks.Interop.sldworks;
using System.Collections.Generic;
using CodeStack.SwEx.AddIn.Attributes;
using SolidWorks.Interop.swconst;

namespace Framework.Tests
{
    [TestClass]
    public class SwAddInExTest
    {
        #region Mocks

        public enum CommandsMock_1
        {
            Cmd1,
            Cmd2
        }

        public enum CommandsMock_2
        {
            [Title("Command1")]
            [System.ComponentModel.Description("Command1 Desc")]
            [CommandItemInfo(false, true, CodeStack.SwEx.AddIn.Enums.swWorkspaceTypes_e.Assembly)]
            Cmd1,
        }

        #endregion

        [TestMethod]
        public void TestConnectToSW()
        {
            var addInExMock1 = new Mock<SwAddInEx>();
            
            bool connectCalled = false;
            addInExMock1.Setup(a => a.OnConnect()).Callback(
                () =>
                {
                    connectCalled = true;
                }).Returns(true);
            
            var addInExMock2 = new Mock<SwAddInEx>();
            addInExMock2.Setup(a => a.OnConnect()).Returns(false);

            var addInExMock3 = new Mock<SwAddInEx>() { CallBase = true };

            var res1 = addInExMock1.Object.ConnectToSW(new Mock<ISldWorks>().Object, 0);
            var res2 = addInExMock2.Object.ConnectToSW(new Mock<ISldWorks>().Object, 0);
            var res3 = addInExMock3.Object.ConnectToSW(new Mock<ISldWorks>().Object, 0);

            Assert.IsTrue(connectCalled);
            Assert.IsTrue(res1);
            Assert.IsFalse(res2);
            Assert.IsTrue(res3);
        }
        
        [TestMethod]
        public void DisconnectFromSWTest()
        {
            var addInExMock1 = new Mock<SwAddInEx>();
            bool disconnectCalled = false;
            addInExMock1.Setup(a => a.OnDisconnect()).Callback(
                () =>
                {
                    disconnectCalled = true;
                }).Returns(true);

            var addInExMock2 = new Mock<SwAddInEx>();
            addInExMock2.Setup(a => a.OnDisconnect()).Returns(false);

            var swMock = new Mock<ISldWorks>();
            swMock.Setup(m => m.GetCommandManager(It.IsAny<int>()))
                .Returns(new Mock<CommandManager>().Object);

            var addInExMock3 = new Mock<SwAddInEx>() { CallBase = true };

            addInExMock1.Object.ConnectToSW(swMock.Object, 0);
            addInExMock2.Object.ConnectToSW(swMock.Object, 0);
            addInExMock3.Object.ConnectToSW(swMock.Object, 0);

            var res1 = addInExMock1.Object.DisconnectFromSW();
            var res2 = addInExMock2.Object.DisconnectFromSW();
            var res3 = addInExMock3.Object.DisconnectFromSW();

            Assert.IsTrue(disconnectCalled);
            Assert.IsTrue(res1);
            Assert.IsFalse(res2);
            Assert.IsTrue(res3);
        }

        [TestMethod]
        public void AddCommandGroupTest()
        {
            var createMockObjectFunc = new Func<string, Dictionary<CommandGroup, List<object[]>>, SwAddInEx>(
                (rev, grps) => 
                {
                    var addInExMock = new Mock<SwAddInEx>();

                    var createCommandGroupMockObjectFunc = new Func<CommandGroup>(() =>
                    {
                        var cmdGroupMock = new Mock<CommandGroup>().SetupAllProperties();
                        var cmds = new List<object[]>();
                        grps.Add(cmdGroupMock.Object, cmds);
                        cmdGroupMock.Setup(m => m.AddCommandItem2(
                            It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(),
                            It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(),
                            It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
                            .Callback<string, int, string, string, int, string, string, int, int>(
                            (name, pos, hint, tooltip, imgList, callback, enable, userId, menuTbOpts) =>
                            {
                                cmds.Add(new object[] { name, pos, hint, tooltip, imgList, callback, enable, userId, menuTbOpts });
                            }).Returns(cmds.Count);

                        return cmdGroupMock.Object;
                    });

                    var cmdMgrMock = new Mock<CommandManager>();
                    var cmdGrpRes = (int)swCreateCommandGroupErrors.swCreateCommandGroup_Success;
                    cmdMgrMock.Setup(m => m.CreateCommandGroup2(
                        It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(),
                        It.IsAny<string>(), It.IsAny<int>(), It.IsAny<bool>(), ref cmdGrpRes))
                        .Returns(createCommandGroupMockObjectFunc.Invoke());
                    
                    var swMock = new Mock<ISldWorks>();
                    swMock.Setup(m => m.GetCommandManager(It.IsAny<int>()))
                        .Returns(cmdMgrMock.Object);
                    swMock.Setup(m => m.RevisionNumber()).Returns(rev);

                    addInExMock.Object.ConnectToSW(swMock.Object, 0);

                    return addInExMock.Object;
                });
            
            var cmds1 = new Dictionary<CommandGroup, List<object[]>>();
            var addInMock1 = createMockObjectFunc.Invoke("23.0.0", cmds1);
            var grp1 = addInMock1.AddCommandGroup<CommandsMock_1>(c => { });

            var cmds2 = new Dictionary<CommandGroup, List<object[]>>();
            var addInMock2 = createMockObjectFunc.Invoke("24.0.0", cmds2);
            var grp2 = addInMock2.AddCommandGroup<CommandsMock_1>(c => { });

            var cmds3 = new Dictionary<CommandGroup, List<object[]>>();
            var addInMock3 = createMockObjectFunc.Invoke("25.0.0", cmds3);
            var grp3 = addInMock3.AddCommandGroup<CommandsMock_2>(c => { });

            Assert.IsFalse(string.IsNullOrEmpty(grp1.LargeMainIcon));
            Assert.IsFalse(string.IsNullOrEmpty(grp1.SmallMainIcon));
            Assert.IsFalse(string.IsNullOrEmpty(grp1.LargeIconList));
            Assert.IsFalse(string.IsNullOrEmpty(grp1.SmallIconList));
            Assert.IsNull(grp1.MainIconList);
            Assert.IsNull(grp1.IconList);

            Assert.IsTrue(string.IsNullOrEmpty(grp2.LargeMainIcon));
            Assert.IsTrue(string.IsNullOrEmpty(grp2.SmallMainIcon));
            Assert.IsTrue(string.IsNullOrEmpty(grp2.LargeIconList));
            Assert.IsTrue(string.IsNullOrEmpty(grp2.SmallIconList));
            Assert.AreEqual(6, (grp2.MainIconList as string[]).Length);
            Assert.AreEqual(6, (grp2.IconList as string[]).Length);

            Assert.AreEqual(2, cmds2[grp2].Count);

            Assert.AreEqual(2, cmds1[grp1].Count);
            Assert.AreEqual("Cmd1", cmds1[grp1][0][0]);
            Assert.AreEqual("Cmd1", cmds1[grp1][0][2]);
            Assert.AreEqual("Cmd2", cmds1[grp1][1][0]);
            Assert.AreEqual("Cmd2", cmds1[grp1][1][2]);

            Assert.AreEqual(1, cmds3[grp3].Count);
            Assert.AreEqual("Command1", cmds3[grp3][0][0]);
            Assert.AreEqual("Command1 Desc", cmds3[grp3][0][2]);
            Assert.AreEqual(2, cmds3[grp3][0][8]);
        }
    }
}
