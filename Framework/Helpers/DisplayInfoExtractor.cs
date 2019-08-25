//**********************
//SwEx.AddIn - development tools for SOLIDWORKS add-ins
//Copyright(C) 2019 www.codestack.net
//License: https://github.com/codestackdev/swex-addin/blob/master/LICENSE
//Product URL: https://www.codestack.net/labs/solidworks/swex/add-in/
//**********************

using CodeStack.SwEx.AddIn.Properties;
using CodeStack.SwEx.Common.Attributes;
using CodeStack.SwEx.Common.Icons;
using CodeStack.SwEx.Common.Reflection;
using System;
using System.Drawing;

namespace CodeStack.SwEx.AddIn.Helpers
{
    internal static class DisplayInfoExtractor
    {
        internal static TIcon ExtractCommandDisplayIcon<TIconAtt, TIcon>(Type type,
            Func<Image, TIcon> masterIconCreator,
            Func<TIconAtt, TIcon> extractIcon, bool useDefault = true)
            where TIconAtt : Attribute
            where TIcon : IIcon
        {
            TIcon icon = default(TIcon);

            if (!type.TryGetAttribute<TIconAtt>(a => icon = extractIcon.Invoke(a)))
            {
                var masterIcon = type.TryGetAttribute<IconAttribute>()?.Icon;

                if (masterIcon == null)
                {
                    if (useDefault)
                    {
                        masterIcon = Resources.swex_addin_default;
                    }
                    else
                    {
                        return icon;
                    }
                }

                icon = masterIconCreator.Invoke(masterIcon);
            }

            return icon;
        }

        internal static TIcon ExtractCommandDisplayIcon<TIconAtt, TIcon>(Enum enumer,
            Func<Image, TIcon> masterIconCreator,
            Func<TIconAtt, TIcon> extractIcon)
            where TIconAtt : Attribute
            where TIcon : IIcon
        {
            var icon = default(TIcon);

            if (!enumer.TryGetAttribute<TIconAtt>(a => icon = extractIcon.Invoke(a)))
            {
                var masterIcon = enumer.TryGetAttribute<IconAttribute>()?.Icon;

                if (masterIcon == null)
                {
                    masterIcon = Resources.swex_addin_default;
                }

                icon = masterIconCreator.Invoke(masterIcon);
            }

            return icon;
        }
    }
}
