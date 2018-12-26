//**********************
//SwEx.AddIn - development tools for SOLIDWORKS add-ins
//Copyright(C) 2018 www.codestack.net
//License: https://github.com/codestack-net-dev/sw-dev-tools-addin/blob/master/LICENSE
//Product URL: https://www.codestack.net/labs/solidworks/swex/add-in/
//**********************

using CodeStack.SwEx.AddIn.Attributes;
using CodeStack.SwEx.Common.Diagnostics;
using CodeStack.SwEx.Common.Reflection;
using Microsoft.Win32;
using SolidWorksTools;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;

namespace CodeStack.SwEx.AddIn.Helpers
{
    internal class RegistrationHelper
    {
        private const string ADDIN_REG_KEY_TEMPLATE = @"SOFTWARE\SolidWorks\Addins\{{{0}}}";
        private const string ADDIN_STARTUP_REG_KEY_TEMPLATE = @"Software\SolidWorks\AddInsStartup\{{{0}}}";
        private const string DESCRIPTION_REG_KEY_NAME = "Description";
        private const string TITLE_REG_KEY_NAME = "Title";

        private readonly ModuleLogger m_Logger;

        internal RegistrationHelper(ModuleLogger logger)
        {
            m_Logger = logger;
        }

        internal bool Register(Type type)
        {
            try
            {
                m_Logger.Log($"Registering add-in (is x64 = {Is64BitProcess})");

                //Visual Studio is usually a x32 process and using the 'Register for COM Interops'
                //option will only register the dll in x32 environment
                //Extending the behavior to force register in x64 environment
                if (!Is64BitProcess)
                {
                    m_Logger.Log("Invoking 64-bit registration");
                    return RegisterAssembly(type);
                }
                
                RegisterAddIn(type);

                return true;
            }
            catch (Exception ex)
            {
                m_Logger.LogException(ex);
                return false;
            }
        }

        internal bool Unregister(Type type)
        {
            try
            {
                m_Logger.Log($"Unregistering add-in (is x64 = {Is64BitProcess})");

                if (!Is64BitProcess)
                {
                    m_Logger.Log("Invoking 64-bit unregistration");
                    return UnregisterAssembly(type);
                }
                
                UnregisterAddIn(type);

                return true;
            }
            catch (Exception ex)
            {
                m_Logger.LogException(ex);
                return false;
            }
        }

        private bool RegisterAssembly(Type type)
        {
            return RunRegAsm(type.Assembly.Location, true);
        }

        private bool UnregisterAssembly(Type type)
        {
            return RunRegAsm(type.Assembly.Location, false);
        }

        private bool RunRegAsm(string dllPath, bool register)
        {
            var winDir = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
            var fw64 = @"Microsoft.NET\Framework64";
            var vers = $"v{Environment.Version.ToString(3)}";

            var frameworkDir = Path.Combine(winDir, fw64, vers);

            var regAsmPath = Path.Combine(frameworkDir, "regasm.exe");
            var args = $"/codebase \"{dllPath}\"" + (register ? "" : " /u");

            m_Logger.Log($"Invoking: \"{regAsmPath}\" {args}");

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
                m_Logger.Log(line);
            }

            return prc.ExitCode == 0;
        }

        private void RegisterAddIn(Type type)
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

            m_Logger.Log($"Created HKLM\\{addInKey}");

            addInKey.SetValue(DESCRIPTION_REG_KEY_NAME, desc);
            addInKey.SetValue(TITLE_REG_KEY_NAME, title);

            var addInStartupKey = Registry.CurrentUser.CreateSubKey(
                string.Format(ADDIN_STARTUP_REG_KEY_TEMPLATE, type.GUID));
            addInStartupKey.SetValue(null, Convert.ToInt32(loadAtStartup), RegistryValueKind.DWord);

            m_Logger.Log($"Created HKCU\\{addInStartupKey}");
        }

        private void UnregisterAddIn(Type type)
        {
            var addInKey = string.Format(ADDIN_REG_KEY_TEMPLATE, type.GUID);
            var addInStartupKey = string.Format(ADDIN_STARTUP_REG_KEY_TEMPLATE, type.GUID);

            if (Registry.LocalMachine.OpenSubKey(addInKey, false) != null)
            {
                Registry.LocalMachine.DeleteSubKey(addInKey);
                m_Logger.Log($"Deleting: HKLM\\{addInKey}");
            }

            if (Registry.CurrentUser.OpenSubKey(addInStartupKey, false) != null)
            {
                Registry.CurrentUser.DeleteSubKey(addInStartupKey);
                m_Logger.Log($"Deleting: HKCU\\{addInStartupKey}");
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
