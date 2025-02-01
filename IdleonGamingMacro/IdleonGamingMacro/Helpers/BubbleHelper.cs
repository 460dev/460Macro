using IdleonMacro.Models;
using OpenCvSharp;
using ProcessBase.Constants;
using ProcessBase.Events;
using ProcessBase.Helpers;
using ProcessBase.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace IdleonMacro.Helpers
{
    public class BubbleHelper : IDisposable
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

            public const string CrucibleImagePath = "crucible.png";
            public const string BrewingDisableImagePath = "brewing_disable.png";

            public const string RedBubble1ImagePath = @"Bubble\Red\1.png";
            public const string RedBubble6ImagePath = @"Bubble\Red\6.png";
            public const string RedBubble11ImagePath = @"Bubble\Red\11.png";
            public const string RedBubble16ImagePath = @"Bubble\Red\16.png";
            public const string RedBubble21ImagePath = @"Bubble\Red\21.png";
            public const string RedBubble26ImagePath = @"Bubble\Red\26.png";

            public const string GreenBubble1ImagePath = @"Bubble\Green\1.png";
            public const string GreenBubble6ImagePath = @"Bubble\Green\6.png";
            public const string GreenBubble11ImagePath = @"Bubble\Green\11.png";
            public const string GreenBubble16ImagePath = @"Bubble\Green\16.png";
            public const string GreenBubble21ImagePath = @"Bubble\Green\21.png";
            public const string GreenBubble26ImagePath = @"Bubble\Green\26.png";

            public const string BlueBubble1ImagePath = @"Bubble\Blue\1.png";
            public const string BlueBubble6ImagePath = @"Bubble\Blue\6.png";
            public const string BlueBubble11ImagePath = @"Bubble\Blue\11.png";
            public const string BlueBubble16ImagePath = @"Bubble\Blue\16.png";
            public const string BlueBubble21ImagePath = @"Bubble\Blue\21.png";
            public const string BlueBubble26ImagePath = @"Bubble\Blue\26.png";

            public const string YellowBubble1ImagePath = @"Bubble\Yellow\1.png";
            public const string YellowBubble6ImagePath = @"Bubble\Yellow\6.png";
            public const string YellowBubble11ImagePath = @"Bubble\Yellow\11.png";
            public const string YellowBubble16ImagePath = @"Bubble\Yellow\16.png";
            public const string YellowBubble21ImagePath = @"Bubble\Yellow\21.png";
            public const string YellowBubble26ImagePath = @"Bubble\Yellow\26.png";
        }

        public IntPtr WindowHandle { get; private set; }
        private OpenCVSharpHelper OpenCVSharpHelper;

        public List<Models.Bubble> PrinterBubbles { get; private set; }
        public List<Models.Bubble> EndlessBubbles { get; private set; }

        private readonly int WindowWidth;
        private readonly int WindowHeight;

        public readonly Rect GamingFrame = new Rect(
            X: 30,
            Y: 44,
            Width: 700,
            Height: 430
        );

        public BubbleHelper(IntPtr windowHandle, List<Models.Bubble> printerBubbles, List<Models.Bubble> endlessBubbles)
        {
            WindowHandle = windowHandle;
            OpenCVSharpHelper = new OpenCVSharpHelper();

            WindowWidth = ProcessBase.Constants.Window.WindowWidth;
            WindowHeight = ProcessBase.Constants.Window.WindowHeight;
            PrinterBubbles = printerBubbles;
            EndlessBubbles = endlessBubbles;
        }

#nullable enable
        private CancellationTokenSource? _cancellationTokenSource;
