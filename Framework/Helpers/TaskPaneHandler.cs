using CodeStack.SwEx.AddIn.Attributes;
using CodeStack.SwEx.AddIn.Icons;
using CodeStack.SwEx.Common.Icons;
using CodeStack.SwEx.Common.Reflection;
using SolidWorks.Interop.sldworks;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace CodeStack.SwEx.AddIn.Helpers
{
    internal interface ITaskPaneHandler : IDisposable
    {
        event Action<ITaskPaneHandler> Disposed;
        void Delete();
    }

    internal enum EmptyTaskPaneCommands_e
    {
    }

    internal class TaskPaneHandler<TCmdEnum> : ITaskPaneHandler
        where TCmdEnum : IComparable, IFormattable, IConvertible
    {
        public event Action<ITaskPaneHandler> Disposed;

        private const int S_OK = 0;

        private readonly ITaskpaneView m_TaskPaneView;

        private readonly Action<TCmdEnum> m_CmdHandler;
        private readonly TCmdEnum[] m_Commands;

        internal TaskPaneHandler(ISldWorks app, ITaskpaneView taskPaneView, Action<TCmdEnum> cmdHandler, IconsConverter iconsConv)
        {
            //TODO: add log

            m_TaskPaneView = taskPaneView;
            m_CmdHandler = cmdHandler;

            if (!typeof(TCmdEnum).IsEnum)
            {
                throw new ArgumentException($"{typeof(TCmdEnum)} must be an enumeration");
            }

            if (typeof(TCmdEnum) != typeof(EmptyTaskPaneCommands_e) && cmdHandler != null)
            {
                var enumValues = Enum.GetValues(typeof(TCmdEnum));

                m_Commands = enumValues.Cast<TCmdEnum>().ToArray();

                foreach (Enum cmdEnum in enumValues)
                {
                    //TODO: check if standard button

                    //NOTE: unlike task pane icon, command icons must have the same transparency key as command manager commands
                    var icon = DisplayInfoExtractor.ExtractCommandDisplayIcon<CommandIconAttribute, CommandGroupIcon>(
                        cmdEnum,
                        i => new MasterIcon(i),
                        a => a.Icon);

                    var tooltip = "";

                    if (!cmdEnum.TryGetAttribute<DisplayNameAttribute>(a => tooltip = a.DisplayName))
                    {
                        cmdEnum.TryGetAttribute<DescriptionAttribute>(a => tooltip = a.Description);
                    }

                    if (app.SupportsHighResIcons(SldWorksExtension.HighResIconsScope_e.TaskPane))
                    {
                        var imageList = iconsConv.ConvertIcon(icon, true);
                        if (!m_TaskPaneView.AddCustomButton2(imageList, tooltip))
                        {
                            throw new InvalidOperationException($"Failed to create task pane button for {cmdEnum} with highres icon");
                        }
                    }
                    else
                    {
                        var imagePath = iconsConv.ConvertIcon(icon, false)[0];
                        if (!m_TaskPaneView.AddCustomButton(imagePath, tooltip))
                        {
                            throw new InvalidOperationException($"Failed to create task pane button for {cmdEnum}");
                        }
                    }
                }

                (m_TaskPaneView as TaskpaneView).TaskPaneToolbarButtonClicked += OnTaskPaneToolbarButtonClicked;
            }

            (m_TaskPaneView as TaskpaneView).TaskPaneDestroyNotify += OnTaskPaneDestroyNotify;
        }

        private int OnTaskPaneToolbarButtonClicked(int buttonIndex)
        {
            //TODO: add log

            if (m_Commands?.Length > buttonIndex)
            {
                m_CmdHandler.Invoke(m_Commands[buttonIndex]);
            }
            else
            {
                //TODO: add log
                Debug.Assert(false, "Invalid command id");
            }

            return S_OK;
        }

        private int OnTaskPaneDestroyNotify()
        {
            //TODO: add log

            Dispose();
            return S_OK;
        }

        public void Delete()
        {
            if (!m_TaskPaneView.DeleteView())
            {
                throw new InvalidOperationException("Failed to remove TaskPane");
            }
        }

        public void Dispose()
        {
            (m_TaskPaneView as TaskpaneView).TaskPaneDestroyNotify -= OnTaskPaneDestroyNotify;
            (m_TaskPaneView as TaskpaneView).TaskPaneToolbarButtonClicked -= OnTaskPaneToolbarButtonClicked;

            var ctrl = m_TaskPaneView.GetControl();

            if (ctrl is IDisposable)
            {
                (ctrl as IDisposable).Dispose();
            }

            Disposed?.Invoke(this);
        }
    }
}
