﻿using ProcessBase.Events;
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
using NLog.LayoutRenderers.Wrappers;

namespace ProcessBase.Helpers
{
    public class GamingHelper : IDisposable
    {
        public struct ImagePath
        {
            public const string HarvestAllImagePath = "harvest_all.png";
            public const string SprinklerMaxImagePath = "sprinkler_max.png";
            public const string ChemicalImagePath = "chemical.png";
            public const string Number2020ImagePath = "20_20.png";
            public const string SprinklerImagePath = "sprinkler.png";
            public const string ShovelNoEffectImagePath = "shovel_no_effect.png";
            public const string SquirrelImagePath = "squirrel_transparent.png";
            public const string CancelBottunImagePath = "cancel_button.png";

            public const string LogBookImagePath = "log_book.png";
            public const string QuikRefDisableImagePath = "quik_ref_disable.png";
            public const string QuikRefMenuImagePath = "quik_ref_menu.png";
            public const string GamingImagePath = "gaming.png";
        }

        public IntPtr WindowHandle { get; private set; }
        private OpenCVSharpHelper OpenCVSharpHelper;

        public const string WindowTitle = "Legends Of Idleon";
        private readonly int WindowWidth;
        private readonly int WindowHeight;

        private const int GameX = 30;
        private const int GameY = 44;
        private const int GamingWidth = 700;
        private const int GamingHeight = 430;

        public GamingHelper(IntPtr windowHandle)
        {
            WindowHandle = windowHandle;
            OpenCVSharpHelper = new OpenCVSharpHelper();

            WindowWidth = Constants.Window.WindowWidth;
            WindowHeight = Constants.Window.WindowHeight;
        }

#nullable enable
        private CancellationTokenSource? _cancellationTokenSource;
#nullable disable

        private bool _disposed = false;

        private int SqurrielLoopCount;
        private int GetWindowRectCount;
        private Constants.Screen.ScreenStatus ScreenStatus;

