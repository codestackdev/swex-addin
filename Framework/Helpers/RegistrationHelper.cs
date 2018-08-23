//**********************
//SwEx.AddIn - development tools for SOLIDWORKS add-ins
//Copyright(C) 2018 www.codestack.net
//License: https://github.com/codestack-net-dev/sw-dev-tools-addin/blob/master/LICENSE
//Product URL: https://www.codestack.net/labs/solidworks/dev-tools-addin/
//**********************

using CodeStack.SwEx.AddIn.Attributes;
using Microsoft.Win32;
using SolidWorksTools;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;

namespace CodeStack.SwEx.AddIn.Helpers
{
    internal static class RegistrationHelper
    {
        private const string ADDIN_REG_KEY_TEMPLATE = @"SOFTWARE\SolidWorks\Addins\{{{0}}}";
        private const string ADDIN_STARTUP_REG_KEY_TEMPLATE = @"Software\SolidWorks\AddInsStartup\{{{0}}}";
        private const string DESCRIPTION_REG_KEY_NAME = "Description";
        private const string TITLE_REG_KEY_NAME = "Title";

        internal static bool Register(Type type)
        {
            try
            {
                //Visual Studio is usually a x32 process and using the 'Register for COM Interops'
                //option will only register the dll in x32 environment
                //Extending the behavior to force register in x64 environment
                if (!Is64BitProcess)
                {
                    RegisterAssembly(type);
                }
                
                RegisterAddIn(type);

                return true;
            }
            catch (Exception ex)
            {
                Debug.Print("Error while registering the addin: " + ex.Message);
                return false;
            }
        }

        internal static bool Unregister(Type type)
        {
            try
            {
                if (!Is64BitProcess)
                {
                    UnregisterAssembly(type);
                }
                
                UnregisterAddIn(type);

                return true;
            }
            catch (Exception ex)
            {
                Debug.Print("Error while unregistering the addin: " + ex.Message);
                return false;
            }
        }

        private static void RegisterAssembly(Type type)
        {
            RunRegAsm(type.Assembly.Location, true);
        }

        private static void UnregisterAssembly(Type type)
        {
            RunRegAsm(type.Assembly.Location, false);
        }

        private static void RunRegAsm(string dllPath, bool register)
        {
            var winDir = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
            var fw64 = @"Microsoft.NET\Framework64";
            var vers = $"v{Environment.Version.ToString(3)}";

            var frameworkDir = Path.Combine(winDir, fw64, vers);

            var prcInfo = new ProcessStartInfo(Path.Combine(frameworkDir, "regasm.exe"),
                $"/codebase \"{dllPath}\"" + (register ? "" : " /u"))
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            Process.Start(prcInfo);
        }

        private static void RegisterAddIn(Type type)
        {
            string title = "";
            string desc = "";
            bool loadAtStartup = true;

            type.TryGetAttribute<AutoRegisterAttribute>(a =>
            {
                title = a.Title;
                desc = a.Description;
                loadAtStartup = a.LoadAtStartup;
            });

            type.TryGetAttribute<SwAddinAttribute>(a =>
            {
                title = a.Title;
                desc = a.Description;
                loadAtStartup = a.LoadAtStartup;
            });

            type.TryGetAttribute<DisplayNameAttribute>(a => title = a.DisplayName);
            type.TryGetAttribute<DescriptionAttribute>(a => desc = a.Description);

            if (string.IsNullOrEmpty(title))
            {
                title = type.Name;
            }

            var addInkey = Registry.LocalMachine.CreateSubKey(
                string.Format(ADDIN_REG_KEY_TEMPLATE, type.GUID));
            addInkey.SetValue(null, 0);

            addInkey.SetValue(DESCRIPTION_REG_KEY_NAME, desc);
            addInkey.SetValue(TITLE_REG_KEY_NAME, title);

            addInkey = Registry.CurrentUser.CreateSubKey(
                string.Format(ADDIN_STARTUP_REG_KEY_TEMPLATE, type.GUID));
            addInkey.SetValue(null, Convert.ToInt32(loadAtStartup), RegistryValueKind.DWord);
        }

        private static void UnregisterAddIn(Type type)
        {
            Registry.LocalMachine.DeleteSubKey(string.Format(ADDIN_REG_KEY_TEMPLATE, type.GUID));
            Registry.CurrentUser.DeleteSubKey(string.Format(ADDIN_STARTUP_REG_KEY_TEMPLATE, type.GUID));
        }
        
        private static bool Is64BitProcess
        {
            get
            {
                return IntPtr.Size == 8;
            }
        }
    }
}
