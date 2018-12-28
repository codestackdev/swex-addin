using CodeStack.SwEx.AddIn.Attributes;
using CodeStack.SwEx.AddIn.Icons;
using CodeStack.SwEx.AddIn.Properties;
using CodeStack.SwEx.Common.Reflection;
using SolidWorks.Interop.sldworks;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace CodeStack.SwEx.AddIn.Core
{
    internal class EnumCommandBar<TCmdEnum> : CommandBar
            where TCmdEnum : IComparable, IFormattable, IConvertible
    {
        internal EnumCommandBar(ISldWorks app, Action<TCmdEnum> callback,
            EnableMethodDelegate<TCmdEnum> enable, int nextGroupId)
        {
            if (!(typeof(TCmdEnum).IsEnum))
            {
                throw new ArgumentException($"{typeof(TCmdEnum)} must be an Enum");
            }

            ExtractCommandGroupInfo(typeof(TCmdEnum), app, callback, enable, nextGroupId);
        }

        private void ExtractCommandGroupInfo(Type cmdGroupType, ISldWorks app, Action<TCmdEnum> callback,
            EnableMethodDelegate<TCmdEnum> enable, int nextGroupId)
        {
            CommandGroupInfoAttribute grpInfoAtt;

            if (cmdGroupType.TryGetAttribute(out grpInfoAtt))
            {
                Id = grpInfoAtt.UserId;
            }
            else
            {
                Id = nextGroupId;
            }

            DisplayNameAttribute dispNameAtt;

            if (cmdGroupType.TryGetAttribute(out dispNameAtt))
            {
                Title = dispNameAtt.DisplayName;
            }
            else
            {
                Title = cmdGroupType.ToString();
            }

            DescriptionAttribute descAtt;

            if (cmdGroupType.TryGetAttribute(out descAtt))
            {
                Tooltip = descAtt.Description;
            }
            else
            {
                Tooltip = cmdGroupType.ToString();
            }

            if (!cmdGroupType.TryGetAttribute<CommandIconAttribute>(a => Icon = a.Icon))
            {
                var icon = cmdGroupType.TryGetAttribute<Common.Attributes.IconAttribute>()?.Icon;

                if (icon == null)
                {
                    icon = Resources.swex_addin_default;
                }

                Icon = new MasterIcon(icon);
            }

            Commands = Enum.GetValues(cmdGroupType).Cast<TCmdEnum>().Select(
                c => new EnumCommand<TCmdEnum>(app, c, callback, enable)).ToArray();
        }
    }
}
