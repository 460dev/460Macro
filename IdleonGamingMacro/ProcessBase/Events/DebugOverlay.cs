#pragma warning disable CA1416

using OpenCvSharp;
using ProcessBase.Models;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Xml;

namespace ProcessBase.Events
{
    public class DebugOverlay
    {
        public static void Overlay(bool isOverlay, Rect targetRect, ImageResult imageResult, Rect borderRect)
        {
            if (isOverlay)
            {
                return;
            }

            // 検知範囲表示 (青)
            using (System.Drawing.Pen pen = new(System.Drawing.Color.Blue, 3))
            {
                DrawDebugRectangle(targetRect.X, targetRect.Y, targetRect.Width, targetRect.Height, pen);
            }

            // 検知した範囲表示 (赤)
            using (System.Drawing.Pen pen = new(System.Drawing.Color.Red, 3))
            {
                DrawDebugRectangle(imageResult.OverlayX, imageResult.OverlayY, imageResult.Width, imageResult.Height, pen);
            }

            // ボーダー線表示 (緑)
            using (System.Drawing.Pen pen = new(System.Drawing.Color.Green, 3))
            {
                DrawDebugRectangle(borderRect.Left, borderRect.Top, borderRect.Width, borderRect.Height, pen);
            }
        }

        // スクリーン上に直接赤枠を描画
        private static void DrawDebugRectangle(int x, int y, int width, int height, Pen pen)
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
