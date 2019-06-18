using CodeStack.SwEx.AddIn.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CodeStack.SwEx.AddIn.Attributes
{
    [AttributeUsage(AttributeTargets.Field)]
    public class CommandSpacerAttribute : Attribute
    {
        internal CommandSpacerPosition_e Position { get; private set; }

        public CommandSpacerAttribute(CommandSpacerPosition_e pos = CommandSpacerPosition_e.Before)
        {
            Position = pos;
        }
    }
}
