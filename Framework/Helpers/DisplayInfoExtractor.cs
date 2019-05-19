using CodeStack.SwEx.AddIn.Attributes;
using CodeStack.SwEx.AddIn.Icons;
using CodeStack.SwEx.AddIn.Properties;
using CodeStack.SwEx.Common.Attributes;
using CodeStack.SwEx.Common.Icons;
using CodeStack.SwEx.Common.Reflection;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;

namespace CodeStack.SwEx.AddIn.Helpers
{
    internal static class DisplayInfoExtractor
    {
        internal static TIcon ExtractCommandDisplayIcon<TIconAtt, TIcon>(Type type,
            Func<Image, TIcon> masterIconCreator,
            Func<TIconAtt, TIcon> extractIcon)
            where TIconAtt : Attribute
            where TIcon : IIcon
        {
            TIcon icon = default(TIcon);

            if (!type.TryGetAttribute<TIconAtt>(a => icon = extractIcon.Invoke(a)))
            {
                var masterIcon = type.TryGetAttribute<IconAttribute>()?.Icon;

                if (masterIcon == null)
                {
                    masterIcon = Resources.swex_addin_default;
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
            TIcon icon = default(TIcon);

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
