using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProcessBase.Models
{
    public class ImageResult
    {
        public ImageResult()
        {

        }

        public bool Status { get; set; }

        public int X { get; set; }
        public int Y { get; set; }
        public int OverlayX { get; set; }
        public int OverlayY { get; set; }
        public int Width { get; set; }
        public int Height { get; set; } 
    }
}
