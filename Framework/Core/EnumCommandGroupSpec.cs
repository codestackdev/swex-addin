using CodeStack.SwEx.AddIn.Attributes;
using CodeStack.SwEx.AddIn.Helpers;
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
    internal class EnumCommandGroupSpec<TCmdEnum> : CommandGroupSpec
            where TCmdEnum : IComparable, IFormattable, IConvertible
    {
        internal EnumCommandGroupSpec(ISldWorks app, Action<TCmdEnum> callback,
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

            Icon = DisplayInfoExtractor.ExtractCommandDisplayIcon<TaskPaneIconAttribute, CommandGroupIcon>(
                cmdGroupType, i => new MasterIcon(i), a => a.Icon);

            if (!cmdGroupType.TryGetAttribute<DisplayNameAttribute>(a => Title = a.DisplayName))
            {
                Title = cmdGroupType.ToString();
            }

            if (!cmdGroupType.TryGetAttribute<DescriptionAttribute>(a => Tooltip = a.Description))
            {
                Tooltip = cmdGroupType.ToString();
            }

            Commands = Enum.GetValues(cmdGroupType).Cast<TCmdEnum>().Select(
                c => new EnumCommandSpec<TCmdEnum>(app, c, callback, enable)).ToArray();
        }
    }
}
