using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IdleonGamingMacro.Events
{
    internal class DebugOverlay
    {
        // スクリーン上に直接赤枠を描画
        public static void DrawDebugRectangle(int x, int y, int width, int height)
        {
            using (Graphics g = Graphics.FromHwnd(IntPtr.Zero)) // デスクトップ全体に描画
            {
                g.CompositingMode = CompositingMode.SourceOver;
                g.CompositingQuality = CompositingQuality.HighQuality;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;

                // 赤いペンを作成し、枠を描画
                using (Pen redPen = new Pen(Color.Red, 2))
                {
                    g.DrawRectangle(redPen, x, y, width, height);
                }
            }
        }
    }
}
