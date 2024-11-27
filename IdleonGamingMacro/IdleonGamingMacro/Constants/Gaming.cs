using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IdleonMacro.Constants
{
    public class Gaming
    {
        private const int GameX = 30;
        private const int GameY = 44;
        private const int GamingWidth = 700;
        private const int GamingHeight = 430;

        public readonly Rect GamingFrame = new Rect(
            X: GameX,
            Y: GameY,
            Width: GamingWidth,
            Height: GamingHeight
        );
    }
}
