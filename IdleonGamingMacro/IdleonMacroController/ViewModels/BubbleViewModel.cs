using IdleonMacro.Helpers;
using IdleonMacroController.Models;
using Prism.Commands;
using Prism.Mvvm;
using ProcessBase.Helpers;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using static ProcessBase.Constants.Bubble;

#nullable enable

namespace IdleonMacroController.ViewModels
{
    public class BubbleViewModel : BindableBase, IDisposable
    {
        public ReactiveProperty<List<Bubble>> PrinterRedBubbles { get; set; }
        public ReactiveProperty<List<Bubble>> PrinterGreenBubbles { get; set; }
        public ReactiveProperty<List<Bubble>> PrinterBlueBubbles { get; set; }
        public ReactiveProperty<List<Bubble>> PrinterYellowBubbles { get; set; }
        public ReactiveProperty<List<Bubble>> EndlessRedBubbles { get; set; }
        public ReactiveProperty<List<Bubble>> EndlessGreenBubbles { get; set; }
        public ReactiveProperty<List<Bubble>> EndlessBlueBubbles { get; set; }
        public ReactiveProperty<List<Bubble>> EndlessYellowBubbles { get; set; }

        public DelegateCommand StartStopCommand { get; private set; }

        public ReactiveProperty<string> StartStopText { get; set; } = new ReactiveProperty<string>("Start");

        public ReactiveProperty<bool> IsRunning { get; set; } = new ReactiveProperty<bool>(false);

        public ReactiveProperty<bool> IsPreview { get; set; } = new ReactiveProperty<bool>(false);

        public ReactiveProperty<int> UpgradeTimeMin { get; set; } = new ReactiveProperty<int>(15);

        private CancellationTokenSource? _cancellationTokenSource;

        public BubbleViewModel()
        {
            // バブルリストを初期化
            PrinterRedBubbles = new ReactiveProperty<List<Bubble>>(GenerateIconItems("Images/Bubble/Red", BubbleColorIndex.Red));
            PrinterGreenBubbles = new ReactiveProperty<List<Bubble>>(GenerateIconItems("Images/Bubble/Green", BubbleColorIndex.Green));
            PrinterBlueBubbles = new ReactiveProperty<List<Bubble>>(GenerateIconItems("Images/Bubble/Blue", BubbleColorIndex.Blue));
            PrinterYellowBubbles = new ReactiveProperty<List<Bubble>>(GenerateIconItems("Images/Bubble/Yellow", BubbleColorIndex.Yellow));

            EndlessRedBubbles = new ReactiveProperty<List<Bubble>>(GenerateIconItems("Images/Bubble/Red", BubbleColorIndex.Red));
            EndlessGreenBubbles = new ReactiveProperty<List<Bubble>>(GenerateIconItems("Images/Bubble/Green", BubbleColorIndex.Green));
            EndlessBlueBubbles = new ReactiveProperty<List<Bubble>>(GenerateIconItems("Images/Bubble/Blue", BubbleColorIndex.Blue));
            EndlessYellowBubbles = new ReactiveProperty<List<Bubble>>(GenerateIconItems("Images/Bubble/Yellow", BubbleColorIndex.Yellow));

            // プリロードを非同期で実行
            Task.Run(async () =>
            {
                await PreloadImagesAsync(PrinterRedBubbles.Value);
                await PreloadImagesAsync(PrinterGreenBubbles.Value);
                await PreloadImagesAsync(PrinterBlueBubbles.Value);
                await PreloadImagesAsync(PrinterYellowBubbles.Value);

                await PreloadImagesAsync(EndlessRedBubbles.Value);
                await PreloadImagesAsync(EndlessGreenBubbles.Value);
                await PreloadImagesAsync(EndlessBlueBubbles.Value);
                await PreloadImagesAsync(EndlessYellowBubbles.Value);
            });
        }

        private List<Bubble> GenerateIconItems(string folder, BubbleColorIndex bubbleColorIndex)
        {
            string basePath = AppDomain.CurrentDomain.BaseDirectory; // 実行時ディレクトリ
            return Enumerable.Range(1, 30)
                .Select(i =>
                {
                    var bubble = new Bubble(System.IO.Path.Combine(basePath, folder, $"{i}.png"));
                    bubble.BubbleColor = bubbleColorIndex;
                    bubble.BubblePage = (BubblePageIndex)((i - 1) / 5); // 画像番号でページを計算
                    return bubble;
                })
                .ToList();
        }

        private async Task PreloadImagesAsync(List<Bubble> bubbles)
        {
            foreach (var bubble in bubbles)
            {
                bubble.ImageSource.Value = await Task.Run(() =>
                {
                    var image = new BitmapImage();
                    image.BeginInit();
                    image.CacheOption = BitmapCacheOption.OnLoad; // 即時キャッシュ
                    image.UriSource = new Uri(bubble.IconPath, UriKind.RelativeOrAbsolute);
                    image.EndInit();
                    image.Freeze(); // スレッドセーフ化
                    return image;
                });
            }
        }

        private void StartStop()
        {
            IntPtr windowHandle = WindowAPIHelper.FindWindow(null, ProcessBase.Constants.Window.WindowTitle);
            if (windowHandle == IntPtr.Zero)
            {
                LogControlHelper.debugLog("[IdleonMacro] Windowが見つかりませんでした。");
                return;
            }

            if (IsRunning.Value)
            {
                // Stop処理
                LogControlHelper.debugLog("[IdleonMacro] 停止します...");

                // GamingHelperの停止処理と解放
                using (var gamingHelper = new GamingHelper(windowHandle))
                {
                    gamingHelper.Stop();
                }

                // キャンセル通知
                _cancellationTokenSource?.Cancel();
                _cancellationTokenSource?.Dispose(); // トークンソースを解放
                _cancellationTokenSource = null;

                LogControlHelper.debugLog("[IdleonMacro] 停止しました。");
                StartStopText.Value = "Start";
            }
            else
            {
                // Start処理
                LogControlHelper.debugLog("[IdleonGaming] 開始します...");

                _cancellationTokenSource = new CancellationTokenSource();

                // 毎回新しいGamingHelperインスタンスを生成
                var gamingHelper = new GamingHelper(windowHandle);

                CancellationToken token = _cancellationTokenSource.Token;

                Task.Run(() =>
                {
                    try
                    {
                        gamingHelper.Start(token, isOverlay: IsPreview.Value);
                    }
                    finally
                    {
                        // 使用後にリソース解放
                        gamingHelper.Dispose();
                    }
                });

                LogControlHelper.debugLog("[IdleonGaming] 開始しました。");
                StartStopText.Value = "Stop";
            }

            // 状態を切り替える
            IsRunning.Value = !IsRunning.Value;
        }

        public void Dispose()
        {
            PrinterRedBubbles.Dispose();
            PrinterGreenBubbles.Dispose();
            PrinterBlueBubbles.Dispose();
            PrinterYellowBubbles.Dispose();

            EndlessRedBubbles.Dispose();
            EndlessGreenBubbles.Dispose();
            EndlessBlueBubbles.Dispose();
            EndlessYellowBubbles.Dispose();
        }
    }
}

#nullable restore