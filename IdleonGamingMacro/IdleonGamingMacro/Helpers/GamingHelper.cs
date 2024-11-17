using ProcessBase.Events;
using ProcessBase.Models;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static System.Formats.Asn1.AsnWriter;
using System.Dynamic;

namespace ProcessBase.Helpers
{
    public class GamingHelper : IDisposable
    {
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

#nullable enable
        private CancellationTokenSource? _cancellationTokenSource;
#nullable disable

        private bool _disposed = false;

        private int SqurrielLoopCount;
        private int GetWindowRectCount;

        public void Start(CancellationToken cancellationToken, bool isOverlay = false)
        {
            WindowAPIHelper.RECT bounds = InitializeWindow();

            SqurrielLoopCount = 0;
            GetWindowRectCount = 0;

            while (!cancellationToken.IsCancellationRequested)
            {
                if (GetWindowRectCount > 10)
                {
                    WindowAPIHelper.GetWindowRect(WindowHandle, out bounds);
                    GetWindowRectCount = 0;
                }

                GamingProcess(bounds);
                GetWindowRectCount++;

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

        private void GamingProcess(WindowAPIHelper.RECT bounds, bool isOverlay = false)
        {
            ImageResult harvestResult = CheckImageProcess(bounds, GameX, GameY, GamingWidth, GamingHeight, ImagePath.HarvestAllImagePath, threshold: 0.7, isOverlay: isOverlay);
            if (harvestResult.Status)
            {
                BackGroundMouseClicker.SendClickToWindowAsync(WindowHandle, harvestResult.X - bounds.Left, harvestResult.Y - bounds.Top).ConfigureAwait(false);
            }

            ImageResult sprinklerMaxResult = CheckImageProcess(bounds, 0, 0, 800, 480, ImagePath.SprinklerMaxImagePath, threshold: 0.7, scale: 0.8, isOverlay: isOverlay); // Sprinkler max
            if (!sprinklerMaxResult.Status)
            {
                ImageResult chemicalResult = CheckImageProcess(bounds, GameX, GameY, GamingWidth, GamingHeight, ImagePath.ChemicalImagePath, threshold: 0.35, scale: 0.9, isOverlay: isOverlay); // Chemical
                if (chemicalResult.Status)
                {
                    BackGroundMouseClicker.SendClickToWindowAsync(WindowHandle, chemicalResult.X - bounds.Left, chemicalResult.Y - bounds.Top).ConfigureAwait(false);
                }
            }

            ImageResult number2020Result = CheckImageProcess(bounds, GameX, GameY, GamingWidth, GamingHeight, ImagePath.Number2020ImagePath, threshold: 0.9, isOverlay: isOverlay); // 2020 number
            if (!number2020Result.Status)
            {
                ImageResult sprinklerResult = CheckImageProcess(bounds, GameX, GameY, GamingWidth, GamingHeight, ImagePath.SprinklerImagePath, threshold: 0.7, isOverlay: isOverlay); // Sprinkler
                if (sprinklerResult.Status)
                {
                    BackGroundMouseClicker.SendClickToWindowAsync(WindowHandle, sprinklerResult.X - bounds.Left, sprinklerResult.Y - bounds.Top).ConfigureAwait(false);
                }
            }

            if (SqurrielLoopCount > 10)
            {
                ImageResult squirrelResult = CheckImageProcess(bounds, GameX, GameY, GamingWidth, GamingHeight, ImagePath.SquirrelImagePath, threshold: 0.5, isOverlay: isOverlay);     // Squirrel
                if (squirrelResult.Status)
                {
                    BackGroundMouseClicker.SendClickToWindowAsync(WindowHandle, squirrelResult.X - bounds.Left, squirrelResult.Y - bounds.Top).ConfigureAwait(false);
                }
                SqurrielLoopCount = 0;
            }

            ImageResult shovelResult = CheckImageProcess(bounds, 527, 326, 69, 69, ImagePath.ShovelNoEffectImagePath, threshold: 0.5, isOverlay: isOverlay); // Shovel
            if (shovelResult.Status)
            {
                BackGroundMouseClicker.SendClickToWindowAsync(WindowHandle, shovelResult.X - bounds.Left, shovelResult.Y - bounds.Top).ConfigureAwait(false);
            }

            ImageResult cancelButtonResult = CheckImageProcess(bounds, GameX, GameY, GamingWidth, GamingHeight, ImagePath.CancelBottunImagePath, threshold: 0.8, isOverlay: isOverlay);   // Cancel button
            if (cancelButtonResult.Status)
            {
                BackGroundMouseClicker.SendClickToWindowAsync(WindowHandle, cancelButtonResult.X - bounds.Left, cancelButtonResult.Y - bounds.Top).ConfigureAwait(false);
            }

            SqurrielLoopCount++;
        }

        private ImageResult CheckImageProcess(WindowAPIHelper.RECT bounds, int offsetX, int offsetY, int width, int height, string imagePath, double threshold, double scale = 1.0, bool isOverlay = false)
        {
            Rect targetRect = new Rect(
                X: bounds.Left + offsetX,
                Y: bounds.Top + offsetY,
                Width: width,
                Height: height
            );

            ComparisonImageOption imageOption = new(threshold: threshold, scale: scale);

            return OpenCVSharpHelper.CheckImage(targetRect, imagePath, imageOption, bounds.Left, bounds.Top, isOverlay);
        }

        private WindowAPIHelper.RECT InitializeWindow()
        {
            WindowAPIHelper.SetForegroundWindow(WindowHandle);
            WindowAPIHelper.GetWindowRect(WindowHandle, out WindowAPIHelper.RECT bounds);
            WindowAPIHelper.MoveWindow(WindowHandle, bounds.Left, bounds.Top, 800, 480, true);
            WindowAPIHelper.GetWindowRect(WindowHandle, out bounds);

            return bounds;
        }

        /// <summary>
        /// リソースを解放します。
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// リソース解放処理。
        /// </summary>
        /// <param name="disposing">マネージドリソースを解放するかどうか。</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // マネージドリソースの解放
                    _cancellationTokenSource?.Cancel();
                    _cancellationTokenSource?.Dispose();
                    _cancellationTokenSource = null;
                }

                // アンマネージリソースの解放（必要に応じて実装）
                WindowHandle = IntPtr.Zero;

                _disposed = true;
            }
        }

        ~GamingHelper()
        {
            Dispose(false);
        }
    }
}
