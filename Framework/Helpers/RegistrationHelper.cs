//**********************
//SwEx.AddIn - development tools for SOLIDWORKS add-ins
//Copyright(C) 2018 www.codestack.net
//License: https://github.com/codestack-net-dev/sw-dev-tools-addin/blob/master/LICENSE
//Product URL: https://www.codestack.net/labs/solidworks/swex/add-in/
//**********************

using CodeStack.SwEx.AddIn.Attributes;
using CodeStack.SwEx.Common.Reflection;
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
                Trace.Log($"Registering add-in (is x64 = {Is64BitProcess})");

                //Visual Studio is usually a x32 process and using the 'Register for COM Interops'
                //option will only register the dll in x32 environment
                //Extending the behavior to force register in x64 environment
                if (!Is64BitProcess)
                {
                    Trace.Log("Invoking 64-bit registration");
                    return RegisterAssembly(type);
                }
                
                RegisterAddIn(type);

                return true;
            }
            catch (Exception ex)
            {
                Trace.Log("Error while registering the addin: " + ex.Message);
                return false;
            }
        }

        internal static bool Unregister(Type type)
        {
            try
            {
                Trace.Log($"Unregistering add-in (is x64 = {Is64BitProcess})");

                if (!Is64BitProcess)
                {
                    Trace.Log("Invoking 64-bit unregistration");
                    return UnregisterAssembly(type);
                }
                
                UnregisterAddIn(type);

                return true;
            }
            catch (Exception ex)
            {
                Trace.Log("Error while unregistering the addin: " + ex.Message);
                return false;
            }
        }

        private static bool RegisterAssembly(Type type)
        {
            return RunRegAsm(type.Assembly.Location, true);
        }

        private static bool UnregisterAssembly(Type type)
        {
            return RunRegAsm(type.Assembly.Location, false);
        }

        private static bool RunRegAsm(string dllPath, bool register)
        {
            var winDir = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
            var fw64 = @"Microsoft.NET\Framework64";
            var vers = $"v{Environment.Version.ToString(3)}";

            var frameworkDir = Path.Combine(winDir, fw64, vers);

            var regAsmPath = Path.Combine(frameworkDir, "regasm.exe");
            var args = $"/codebase \"{dllPath}\"" + (register ? "" : " /u");

            Trace.Log($"Invoking: \"{regAsmPath}\" {args}");

            var prcInfo = new ProcessStartInfo(regAsmPath, args)
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            var prc = Process.Start(prcInfo);

            while (!prc.StandardOutput.EndOfStream)
            {
                var line = prc.StandardOutput.ReadLine();
                Trace.Log(line);
            }

            return prc.ExitCode == 0;
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

            var addInKey = Registry.LocalMachine.CreateSubKey(
                string.Format(ADDIN_REG_KEY_TEMPLATE, type.GUID));
            addInKey.SetValue(null, 0);

            Trace.Log($"Created HKLM\\{addInKey}");

            addInKey.SetValue(DESCRIPTION_REG_KEY_NAME, desc);
            addInKey.SetValue(TITLE_REG_KEY_NAME, title);

            var addInStartupKey = Registry.CurrentUser.CreateSubKey(
                string.Format(ADDIN_STARTUP_REG_KEY_TEMPLATE, type.GUID));
            addInStartupKey.SetValue(null, Convert.ToInt32(loadAtStartup), RegistryValueKind.DWord);

            Trace.Log($"Created HKCU\\{addInStartupKey}");
        }

        private static void UnregisterAddIn(Type type)
        {
            var addInKey = string.Format(ADDIN_REG_KEY_TEMPLATE, type.GUID);
            var addInStartupKey = string.Format(ADDIN_STARTUP_REG_KEY_TEMPLATE, type.GUID);

            if (Registry.LocalMachine.OpenSubKey(addInKey, false) != null)
            {
                Registry.LocalMachine.DeleteSubKey(addInKey);
                Trace.Log($"Deleting: HKLM\\{addInKey}");
            }

            if (Registry.CurrentUser.OpenSubKey(addInStartupKey, false) != null)
            {
                Registry.CurrentUser.DeleteSubKey(addInStartupKey);
                Trace.Log($"Deleting: HKCU\\{addInStartupKey}");
            }
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
