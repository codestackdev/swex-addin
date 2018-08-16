using SolidWorks.Interop.swconst;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeStack.Dev.Sw.AddIn.Attributes
{
    public class CommandGroupInfoAttribute : Attribute
    {
        public int Id { get; private set; }

        public CommandGroupInfoAttribute(int id)
        {
            Id = id;
        }
    }

    public class ContextMenuInfoAttribute : CommandGroupInfoAttribute
    {
        public swSelectType_e SelectType { get; private set; }

        public ContextMenuInfoAttribute(int id, swSelectType_e selectType) : base(id)
        {
            SelectType = selectType;
        }
    }
}
