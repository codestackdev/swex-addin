//**********************
//SwEx.AddIn - development tools for SOLIDWORKS add-ins
//Copyright(C) 2018 www.codestack.net
//License: https://github.com/codestack-net-dev/sw-dev-tools-addin/blob/master/LICENSE
//Product URL: https://www.codestack.net/labs/solidworks/swex/add-in/
//**********************

using System;
using System.Reflection;

namespace CodeStack.SwEx.AddIn.Reflection
{
    internal static class ResourceHelper
    {
        internal static T GetResource<T>(Type resType, string resName)
        {
            return (T)GetValue(null, resType, resName.Split('.'));
        }

        internal static object GetValue(object obj, Type type, string[] prpsPath)
        {
            foreach (var prpName in prpsPath)
            {
                var prp = type.GetProperty(prpName,
                    BindingFlags.NonPublic | BindingFlags.Public
                    | BindingFlags.Static | BindingFlags.Instance);

                if (prp == null)
                {
                    throw new NullReferenceException($"Resource '{prpName}' is missing in '{type.Name}'");
                }

                obj = prp.GetValue(obj, null);

                if (obj != null)
                {
                    type = obj.GetType();
                }
            }

            return obj;
        }
    }
}
