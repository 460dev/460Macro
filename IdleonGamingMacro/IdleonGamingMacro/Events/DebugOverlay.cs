using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#pragma warning disable CA1416

namespace IdleonGamingMacro.Events
{
    internal class DebugOverlay
    {
        // スクリーン上に直接赤枠を描画
        public static void DrawDebugRectangle(int x, int y, int width, int height, Pen pen)
        {
            using (Graphics g = Graphics.FromHwnd(IntPtr.Zero)) // デスクトップ全体に描画
            {
                g.CompositingMode = CompositingMode.SourceOver;
                g.CompositingQuality = CompositingQuality.HighQuality;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;

                g.DrawRectangle(pen, x, y, width, height);
            }
        }
    }
}

#pragma warning restore CA1416