#nullable disable

        private bool _disposed = false;

        private int SqurrielLoopCount;
        private int GetWindowRectCount;
        private ProcessBase.Constants.Screen.ScreenStatus ScreenStatus;

        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1); // 排他制御用

        public async void Start(CancellationToken cancellationToken, List<Models.Bubble> printerBubbles, List<Models.Bubble> endlessBubbles, bool isGaming, bool isEndlessBubbleUpgrade, bool isOverlay)
        {
            WindowAPIHelper.RECT bounds = InitializeWindow();
            SqurrielLoopCount = 0;
            GetWindowRectCount = 0;
            ScreenStatus = ProcessBase.Constants.Screen.ScreenStatus.None;

            // ゲーミング処理: 常に実行する処理
            if (isGaming)
            {
                _ = Task.Run(async () =>
                {
                    while (true)
                    {
                        await ExecuteWithPriority("Gaming", () => Gaming(bounds, isOverlay), TimeSpan.FromMilliseconds(100));
                    }
                });
            }

            // 3Dプリンター泡アップグレード処理: 1時間置きに実行する処理
            _ = Task.Run(async () =>
            {
                while (true)
                {
                    await Task.Delay(TimeSpan.FromHours(1));
                    await ExecuteWithPriority("3DPrinterBubbleUpgrade", () => PrinterBubbleUpgrade(bounds, printerBubbles, isOverlay));
                }
            });

            // エンドレス泡アップグレード処理: 15分置きに実行する処理
            if (isEndlessBubbleUpgrade)
            {
                _ = Task.Run(async () =>
                {
                    while (true)
                    {
                        await Task.Delay(TimeSpan.FromMinutes(15));
                        await ExecuteWithPriority("EndlessBubbleUpgrade", () => EndlessBubbleUpgrade(bounds, endlessBubbles, isOverlay));
                    }
                });
            }

            // メインスレッドの終了を防ぐ
            await Task.Delay(-1);
        }

        public void Stop()
        {
            _cancellationTokenSource?.Cancel();
        }

        // 排他制御を行いながら処理を実行
        private async Task ExecuteWithPriority(string processName, Func<Task> process, TimeSpan? interval = null)
        {
            await _semaphore.WaitAsync(); // 排他制御開始
            try
            {
                Console.WriteLine($"[{processName}] 開始: {DateTime.Now}");
                await process();
                Console.WriteLine($"[{processName}] 終了: {DateTime.Now}");
            }
            finally
            {
                _semaphore.Release(); // 排他制御解除
            }

            // 次回実行までの間隔
            if (interval.HasValue)
            {
                await Task.Delay(interval.Value);
            }
        }

        private ProcessBase.Constants.Screen.ScreenStatus GetScreenStatus(WindowAPIHelper.RECT bounds, bool isOverlay)
        {
            ImageResult quikRefDisableResult = CheckImageProcess(GetTargetRect(bounds, 0, 0, WindowWidth, WindowHeight),
                                                                 new ReferenceImage(ImagePath.QuikRefDisableImagePath),
                                                                 new ComparisonImageOption(threshold: 0.99));
            if (quikRefDisableResult.Status)
            {
                return ProcessBase.Constants.Screen.ScreenStatus.CodexEtc;
            }

            ImageResult quikRefMenuResult = CheckImageProcess(GetTargetRect(bounds, 0, 0, WindowWidth, WindowHeight),
                                                              new ReferenceImage(ImagePath.QuikRefMenuImagePath),
                                                              new ComparisonImageOption(threshold: 0.9));
            if (quikRefMenuResult.Status)
            {
                return ProcessBase.Constants.Screen.ScreenStatus.CodexQuikRef;
            }

            ImageResult brewingDisableResult = CheckImageProcess(GetTargetRect(bounds, 0, 0, WindowWidth, WindowHeight),
                                                                 new ReferenceImage(ImagePath.BrewingDisableImagePath),
                                                                 new ComparisonImageOption(threshold: 0.99));
            if (brewingDisableResult.Status)
            {
                return ProcessBase.Constants.Screen.ScreenStatus.AlchemyEtc;
            }

            ImageResult crucibleResult = CheckImageProcess(GetTargetRect(bounds, 0, 0, WindowWidth, WindowHeight),
                                                                 new ReferenceImage(ImagePath.CrucibleImagePath),
                                                                 new ComparisonImageOption(threshold: 0.9));
            if (crucibleResult.Status)
            {
                return ProcessBase.Constants.Screen.ScreenStatus.Brewing;
            }

            ImageResult logBookResult = CheckImageProcess(GetTargetRect(bounds, 0, 0, WindowWidth, WindowHeight),
                                                          new ReferenceImage(ImagePath.LogBookImagePath),
                                                          new ComparisonImageOption(threshold: 0.9));
            if (logBookResult.Status)
            {
                return ProcessBase.Constants.Screen.ScreenStatus.Gaming;
            }

            return ProcessBase.Constants.Screen.ScreenStatus.None;
        }

        private ProcessBase.Constants.Bubble.BubblePageIndex GetRedBubblePage(Rect targetRect, bool isOverlay)
        {
            ImageResult redBubble1Result = CheckImageProcess(targetRect,
                                                            new ReferenceImage(ImagePath.RedBubble1ImagePath),
                                                            new ComparisonImageOption(threshold: 0.9));
            if (redBubble1Result.Status)
            {
                return ProcessBase.Constants.Bubble.BubblePageIndex.W1;
            }

            ImageResult redBubble6Result = CheckImageProcess(targetRect,
                                                            new ReferenceImage(ImagePath.RedBubble6ImagePath),
                                                            new ComparisonImageOption(threshold: 0.9));
            if (redBubble6Result.Status)
            {
                return ProcessBase.Constants.Bubble.BubblePageIndex.W2;
            }

            ImageResult redBubble11Result = CheckImageProcess(targetRect,
                                                            new ReferenceImage(ImagePath.RedBubble11ImagePath),
                                                            new ComparisonImageOption(threshold: 0.9));
            if (redBubble11Result.Status)
            {
                return ProcessBase.Constants.Bubble.BubblePageIndex.W3;
            }

            ImageResult redBubble16Result = CheckImageProcess(targetRect,
                                                            new ReferenceImage(ImagePath.RedBubble16ImagePath),
                                                            new ComparisonImageOption(threshold: 0.9));
            if (redBubble16Result.Status)
            {
                return ProcessBase.Constants.Bubble.BubblePageIndex.W4;
            }

            ImageResult redBubble21Result = CheckImageProcess(targetRect,
                                                          new ReferenceImage(ImagePath.RedBubble21ImagePath),
                                                          new ComparisonImageOption(threshold: 0.9));
            if (redBubble21Result.Status)
            {
                return ProcessBase.Constants.Bubble.BubblePageIndex.W5;
            }

            ImageResult logBookResult = CheckImageProcess(targetRect,
                                                          new ReferenceImage(ImagePath.LogBookImagePath),
                                                          new ComparisonImageOption(threshold: 0.9));
            if (logBookResult.Status)
            {
                return ProcessBase.Constants.Bubble.BubblePageIndex.W6;
            }

            return ProcessBase.Constants.Bubble.BubblePageIndex.W6;
        }

        private ProcessBase.Constants.Bubble.BubblePageIndex GetGreenBubblePage(Rect targetRect, bool isOverlay)
        {
            ImageResult greenBubble1Result = CheckImageProcess(targetRect,
                                                               new ReferenceImage(ImagePath.GreenBubble1ImagePath),
                                                               new ComparisonImageOption(threshold: 0.9));
            if (greenBubble1Result.Status)
            {
                return ProcessBase.Constants.Bubble.BubblePageIndex.W1;
            }

            ImageResult greenBubble6Result = CheckImageProcess(targetRect,
                                                               new ReferenceImage(ImagePath.GreenBubble6ImagePath),
                                                               new ComparisonImageOption(threshold: 0.9));
            if (greenBubble6Result.Status)
            {
                return ProcessBase.Constants.Bubble.BubblePageIndex.W2;
            }

            ImageResult greenBubble11Result = CheckImageProcess(targetRect,
                                                                new ReferenceImage(ImagePath.GreenBubble11ImagePath),
                                                                new ComparisonImageOption(threshold: 0.9));
            if (greenBubble11Result.Status)
            {
                return ProcessBase.Constants.Bubble.BubblePageIndex.W3;
            }

            ImageResult greenBubble16Result = CheckImageProcess(targetRect,
                                                                new ReferenceImage(ImagePath.GreenBubble16ImagePath),
                                                                new ComparisonImageOption(threshold: 0.9));
            if (greenBubble16Result.Status)
            {
                return ProcessBase.Constants.Bubble.BubblePageIndex.W4;
            }

            ImageResult greenBubble21Result = CheckImageProcess(targetRect,
                                                                new ReferenceImage(ImagePath.GreenBubble21ImagePath),
                                                                new ComparisonImageOption(threshold: 0.9));
            if (greenBubble21Result.Status)
            {
                return ProcessBase.Constants.Bubble.BubblePageIndex.W5;
            }

            ImageResult logBookResult = CheckImageProcess(targetRect,
                                                          new ReferenceImage(ImagePath.LogBookImagePath),
                                                          new ComparisonImageOption(threshold: 0.9));
            if (logBookResult.Status)
            {
                return ProcessBase.Constants.Bubble.BubblePageIndex.W6;
            }

            return ProcessBase.Constants.Bubble.BubblePageIndex.W6;
        }

        private ProcessBase.Constants.Bubble.BubblePageIndex GetBlueBubblePage(Rect targetRect, bool isOverlay)
        {
            ImageResult blueBubble1Result = CheckImageProcess(targetRect,
                                                              new ReferenceImage(ImagePath.BlueBubble1ImagePath),
                                                              new ComparisonImageOption(threshold: 0.9));
            if (blueBubble1Result.Status)
            {
                return ProcessBase.Constants.Bubble.BubblePageIndex.W1;
            }

            ImageResult blueBubble6Result = CheckImageProcess(targetRect,
                                                              new ReferenceImage(ImagePath.BlueBubble6ImagePath),
                                                              new ComparisonImageOption(threshold: 0.9));
            if (blueBubble6Result.Status)
            {
                return ProcessBase.Constants.Bubble.BubblePageIndex.W2;
            }

            ImageResult blueBubble11Result = CheckImageProcess(targetRect,
                                                               new ReferenceImage(ImagePath.BlueBubble11ImagePath),
                                                               new ComparisonImageOption(threshold: 0.9));
            if (blueBubble11Result.Status)
            {
                return ProcessBase.Constants.Bubble.BubblePageIndex.W3;
            }

            ImageResult blueBubble16Result = CheckImageProcess(targetRect,
                                                               new ReferenceImage(ImagePath.BlueBubble16ImagePath),
                                                               new ComparisonImageOption(threshold: 0.9));
            if (blueBubble16Result.Status)
            {
                return ProcessBase.Constants.Bubble.BubblePageIndex.W4;
            }

            ImageResult blueBubble21Result = CheckImageProcess(targetRect,
                                                               new ReferenceImage(ImagePath.BlueBubble21ImagePath),
                                                               new ComparisonImageOption(threshold: 0.9));
            if (blueBubble21Result.Status)
            {
                return ProcessBase.Constants.Bubble.BubblePageIndex.W5;
            }

            ImageResult logBookResult = CheckImageProcess(targetRect,
                                                          new ReferenceImage(ImagePath.LogBookImagePath),
                                                          new ComparisonImageOption(threshold: 0.9));
            if (logBookResult.Status)
            {
                return ProcessBase.Constants.Bubble.BubblePageIndex.W6;
            }

            return ProcessBase.Constants.Bubble.BubblePageIndex.W6;
        }

        private ProcessBase.Constants.Bubble.BubblePageIndex GetYellowBubblePage(Rect targetRect, bool isOverlay)
        {
            ImageResult yellowBubble1Result = CheckImageProcess(targetRect,
                                                                new ReferenceImage(ImagePath.YellowBubble1ImagePath),
                                                                new ComparisonImageOption(threshold: 0.9));
            if (yellowBubble1Result.Status)
            {
                return ProcessBase.Constants.Bubble.BubblePageIndex.W1;
            }

            ImageResult yellowBubble6Result = CheckImageProcess(targetRect,
                                                                new ReferenceImage(ImagePath.YellowBubble6ImagePath),
                                                                new ComparisonImageOption(threshold: 0.9));
            if (yellowBubble6Result.Status)
            {
                return ProcessBase.Constants.Bubble.BubblePageIndex.W2;
            }

            ImageResult yellowBubble11Result = CheckImageProcess(targetRect,
                                                                 new ReferenceImage(ImagePath.YellowBubble11ImagePath),
                                                                 new ComparisonImageOption(threshold: 0.9));
            if (yellowBubble11Result.Status)
            {
                return ProcessBase.Constants.Bubble.BubblePageIndex.W3;
            }

            ImageResult yellowBubble16Result = CheckImageProcess(targetRect,
                                                                 new ReferenceImage(ImagePath.YellowBubble16ImagePath),
                                                                 new ComparisonImageOption(threshold: 0.9));
            if (yellowBubble16Result.Status)
            {
                return ProcessBase.Constants.Bubble.BubblePageIndex.W4;
            }

            ImageResult yellowBubble21Result = CheckImageProcess(targetRect,
                                                                 new ReferenceImage(ImagePath.YellowBubble21ImagePath),
                                                                 new ComparisonImageOption(threshold: 0.9));
            if (yellowBubble21Result.Status)
            {
                return ProcessBase.Constants.Bubble.BubblePageIndex.W5;
            }

            ImageResult logBookResult = CheckImageProcess(targetRect,
                                                          new ReferenceImage(ImagePath.LogBookImagePath),
                                                          new ComparisonImageOption(threshold: 0.9));
            if (logBookResult.Status)
            {
                return ProcessBase.Constants.Bubble.BubblePageIndex.W6;
            }

            return ProcessBase.Constants.Bubble.BubblePageIndex.W6;
        }


        private async Task Gaming(WindowAPIHelper.RECT bounds, bool isOverlay)
        {
            if (GetWindowRectCount > 10)
            {
                WindowAPIHelper.GetWindowRect(WindowHandle, out bounds);
                GetWindowRectCount = 0;
            }

            GetWindowRectCount++;

            // 画面全体から対象画像を探してScreenStatesを更新する
            ScreenStatus = GetScreenStatus(bounds, isOverlay);
            await Task.Delay(200);

            switch (ScreenStatus)
            {
                case ProcessBase.Constants.Screen.ScreenStatus.None:
                    NoneProcess();
                    break;
                case ProcessBase.Constants.Screen.ScreenStatus.CodexQuikRef:
                    CodexQuikRefProcess(bounds, isOverlay);
                    break;
                case ProcessBase.Constants.Screen.ScreenStatus.CodexEtc:
                    CodexEtcProcess(bounds, isOverlay);
                    break;
                case ProcessBase.Constants.Screen.ScreenStatus.Gaming:
                    GamingBitsProcess(bounds, isOverlay);
                    break;
                default:
                    break;
            }
        }

        private async Task PrinterBubbleUpgrade(WindowAPIHelper.RECT bounds, List<Models.Bubble> bubbles, bool isOverlay)
        {
            while (true)
            {
                if (GetWindowRectCount > 10)
                {
                    WindowAPIHelper.GetWindowRect(WindowHandle, out bounds);
                    GetWindowRectCount = 0;
                }

                GetWindowRectCount++;

                // 画面全体から対象画像を探してScreenStatesを更新する
                ScreenStatus = GetScreenStatus(bounds, isOverlay);
                await Task.Delay(200);

                switch (ScreenStatus)
                {
                    case ProcessBase.Constants.Screen.ScreenStatus.None:
                        NoneProcess();
                        break;
                    case ProcessBase.Constants.Screen.ScreenStatus.CodexQuikRef:
                        CodexQuikRefProcess(bounds, isOverlay);
                        break;
                    case ProcessBase.Constants.Screen.ScreenStatus.CodexEtc:
                        CodexEtcProcess(bounds, isOverlay);
                        break;
                    case ProcessBase.Constants.Screen.ScreenStatus.AlchemyEtc:
                        AlchemyEtcProcess(bounds, isOverlay);
                        break;
                    case ProcessBase.Constants.Screen.ScreenStatus.Brewing:
                        PrinterBubbleUpgradeProcess(bounds, bubbles, isOverlay);
                        return; //処理が終了したらreturnする
                    default:
                        break;
                }
            }
        }

        private async Task EndlessBubbleUpgrade(WindowAPIHelper.RECT bounds, List<Models.Bubble> bubbles, bool isOverlay)
        {
            while (true)
            {
                if (GetWindowRectCount > 10)
                {
                    WindowAPIHelper.GetWindowRect(WindowHandle, out bounds);
                    GetWindowRectCount = 0;
                }

                GetWindowRectCount++;

                // 画面全体から対象画像を探してScreenStatesを更新する
                ScreenStatus = GetScreenStatus(bounds, isOverlay);
                await Task.Delay(200);

                switch (ScreenStatus)
                {
                    case ProcessBase.Constants.Screen.ScreenStatus.None:
                        NoneProcess();
                        break;
                    case ProcessBase.Constants.Screen.ScreenStatus.CodexQuikRef:
                        CodexQuikRefProcess(bounds, isOverlay);
                        break;
                    case ProcessBase.Constants.Screen.ScreenStatus.CodexEtc:
                        CodexEtcProcess(bounds, isOverlay);
                        break;
                    case ProcessBase.Constants.Screen.ScreenStatus.AlchemyEtc:
                        AlchemyEtcProcess(bounds, isOverlay);
                        break;
                    case ProcessBase.Constants.Screen.ScreenStatus.Brewing:
                        EndlessBubbleUpgradeProcess(bounds, bubbles, isOverlay);
                        return; //処理が終了したらreturnする
                    default:
                        break;
                }
            }
        }

        private void GamingBitsProcess(WindowAPIHelper.RECT bounds, bool isOverlay)
        {
            ImageResult harvestResult = CheckImageProcess(GetTargetRect(bounds, GamingFrame.X, GamingFrame.Y, GamingFrame.Width, GamingFrame.Height),
                                                          new ReferenceImage(ImagePath.HarvestAllImagePath),
                                                          new ComparisonImageOption(threshold: 0.7),
                                                          GetGamingBorderRect(bounds),
                                                          checkBorderType: OpenCVSharpHelper.CheckBorderType.Out);

            if (harvestResult.Status)
            {
                BackGroundEvent.SendClickToWindowAsync(WindowHandle, harvestResult.X - bounds.Left, harvestResult.Y - bounds.Top).ConfigureAwait(false);
            }

            ImageResult sprinklerMaxResult = CheckImageProcess(GetTargetRect(bounds, GamingFrame.X, GamingFrame.Y, GamingFrame.Width, GamingFrame.Height),
                                                               new ReferenceImage(ImagePath.SprinklerMaxImagePath),
                                                               new ComparisonImageOption(threshold: 0.7, scale: 0.8),
                                                               GetGamingBorderRect(bounds),
                                                               checkBorderType: OpenCVSharpHelper.CheckBorderType.Out); // Sprinkler max
            if (!sprinklerMaxResult.Status)
            {
                ImageResult chemicalResult = CheckImageProcess(GetTargetRect(bounds, GamingFrame.X, GamingFrame.Y, GamingFrame.Width, GamingFrame.Height),
                                                               new ReferenceImage(ImagePath.ChemicalImagePath),
                                                               new ComparisonImageOption(threshold: 0.35, scale: 0.9),
                                                               GetGamingBorderRect(bounds),
                                                               checkBorderType: OpenCVSharpHelper.CheckBorderType.Out); // Chemical
                if (chemicalResult.Status)
                {
                    BackGroundEvent.SendClickToWindowAsync(WindowHandle, chemicalResult.X - bounds.Left, chemicalResult.Y - bounds.Top).ConfigureAwait(false);
                }
            }

            ImageResult number2020Result = CheckImageProcess(GetTargetRect(bounds, GamingFrame.X, GamingFrame.Y, GamingFrame.Width, GamingFrame.Height),
                                                             new ReferenceImage(ImagePath.Number2020ImagePath),
                                                             new ComparisonImageOption(threshold: 0.9),
                                                             GetGamingBorderRect(bounds),
                                                             checkBorderType: OpenCVSharpHelper.CheckBorderType.Out); // 2020 number
            if (!number2020Result.Status)
            {
                ImageResult sprinklerResult = CheckImageProcess(GetTargetRect(bounds, GamingFrame.X, GamingFrame.Y, GamingFrame.Width, GamingFrame.Height),
                                                                new ReferenceImage(ImagePath.SprinklerImagePath),
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
                ImageResult squirrelResult = CheckImageProcess(GetTargetRect(bounds, GamingFrame.X, GamingFrame.Y, GamingFrame.Width, GamingFrame.Height),
                                                               new ReferenceImage(ImagePath.SquirrelImagePath),
                                                               new ComparisonImageOption(threshold: 0.5),
                                                               GetGamingBorderRect(bounds),
                                                               checkBorderType: OpenCVSharpHelper.CheckBorderType.Out);     // Squirrel
                if (squirrelResult.Status)
                {
                    BackGroundEvent.SendClickToWindowAsync(WindowHandle, squirrelResult.X - bounds.Left, squirrelResult.Y - bounds.Top).ConfigureAwait(false);
                    SqurrielLoopCount = 0;
                }
            }

            ImageResult shovelResult = CheckImageProcess(GetTargetRect(bounds, GamingFrame.X, GamingFrame.Y, GamingFrame.Width, GamingFrame.Height),
                                                         new ReferenceImage(ImagePath.ShovelNoEffectImagePath),
                                                         new ComparisonImageOption(threshold: 0.5),
                                                          GetGamingBorderRect(bounds),
                                                         checkBorderType: OpenCVSharpHelper.CheckBorderType.Out); // Shovel
            if (shovelResult.Status)
            {
                BackGroundEvent.SendClickToWindowAsync(WindowHandle, shovelResult.X - bounds.Left, shovelResult.Y - bounds.Top).ConfigureAwait(false);
            }

            ImageResult cancelButtonResult = CheckImageProcess(GetTargetRect(bounds, GamingFrame.X, GamingFrame.Y, GamingFrame.Width, GamingFrame.Height),
                                                               new ReferenceImage(ImagePath.CancelBottunImagePath),
                                                               new ComparisonImageOption(threshold: 0.8),
                                                               GetGamingBorderRect(bounds),
                                                               checkBorderType: OpenCVSharpHelper.CheckBorderType.Out);   // Cancel button
            if (cancelButtonResult.Status)
            {
                BackGroundEvent.SendClickToWindowAsync(WindowHandle, cancelButtonResult.X - bounds.Left, cancelButtonResult.Y - bounds.Top).ConfigureAwait(false);
            }

            SqurrielLoopCount++;
        }

        private void PrinterBubbleUpgradeProcess(WindowAPIHelper.RECT bounds, List<Models.Bubble> bubbles, bool isOverlay)
        {
            foreach (var bubble in bubbles)
            {
                ProcessBase.Constants.Bubble.BubblePageIndex bubblePage = ProcessBase.Constants.Bubble.BubblePageIndex.W1;
                Rect targetRect;

                while (true)
                {
                    switch (bubble.BubbleColor)
                    {
                        case ProcessBase.Constants.Bubble.BubbleColorIndex.Red:
                            targetRect = GetTargetRect(bounds, 0, 0, 0, 0);
                            bubblePage = GetRedBubblePage(targetRect, isOverlay);
                            break;
                        case ProcessBase.Constants.Bubble.BubbleColorIndex.Green:
                            targetRect = GetTargetRect(bounds, 0, 0, 0, 0);
                            bubblePage = GetGreenBubblePage(targetRect, isOverlay);
                            break;
                        case ProcessBase.Constants.Bubble.BubbleColorIndex.Blue:
                            targetRect = GetTargetRect(bounds, 0, 0, 0, 0);
                            bubblePage = GetBlueBubblePage(targetRect, isOverlay);
                            break;
                        case ProcessBase.Constants.Bubble.BubbleColorIndex.Yellow:
                            targetRect = GetTargetRect(bounds, 0, 0, 0, 0);
                            bubblePage = GetYellowBubblePage(targetRect, isOverlay);
                            break;
                        default:
                            break;
                    }

                    // 上ボタンを押す
                    if (bubble.BubblePage > bubblePage)
                    {

                    }
                    // 下ボタンを押す
                    else if (bubble.BubblePage < bubblePage)
                    {

                    }
                    // ページ一致しているのでループを抜け出す
                    else
                    {
                        break;
                    }
                    
                }
            }
        }

        private void EndlessBubbleUpgradeProcess(WindowAPIHelper.RECT bounds, List<Models.Bubble> bubbles, bool isOverlay)
        {
            foreach (var bubble in bubbles)
            {

            }
        }

        private void NoneProcess()
        {
            BackGroundEvent.SendKeyToWindowAsync(WindowHandle, 'C').ConfigureAwait(false);
        }

        private void CodexEtcProcess(WindowAPIHelper.RECT bounds, bool isOverlay = false)
        {
            ImageResult quikRefResult = CheckImageProcess(GetTargetRect(bounds, 0, 0, bounds.Right - bounds.Left, bounds.Bottom - bounds.Top),
                                                          new ReferenceImage(ImagePath.QuikRefDisableImagePath),
                                                          new ComparisonImageOption(threshold: 0.8));
            if (quikRefResult.Status)
            {
                BackGroundEvent.SendClickToWindowAsync(WindowHandle, quikRefResult.X - bounds.Left, quikRefResult.Y - bounds.Top).ConfigureAwait(false);
            }
        }

        private void CodexQuikRefProcess(WindowAPIHelper.RECT bounds, bool isOverlay = false)
        {
            ImageResult quikRefEtcResult = CheckImageProcess(GetTargetRect(bounds, 0, 0, bounds.Right - bounds.Left, bounds.Bottom - bounds.Top),
                                                         new ReferenceImage(ImagePath.GamingImagePath),
                                                         new ComparisonImageOption(threshold: 0.8));
            if (quikRefEtcResult.Status)
            {
                BackGroundEvent.SendClickToWindowAsync(WindowHandle, quikRefEtcResult.X - bounds.Left, quikRefEtcResult.Y - bounds.Top).ConfigureAwait(false);
            }
        }

        private void AlchemyEtcProcess(WindowAPIHelper.RECT bounds, bool isOverlay = false)
        {
            ImageResult alchemyEtcResult = CheckImageProcess(GetTargetRect(bounds, 0, 0, bounds.Right - bounds.Left, bounds.Bottom - bounds.Top),
                                                         new ReferenceImage(ImagePath.BrewingDisableImagePath),
                                                         new ComparisonImageOption(threshold: 0.8));
            if (alchemyEtcResult.Status)
            {
                BackGroundEvent.SendClickToWindowAsync(WindowHandle, alchemyEtcResult.X - bounds.Left, alchemyEtcResult.Y - bounds.Top).ConfigureAwait(false);
            }
        }

        private ImageResult CheckImageProcess(Rect targetRect,
                                              ReferenceImage referenceImage,
                                              ComparisonImageOption imageOption,
                                              Rect borderRect = new(),
                                              OpenCVSharpHelper.CheckBorderType checkBorderType = OpenCVSharpHelper.CheckBorderType.None)
        {
            return OpenCVSharpHelper.CheckImage(borderRect, targetRect, referenceImage, imageOption, checkBorderType);
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

        ~BubbleHelper()
        {
            Dispose(false);
        }

    }
}
