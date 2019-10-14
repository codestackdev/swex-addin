using CodeStack.SwEx.AddIn.Attributes;
using CodeStack.SwEx.AddIn.Helpers;
using CodeStack.SwEx.AddIn.Icons;
using CodeStack.SwEx.Common.Diagnostics;
using CodeStack.SwEx.Common.Icons;
using CodeStack.SwEx.Common.Reflection;
using SolidWorks.Interop.sldworks;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace CodeStack.SwEx.AddIn.Modules
{
    internal class TaskPaneModule : IDisposable
    {
        private readonly List<ITaskPaneHandler> m_TaskPanes;

        private readonly ISldWorks m_App;
        private readonly ILogger m_Logger;

        internal TaskPaneModule(ISldWorks app, ILogger logger)
        {
            m_App = app;
            m_Logger = logger;
            m_TaskPanes = new List<ITaskPaneHandler>();
        }

        internal ITaskpaneView CreateTaskPane<TControl, TCmdEnum>(Action<TCmdEnum> cmdHandler, out TControl ctrl)
            where TControl : UserControl, new()
            where TCmdEnum : IComparable, IFormattable, IConvertible
        {
            var tooltip = "";
            CommandGroupIcon taskPaneIcon = null;

            var getTaskPaneDisplayData = new Action<Type, bool>((t, d) =>
            {
                if (taskPaneIcon == null)
                {
                    taskPaneIcon = DisplayInfoExtractor.ExtractCommandDisplayIcon<TaskPaneIconAttribute, CommandGroupIcon>(
                        t, i => new TaskPaneMasterIcon(i), a => a.Icon, d);
                }

                if (string.IsNullOrEmpty(tooltip))
                {
                    if (!t.TryGetAttribute<DisplayNameAttribute>(a => tooltip = a.DisplayName))
                    {
                        t.TryGetAttribute<DescriptionAttribute>(a => tooltip = a.Description);
                    }
                }
            });

            if (typeof(TCmdEnum) != typeof(EmptyTaskPaneCommands_e))
            {
                getTaskPaneDisplayData.Invoke(typeof(TCmdEnum), false);
            }

            getTaskPaneDisplayData.Invoke(typeof(TControl), true);

            ITaskpaneView taskPaneView = null;
            ITaskPaneHandler taskPaneHandler = null;

            m_Logger.Log($"Creating task pane for {typeof(TControl).FullName} type");

            using (var iconConv = new IconsConverter())
            {
                if (m_App.SupportsHighResIcons(SldWorksExtension.HighResIconsScope_e.TaskPane))
                {
                    var taskPaneIconImages = iconConv.ConvertIcon(taskPaneIcon, true);
                    taskPaneView = m_App.CreateTaskpaneView3(taskPaneIconImages, tooltip);
                }
                else
                {
                    var taskPaneIconImage = iconConv.ConvertIcon(taskPaneIcon, false)[0];
                    taskPaneView = m_App.CreateTaskpaneView2(taskPaneIconImage, tooltip);
                }

                taskPaneHandler = new TaskPaneHandler<TCmdEnum>(m_App, taskPaneView, cmdHandler, iconConv, m_Logger);
            }

            if (typeof(TControl).IsComVisible())
            {
                var progId = typeof(TControl).GetProgId();
                ctrl = taskPaneView.AddControl(progId, "") as TControl;

                if (ctrl == null)
                {
                    throw new NullReferenceException(
                        $"Failed to create COM control from {progId}. Make sure that COM component is properly registered");
                }
            }
            else
            {
                ctrl = new TControl();
                ctrl.CreateControl();
                var handle = ctrl.Handle;

                if (!taskPaneView.DisplayWindowFromHandle(handle.ToInt32()))
                {
                    throw new NullReferenceException($"Failed to host .NET control (handle {handle}) in task pane");
                }
            }

            taskPaneHandler.Disposed += OnTaskPaneHandlerDisposed;
            m_TaskPanes.Add(taskPaneHandler);

            return taskPaneView;
        }

        private void OnTaskPaneHandlerDisposed(ITaskPaneHandler handler)
        {
            handler.Disposed -= OnTaskPaneHandlerDisposed;
            m_TaskPanes.Remove(handler);
        }

        public void Dispose()
        {
            for (int i = m_TaskPanes.Count - 1; i >= 0; i--)
            {
                m_TaskPanes[i].Delete();
            }

            m_TaskPanes.Clear();
        }
    }
}
