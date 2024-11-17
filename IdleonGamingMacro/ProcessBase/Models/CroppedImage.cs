using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Diagnostics;
using ProcessBase.Events;
using ProcessBase.Helpers;

namespace ProcessBase.Models
{
    public class CroppedImage
    {
        public Mat Image { get; private set; }
        public int X { get; private set; }
        public int Y { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }

        public CroppedImage(int captureX, int captureY, int captureWidth, int captureHeight)
        {
            // キャプチャ範囲の設定を確認
            LogControlHelper.debugLog($"[IdleonGaming] CaptureX: {captureX}, CaptureY: {captureY}, Width: {captureWidth}, Height: {captureHeight}");

            Bitmap screenshot = new Bitmap(captureWidth, captureHeight, PixelFormat.Format32bppArgb);
            using (Graphics g = Graphics.FromImage(screenshot))
            {
                g.CopyFromScreen(captureX, captureY, 0, 0, new System.Drawing.Size(captureWidth, captureHeight), CopyPixelOperation.SourceCopy);
            }

            Image = OpenCvSharp.Extensions.BitmapConverter.ToMat(screenshot);

            X = captureX;
            Y = captureY;
            Width = captureWidth;
            Height = captureHeight;
        }

        // 画像を解放するメソッド
        public void Dispose()
        {
            Image?.Dispose();
        }
    }
}
