using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Devices;

namespace EposRetail.Models
{
    public class ScreenModel
    {
        public DisplayInfo DisplayInfo { get; set; }
        public double Height { get; set; }
        public double Width { get; set; }
        public string Density { get; set; }

    }
}