        public void Start(CancellationToken cancellationToken, bool isOverlay)
        {
            WindowAPIHelper.RECT bounds = InitializeWindow();

            SqurrielLoopCount = 0;
            GetWindowRectCount = 0;
            ScreenStatus = Constants.Screen.ScreenStatus.None;

            while (!cancellationToken.IsCancellationRequested)
            {
                if (GetWindowRectCount > 10)
                {
                    WindowAPIHelper.GetWindowRect(WindowHandle, out bounds);
                    GetWindowRectCount = 0;
                }

                GetWindowRectCount++;

                // 画面全体から対象画像を探してScreenStatesを更新する
                ScreenStatus = GetScreenStatus(bounds, isOverlay);
                Task.Delay(200);

                switch (ScreenStatus)
                {
                    case Constants.Screen.ScreenStatus.None:
                        NoneProcess();
                        break;
                    case Constants.Screen.ScreenStatus.CodexQuikRef:
                        CodexQuikRefProcess(bounds, isOverlay);
                        break;
                    case Constants.Screen.ScreenStatus.CodexEtc:
                        CodexEtcProcess(bounds, isOverlay);
                        break;
                    case Constants.Screen.ScreenStatus.Gaming:
                        GamingProcess(bounds, isOverlay);
                        break;
                    default:
                        break;
                }

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

        private Constants.Screen.ScreenStatus GetScreenStatus(WindowAPIHelper.RECT bounds, bool isOverlay)
        {
            ImageResult quikRefDisableResult = CheckImageProcess(GetTargetRect(bounds, 0, 0, WindowWidth, WindowHeight),
                                                                 ImagePath.QuikRefDisableImagePath,
                                                                 new ComparisonImageOption(threshold: 0.99));
            if (quikRefDisableResult.Status)
            {
                return Constants.Screen.ScreenStatus.CodexEtc;
            }

            ImageResult quikRefMenuResult = CheckImageProcess(GetTargetRect(bounds, 0, 0, WindowWidth, WindowHeight),
                                                              ImagePath.QuikRefMenuImagePath,
                                                              new ComparisonImageOption(threshold: 0.9));
            if (quikRefMenuResult.Status)
            {
                return Constants.Screen.ScreenStatus.CodexQuikRef;
            }

            ImageResult logBookResult = CheckImageProcess(GetTargetRect(bounds, 0, 0, WindowWidth, WindowHeight),
                                                          ImagePath.LogBookImagePath,
                                                          new ComparisonImageOption(threshold: 0.9));
            if (logBookResult.Status)
            {
                return Constants.Screen.ScreenStatus.Gaming;
            }

            return Constants.Screen.ScreenStatus.None;
        }

        private void GamingProcess(WindowAPIHelper.RECT bounds, bool isOverlay)
        {
            ImageResult harvestResult = CheckImageProcess(GetTargetRect(bounds, GameX, GameY, GamingWidth, GamingHeight),
                                                          ImagePath.HarvestAllImagePath,
                                                          new ComparisonImageOption(threshold: 0.7),
                                                          GetGamingBorderRect(bounds),
                                                          checkBorderType: OpenCVSharpHelper.CheckBorderType.Out);

            if (harvestResult.Status)
            {
                BackGroundEvent.SendClickToWindowAsync(WindowHandle, harvestResult.X - bounds.Left, harvestResult.Y - bounds.Top).ConfigureAwait(false);
            }

            ImageResult sprinklerMaxResult = CheckImageProcess(GetTargetRect(bounds, GameX, GameY, GamingWidth, GamingHeight),
                                                               ImagePath.SprinklerMaxImagePath,
                                                               new ComparisonImageOption(threshold: 0.7, scale:0.8),
                                                               GetGamingBorderRect(bounds),
                                                               checkBorderType: OpenCVSharpHelper.CheckBorderType.Out); // Sprinkler max
            if (!sprinklerMaxResult.Status)
            {
                ImageResult chemicalResult = CheckImageProcess(GetTargetRect(bounds, GameX, GameY, GamingWidth, GamingHeight),
                                                               ImagePath.ChemicalImagePath,
                                                               new ComparisonImageOption(threshold: 0.35, scale: 0.9),
                                                               GetGamingBorderRect(bounds),
                                                               checkBorderType: OpenCVSharpHelper.CheckBorderType.Out); // Chemical
                if (chemicalResult.Status)
                {
                    BackGroundEvent.SendClickToWindowAsync(WindowHandle, chemicalResult.X - bounds.Left, chemicalResult.Y - bounds.Top).ConfigureAwait(false);
                }
            }

            ImageResult number2020Result = CheckImageProcess(GetTargetRect(bounds, GameX, GameY, GamingWidth, GamingHeight),
                                                             ImagePath.Number2020ImagePath,
                                                             new ComparisonImageOption(threshold: 0.9),
                                                             GetGamingBorderRect(bounds),
                                                             checkBorderType: OpenCVSharpHelper.CheckBorderType.Out); // 2020 number
            if (!number2020Result.Status)
            {
                ImageResult sprinklerResult = CheckImageProcess(GetTargetRect(bounds, GameX, GameY, GamingWidth, GamingHeight),
                                                                ImagePath.SprinklerImagePath,
                                                                new ComparisonImageOption(threshold: 0.7),
                                                                GetGamingBorderRect(bounds),
                                                                checkBorderType: OpenCVSharpHelper.CheckBorderType.Out); // Sprinkler
                if (sprinklerResult.Status)
                {
                    BackGroundEvent.SendClickToWindowAsync(WindowHandle, sprinklerResult.X - bounds.Left, sprinklerResult.Y - bounds.Top).ConfigureAwait(false);
                }
            }

            if (SqurrielLoopCount > 10)
            {
                ImageResult squirrelResult = CheckImageProcess(GetTargetRect(bounds, GameX, GameY, GamingWidth, GamingHeight),
                                                               ImagePath.SquirrelImagePath,
                                                               new ComparisonImageOption(threshold: 0.5),
                                                               GetGamingBorderRect(bounds),
                                                               checkBorderType: OpenCVSharpHelper.CheckBorderType.Out);     // Squirrel
                if (squirrelResult.Status)
                {
                    BackGroundEvent.SendClickToWindowAsync(WindowHandle, squirrelResult.X - bounds.Left, squirrelResult.Y - bounds.Top).ConfigureAwait(false);
                }
                SqurrielLoopCount = 0;
            }

            ImageResult shovelResult = CheckImageProcess(GetTargetRect(bounds, GameX, GameY, GamingWidth, GamingHeight),
                                                         ImagePath.ShovelNoEffectImagePath,
                                                         new ComparisonImageOption(threshold: 0.5),
                                                          GetGamingBorderRect(bounds),
                                                         checkBorderType: OpenCVSharpHelper.CheckBorderType.Out); // Shovel
            if (shovelResult.Status)
            {
                BackGroundEvent.SendClickToWindowAsync(WindowHandle, shovelResult.X - bounds.Left, shovelResult.Y - bounds.Top).ConfigureAwait(false);
            }

            ImageResult cancelButtonResult = CheckImageProcess(GetTargetRect(bounds, GameX, GameY, GamingWidth, GamingHeight),
                                                               ImagePath.CancelBottunImagePath,
                                                               new ComparisonImageOption(threshold: 0.8),
                                                               GetGamingBorderRect(bounds),
                                                               checkBorderType: OpenCVSharpHelper.CheckBorderType.Out);   // Cancel button
            if (cancelButtonResult.Status)
            {
                BackGroundEvent.SendClickToWindowAsync(WindowHandle, cancelButtonResult.X - bounds.Left, cancelButtonResult.Y - bounds.Top).ConfigureAwait(false);
            }

            SqurrielLoopCount++;
        }

        private void NoneProcess()
        {
            BackGroundEvent.SendKeyToWindowAsync(WindowHandle, 'C').ConfigureAwait(false);
        }

        private void CodexEtcProcess(WindowAPIHelper.RECT bounds, bool isOverlay = false)
        {
            ImageResult quikRefResult = CheckImageProcess(GetTargetRect(bounds, 0, 0, bounds.Right - bounds.Left, bounds.Bottom - bounds.Top),
                                                          ImagePath.QuikRefDisableImagePath,
                                                          new ComparisonImageOption(threshold: 0.8));
            if (quikRefResult.Status)
            {
                BackGroundEvent.SendClickToWindowAsync(WindowHandle, quikRefResult.X - bounds.Left, quikRefResult.Y - bounds.Top).ConfigureAwait(false);
            }
        }

        private void CodexQuikRefProcess(WindowAPIHelper.RECT bounds, bool isOverlay = false)
        {
            ImageResult gamingResult = CheckImageProcess(GetTargetRect(bounds, 0, 0, bounds.Right - bounds.Left, bounds.Bottom - bounds.Top),
                                                         ImagePath.GamingImagePath,
                                                         new ComparisonImageOption(threshold: 0.8));
            if (gamingResult.Status)
            {
                BackGroundEvent.SendClickToWindowAsync(WindowHandle, gamingResult.X - bounds.Left, gamingResult.Y - bounds.Top).ConfigureAwait(false);
            }
        }

        private ImageResult CheckImageProcess(Rect targetRect,
                                              string imagePath,
                                              ComparisonImageOption imageOption,
                                              Rect borderRect = new(),
                                              OpenCVSharpHelper.CheckBorderType checkBorderType = OpenCVSharpHelper.CheckBorderType.None)
        {
            return OpenCVSharpHelper.CheckImage(borderRect, targetRect, imagePath, imageOption, checkBorderType);
        }

        private Rect GetGamingBorderRect(WindowAPIHelper.RECT bounds)
        {
            return new Rect(
                X: bounds.Left + 34,
                Y: bounds.Top + 380,
                Width: 110,
                Height: 90
            );
        }

        private Rect GetTargetRect(WindowAPIHelper.RECT bounds, int offsetX, int offsetY, int width, int height)
        {
            return new Rect(
                X: bounds.Left + offsetX,
                Y: bounds.Top + offsetY,
                Width: width,
                Height: height
            );
        }

        private WindowAPIHelper.RECT InitializeWindow()
        {
            WindowAPIHelper.GetWindowRect(WindowHandle, out WindowAPIHelper.RECT bounds);
            WindowAPIHelper.MoveWindow(WindowHandle, bounds.Left, bounds.Top, WindowWidth, WindowHeight, true);
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
