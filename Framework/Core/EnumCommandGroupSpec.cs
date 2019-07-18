//**********************
//SwEx.AddIn - development tools for SOLIDWORKS add-ins
//Copyright(C) 2019 www.codestack.net
//License: https://github.com/codestack-net-dev/sw-dev-tools-addin/blob/master/LICENSE
//Product URL: https://www.codestack.net/labs/solidworks/swex/add-in/
//**********************

using CodeStack.SwEx.AddIn.Attributes;
using CodeStack.SwEx.AddIn.Base;
using CodeStack.SwEx.AddIn.Helpers;
using CodeStack.SwEx.AddIn.Icons;
using CodeStack.SwEx.Common.Reflection;
using SolidWorks.Interop.sldworks;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace CodeStack.SwEx.AddIn.Core
{
    internal class EnumCommandGroupSpecBase : CommandGroupSpec
    {
        private readonly Type m_CmdGrpEnumType;

        internal Type CmdGrpEnumType
        {
            get
            {
                return m_CmdGrpEnumType;
            }
        }

        internal EnumCommandGroupSpecBase(Type cmdGrpEnumType)
        {
            m_CmdGrpEnumType = cmdGrpEnumType;
        }
    }

    internal class EnumCommandGroupSpec<TCmdEnum> : EnumCommandGroupSpecBase
            where TCmdEnum : IComparable, IFormattable, IConvertible
    {
        internal EnumCommandGroupSpec(ISldWorks app, Action<TCmdEnum> callback,
            EnableMethodDelegate<TCmdEnum> enable, int nextGroupId, 
            IEnumerable<ICommandGroupSpec> groups) : base(typeof(TCmdEnum))
        {
            if (!(typeof(TCmdEnum).IsEnum))
            {
                throw new ArgumentException($"{typeof(TCmdEnum)} must be an Enum");
            }
            
            ExtractCommandGroupInfo(typeof(TCmdEnum), app, callback, enable, nextGroupId, groups);
        }

        private void ExtractCommandGroupInfo(Type cmdGroupType, ISldWorks app, Action<TCmdEnum> callback,
            EnableMethodDelegate<TCmdEnum> enable, int nextGroupId, IEnumerable<ICommandGroupSpec> groups)
        {
            CommandGroupInfoAttribute grpInfoAtt;

            if (cmdGroupType.TryGetAttribute(out grpInfoAtt))
            {
                if (grpInfoAtt.UserId != -1)
                {
                    Id = grpInfoAtt.UserId;
                }
                else
                {
                    Id = nextGroupId;
                }

                if (grpInfoAtt.ParentGroupType != null)
                {
                    var parentGrpSpec = groups.OfType<EnumCommandGroupSpecBase>()
                        .FirstOrDefault(g => g.CmdGrpEnumType == grpInfoAtt.ParentGroupType);

                    if (parentGrpSpec == null)
                    {
                        //TODO: create a specific exception
                        throw new NullReferenceException("Parent group is not created");
                    }

                    if (grpInfoAtt.ParentGroupType == cmdGroupType)
                    {
                        throw new InvalidOperationException("Group cannot be a parent of itself");
                    }

                    Parent = parentGrpSpec;
                }
            }
            else
            {
                Id = nextGroupId;
            }

            Icon = DisplayInfoExtractor.ExtractCommandDisplayIcon<CommandIconAttribute, CommandGroupIcon>(
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
