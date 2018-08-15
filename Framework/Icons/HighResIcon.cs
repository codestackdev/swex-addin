using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeStack.Dev.Sw.AddIn.Icons
{
    public class HighResIcon : IIcon
    {
        public Image Size20x20 { get; set; }
        public Image Size32x32 { get; set; }
        public Image Size40x40 { get; set; }
        public Image Size64x64 { get; set; }
        public Image Size96x96 { get; set; }
        public Image Size128x128 { get; set; }
    }
}
