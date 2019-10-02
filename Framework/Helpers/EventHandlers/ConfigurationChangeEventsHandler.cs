//**********************
//SwEx.AddIn - development tools for SOLIDWORKS add-ins
//Copyright(C) 2019 www.codestack.net
//License: https://github.com/codestackdev/swex-addin/blob/master/LICENSE
//Product URL: https://www.codestack.net/labs/solidworks/swex/add-in/
//**********************

using CodeStack.SwEx.AddIn.Delegates;
using SolidWorks.Interop.sldworks;
using CodeStack.SwEx.AddIn.Core;
using CodeStack.SwEx.AddIn.Enums;

namespace CodeStack.SwEx.AddIn.Helpers.EventHandlers
{
    internal class ConfigurationChangeEventsHandler : EventsHandler<ConfigurationChangeDelegate>
    {
        public ConfigurationChangeEventsHandler(DocumentHandler docHandler) : base(docHandler)
        {
        }

        protected override void OnAttach(ConfigurationChangeDelegate del)
        {
            Delegate += del;
        }

        protected override void OnDetach(ConfigurationChangeDelegate del)
        {
            Delegate -= del;
        }

        protected override void SubscribeAssemblyEvents(AssemblyDoc assm)
        {
            assm.ConfigurationChangeNotify += OnConfigurationChangeNotify;
        }

        protected override void SubscribeDrawingEvents(DrawingDoc draw)
        {
            draw.ActivateSheetPreNotify += OnActivateSheetPreNotify;
            draw.ActivateSheetPostNotify += OnActivateSheetPostNotify;
        }

        protected override void SubscribePartEvents(PartDoc part)
        {
            part.ConfigurationChangeNotify += OnConfigurationChangeNotify;
        }

        protected override void UnsubscribeAssemblyEvents(AssemblyDoc assm)
        {
            assm.ConfigurationChangeNotify -= OnConfigurationChangeNotify;
        }

        protected override void UnsubscribeDrawingEvents(DrawingDoc draw)
        {
            draw.ActivateSheetPreNotify -= OnActivateSheetPreNotify;
            draw.ActivateSheetPostNotify -= OnActivateSheetPostNotify;
        }

        protected override void UnsubscribePartEvents(PartDoc part)
        {
            part.ConfigurationChangeNotify -= OnConfigurationChangeNotify;
        }

        private int OnActivateSheetPreNotify(string sheetName)
        {
            Delegate.Invoke(m_DocHandler, ConfigurationChangeState_e.PreActivate,
                sheetName);

            return S_OK;
        }

        private int OnActivateSheetPostNotify(string sheetName)
        {
            Delegate.Invoke(m_DocHandler, ConfigurationChangeState_e.PostActivate,
                sheetName);

            return S_OK;
        }

        private int OnConfigurationChangeNotify(string configurationName, object obj, int objectType, int changeType)
        {
            const int POST_NOTIFICATION = 11;

            Delegate.Invoke(m_DocHandler, (changeType == POST_NOTIFICATION ? ConfigurationChangeState_e.PostActivate : ConfigurationChangeState_e.PreActivate),
                configurationName);

            return S_OK;
        }
    }
}
