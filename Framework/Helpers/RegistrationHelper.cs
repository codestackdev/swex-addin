//**********************
//Development tools for SOLIDWORKS add-ins
//Copyright(C) 2018 www.codestack.net
//License: https://github.com/codestack-net-dev/sw-dev-tools-addin/blob/master/LICENSE
//Product URL: https://www.codestack.net/labs/solidworks/dev-tools-addin/
//**********************

using CodeStack.Dev.Sw.AddIn.Attributes;
using Microsoft.Win32;
using SolidWorksTools;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace CodeStack.Dev.Sw.AddIn.Helpers
{
    internal static class RegistrationHelper
    {
        private const string ADDIN_REG_KEY_TEMPLATE = @"SOFTWARE\SolidWorks\Addins\{0}";
        private const string ADDIN_STARTUP_REG_KEY_TEMPLATE = @"Software\SolidWorks\AddInsStartup\{0}";
        private const string DESCRIPTION_REG_KEY_NAME = "Description";
        private const string TITLE_REG_KEY_NAME = "Title";

        internal static bool Register(Type t)
        {
            try
            {
                string title = "";
                string desc = "";
                bool loadAtStartup = true;

                t.TryGetAttribute<AutoRegisterAttribute>(a =>
                {
                    title = a.Title;
                    desc = a.Description;
                    loadAtStartup = a.LoadAtStartup;
                });

                t.TryGetAttribute<SwAddinAttribute>(a => 
                {
                    title = a.Title;
                    desc = a.Description;
                    loadAtStartup = a.LoadAtStartup;
                });

                t.TryGetAttribute<DisplayNameAttribute>(a => title = a.DisplayName);
                t.TryGetAttribute<DescriptionAttribute>(a => desc = a.Description);

                if (string.IsNullOrEmpty(title))
                {
                    title = t.Name;
                }
                
                var addInkey = Registry.LocalMachine.CreateSubKey(
                    string.Format(ADDIN_REG_KEY_TEMPLATE, t.GUID));
                addInkey.SetValue(null, 0);

                addInkey.SetValue(DESCRIPTION_REG_KEY_NAME, desc);
                addInkey.SetValue(TITLE_REG_KEY_NAME, title);
                
                addInkey = Registry.CurrentUser.CreateSubKey(
                    string.Format(ADDIN_STARTUP_REG_KEY_TEMPLATE, t.GUID));
                addInkey.SetValue(null, Convert.ToInt32(loadAtStartup), RegistryValueKind.DWord);

                return true;
            }
            catch
            {
                return false;
            }
        }

        internal static bool Unregister(Type t)
        {
            try
            {
                Registry.LocalMachine.DeleteSubKey(string.Format(ADDIN_REG_KEY_TEMPLATE, t.GUID));
                Registry.CurrentUser.DeleteSubKey(string.Format(ADDIN_STARTUP_REG_KEY_TEMPLATE, t.GUID));

                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Error while unregistering the addin: " + e.Message);
                return false;
            }
        }
    }
}
