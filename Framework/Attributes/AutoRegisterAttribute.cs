using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CodeStack.Dev.Sw.AddIn.Attributes
{
    public class AutoRegisterAttribute : Attribute
    {
        public string Title { get; private set; }
        public string Description { get; private set; }
        public bool LoadAtStartup { get; private set; }

        public AutoRegisterAttribute(string title = "", string desc = "", bool loadAtStartup = true)
        {
            Title = title;
            Description = desc;
            LoadAtStartup = loadAtStartup;
        }
    }
}
