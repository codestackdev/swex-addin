using CodeStack.SwEx.AddIn;
using CodeStack.SwEx.AddIn.Base;
using CodeStack.SwEx.AddIn.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Framework.Tests
{
    [TestClass]
    public class DocumentsHandlerTest
    {
        #region Mocks

        public class DocumentHandlerMock : IDocumentHandler
        {
            public static event Action<IModelDoc2> Initialized;
            public static event Action<IModelDoc2> Disposed;

            public IModelDoc2 Model { get; private set; }

            public void Dispose()
            {
                Disposed?.Invoke(Model);
            }

            public void Init(ISldWorks app, IModelDoc2 model)
            {
                Model = model;
                Initialized?.Invoke(Model);
            }
        }

        #endregion

        [TestMethod]
        public void HandlerCreatedEventRaisedTest()
        {
            var addInExMock = new Mock<SwAddInEx>();
            var addInEx = addInExMock.Object;

            var doc1Mock = new Mock<IModelDoc2>();
            var doc2Mock = new Mock<IModelDoc2>();

            doc2Mock.Setup(d => d.GetTitle()).Returns("doctitle");
            
            var swMock = new Mock<SldWorks>();
            swMock.Setup(s => s.GetOpenDocumentByName(It.IsAny<string>()))
                .Returns((string n) =>
                {
                    if (n == "docpath")
                    {
                        return doc1Mock.Object;
                    }
                    else
                    {
                        return null;
                    }
                });
            
            addInEx.ConnectToSW(swMock.Object, 0);

            var docsHandler = addInEx.CreateDocumentsHandler<DocumentHandlerMock>();

            var res = new List<IModelDoc2>();
            
            docsHandler.HandlerCreated += h =>
            {
                res.Add(h.Model);
            };

            swMock.Setup(s => s.GetDocuments()).Returns(new IModelDoc2[] { doc1Mock.Object, doc2Mock.Object });

            swMock.Raise(s => s.DocumentLoadNotify2 += null, "", "docpath");
            swMock.Raise(s => s.DocumentLoadNotify2 += null, "doctitle", "");

            Assert.AreEqual(2, res.Count);
            Assert.AreEqual(doc1Mock.Object, res[0]);
            Assert.AreEqual(doc2Mock.Object, res[1]);
        }
        
        [TestMethod]
        public void DocumentHandlerLifecycleTest()
        {
            var addInExMock = new Mock<SwAddInEx>();
            var addInEx = addInExMock.Object;

            var doc1Mock = new Mock<IModelDoc2>();
            doc1Mock.As<PartDoc>();

            var swMock = new Mock<SldWorks>();

            swMock.Setup(s => s.GetDocuments()).Returns(new IModelDoc2[] { doc1Mock.Object });

            addInEx.ConnectToSW(swMock.Object, 0);

            IModelDoc2 initModel = null;
            IModelDoc2 disposedModel = null;
            DocumentHandlerMock.Initialized += d => initModel = d;
            DocumentHandlerMock.Disposed += d => disposedModel = d;

            var docsHandler = addInEx.CreateDocumentsHandler<DocumentHandlerMock>();

            var initDocHandler = docsHandler[doc1Mock.Object as IModelDoc2];
            
            doc1Mock.Raise(d => (d as PartDoc).DestroyNotify2 += null, (int)swDestroyNotifyType_e.swDestroyNotifyDestroy);

            Assert.IsNotNull(initModel);
            Assert.IsNotNull(disposedModel);
            Assert.AreEqual(doc1Mock.Object, initModel);
            Assert.AreEqual(disposedModel, initModel);
            Assert.IsNotNull(initDocHandler);
            Assert.AreEqual(doc1Mock.Object, initDocHandler.Model);
            AssertThrows<KeyNotFoundException>(() => { var d = docsHandler[doc1Mock.Object as IModelDoc2]; });
        }

        private static void AssertThrows<T>(Action func) where T : Exception
        {
            var isThrown = false;

            try
            {
                func.Invoke();
            }
            catch (T)
            {
                isThrown = true;
            }

            if (!isThrown)
            {
                throw new AssertFailedException(
                    $"An exception of type {typeof(T).FullName} was expected, but not thrown");
            }
        }
    }
}
