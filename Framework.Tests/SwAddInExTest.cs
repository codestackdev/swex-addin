using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Protected;
using CodeStack.SwEx.AddIn;
using SolidWorks.Interop.sldworks;
using System.Collections.Generic;
using CodeStack.SwEx.AddIn.Attributes;
using SolidWorks.Interop.swconst;
using CodeStack.SwEx.Common.Attributes;

namespace Framework.Tests
{
    [TestClass]
    public class SwAddInExTest
    {
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
    }
}
