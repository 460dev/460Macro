using IdleonGamingMacro.Events;
using IdleonGamingMacro.Models;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static System.Formats.Asn1.AsnWriter;

namespace IdleonGamingMacro.Helpers
{
    public class GamingHelper
    {
        [DllImport("user32.dll")]
        private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool MoveWindow(IntPtr hWnd, int x, int y, int nWidth, int nHeight, bool bRepaint);

        public struct ImagePath
        {
            public const string HarvestAllImagePath = "harvest_all.png";
            public const string SprinklerMaxImagePath = "sprinkler_max_transparent.png";
            public const string ChemicalImagePath = "chemical.png";
            public const string Number2020ImagePath = "20_20.png";
            public const string SprinklerImagePath = "sprinkler.png";
            public const string ShovelNoEffectImagePath = "shovel_no_effect.png";
            public const string SquirrelImagePath = "squirrel_transparent.png";
            public const string CancelBottunImagePath = "cancel_button.png";
        }

        public IntPtr WindowHandle { get; private set; }

        private OpenCVSharpHelper OpenCVSharpHelper;

        public const string WindowTitle = "Legends Of Idleon";

        private const int GameX = 6;
        private const int GameY = 37;
        private const int GamingWidth = 600;
        private const int GamingHeight = 400;

        public GamingHelper(IntPtr windowHandle)
        {
            WindowHandle = windowHandle;
            OpenCVSharpHelper = new OpenCVSharpHelper();
        }

        private CancellationTokenSource? _cancellationTokenSource;

        public void Start(CancellationToken cancellationToken)
        {
            RECT bounds = InitializeWindow();

            int getWindowRectCount = 0;
            int squrrielLoopCount = 0;

            while (!cancellationToken.IsCancellationRequested)
            {
                if (getWindowRectCount > 10)
                {
                    GetWindowRect(WindowHandle, out bounds);
                    getWindowRectCount = 0;
                }

                ImageResult harvestResult = CheckImageProcess(bounds, GameX, GameY, GamingWidth, GamingHeight, ImagePath.HarvestAllImagePath, threshold: 0.7);
                if (harvestResult.Status)
                {
                    BackGroundMouseClicker.SendClickToWindowAsync(WindowHandle, harvestResult.X - bounds.Left, harvestResult.Y - bounds.Top).ConfigureAwait(false);
                }

                ImageResult sprinklerMaxResult = CheckImageProcess(bounds, 0, 0, 800, 480, ImagePath.SprinklerMaxImagePath, threshold: 0.7, scale: 0.8); // Sprinkler max
                if (!sprinklerMaxResult.Status)
                {
                    ImageResult chemicalResult = CheckImageProcess(bounds, GameX, GameY, GamingWidth, GamingHeight, ImagePath.ChemicalImagePath, threshold: 0.35, scale: 0.9); // Chemical
                    if (chemicalResult.Status)
                    {
                        BackGroundMouseClicker.SendClickToWindowAsync(WindowHandle, chemicalResult.X - bounds.Left, chemicalResult.Y - bounds.Top).ConfigureAwait(false);
                    }
                }

                ImageResult number2020Result = CheckImageProcess(bounds, GameX, GameY, GamingWidth, GamingHeight, ImagePath.Number2020ImagePath, threshold: 0.9); // 2020 number
                if (!number2020Result.Status)
                {
                    ImageResult sprinklerResult = CheckImageProcess(bounds, GameX, GameY, GamingWidth, GamingHeight, ImagePath.SprinklerImagePath, threshold: 0.7); // Sprinkler
                    if (sprinklerResult.Status)
                    {
                        BackGroundMouseClicker.SendClickToWindowAsync(WindowHandle, sprinklerResult.X - bounds.Left, sprinklerResult.Y - bounds.Top).ConfigureAwait(false);
                    }
                }

                if (squrrielLoopCount > 10)
                {
                    ImageResult squirrelResult = CheckImageProcess(bounds, GameX, GameY, GamingWidth, GamingHeight, ImagePath.SquirrelImagePath, threshold: 0.5);     // Squirrel
                    if (squirrelResult.Status)
                    {
                        BackGroundMouseClicker.SendClickToWindowAsync(WindowHandle, squirrelResult.X - bounds.Left, squirrelResult.Y - bounds.Top).ConfigureAwait(false);
                    }
                    squrrielLoopCount = 0;
                }

                ImageResult shovelResult = CheckImageProcess(bounds, 527, 326, 69, 69, ImagePath.ShovelNoEffectImagePath, threshold: 0.5); // Shovel
                if (shovelResult.Status)
                {
                    BackGroundMouseClicker.SendClickToWindowAsync(WindowHandle, shovelResult.X - bounds.Left, shovelResult.Y - bounds.Top).ConfigureAwait(false);
                }

                ImageResult cancelButtonResult = CheckImageProcess(bounds, GameX, GameY, GamingWidth, GamingHeight, ImagePath.CancelBottunImagePath, threshold: 0.8);   // Cancel button
                if (shovelResult.Status)
                {
                    BackGroundMouseClicker.SendClickToWindowAsync(WindowHandle, shovelResult.X - bounds.Left, shovelResult.Y - bounds.Top).ConfigureAwait(false);
                }

                getWindowRectCount++;
                squrrielLoopCount++;

                LogControlHelper.debugLog("[IdleonGaming] 0.1秒待機");
                try
                {
                    Task.Delay(100, cancellationToken).Wait(); // キャンセルが通知されると即座に中断
                }
                catch (OperationCanceledException)
                {
                    // キャンセルされた場合
                    break;
                }
            }

            LogControlHelper.debugLog("[IdleonGaming] ループを終了しました。");
        }

        public void Stop()
        {
            _cancellationTokenSource?.Cancel();
        }

        private ImageResult CheckImageProcess(RECT bounds, int offsetX, int offsetY, int width, int height, string imagePath, double threshold, double scale = 1.0)
        {
            Rect targetRect = new Rect(
                X: bounds.Left + offsetX,
                Y: bounds.Top + offsetY,
                Width: width,
                Height: height
            );

            ComparisonImageOption imageOption = new(threshold: threshold, scale: scale);

            return OpenCVSharpHelper.CheckImage(targetRect, imagePath, imageOption, bounds.Left, bounds.Top);
        }

        private RECT InitializeWindow()
        {
            SetForegroundWindow(WindowHandle);
            GetWindowRect(WindowHandle, out RECT bounds);
            MoveWindow(WindowHandle, bounds.Left, bounds.Top, 800, 480, true);
            GetWindowRect(WindowHandle, out bounds);

            return bounds;
        }
    }
}
