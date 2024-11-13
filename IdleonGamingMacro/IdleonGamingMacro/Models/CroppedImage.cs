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
using IdleonGamingMacro.Events;
using IdleonGamingMacro.Helpers;

namespace IdleonGamingMacro.Models
{
    internal class CroppedImage
    {
        public Mat Image { get; private set; }

        public CroppedImage(int captureX, int captureY, int captureWidth, int captureHeight)
        {
            // キャプチャ範囲の設定を確認
            LogControlHelper.debugLog($"[IdleonGaming] CaptureX: {captureX}, CaptureY: {captureY}, Width: {captureWidth}, Height: {captureHeight}");

            Bitmap screenshot = new Bitmap(captureWidth, captureHeight, PixelFormat.Format32bppArgb);
            using (Graphics g = Graphics.FromImage(screenshot))
            {
                g.CopyFromScreen(captureX, captureY, 0, 0, new System.Drawing.Size(captureWidth, captureHeight), CopyPixelOperation.SourceCopy);
            }

            // BitmapをMatに変換
            Mat matImage = OpenCvSharp.Extensions.BitmapConverter.ToMat(screenshot);

            //// デバッグ表示
            //DebugOverlay.DrawDebugRectangle(captureX, captureY, captureWidth, captureHeight);

            Image = OpenCvSharp.Extensions.BitmapConverter.ToMat(screenshot);
        }

        // 画像を解放するメソッド
        public void Dispose()
        {
            Image?.Dispose();
        }
    }
}
