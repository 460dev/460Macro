using Prism.Commands;
using Prism.Mvvm;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Linq;
using IdleonGamingMacro.Helpers;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

#nullable enable

namespace IdleonMacroController.ViewModels
{
    public class GamingViewModel : BindableBase
    {

        [DllImport("user32.dll")]
        private static extern IntPtr FindWindow(string? lpClassName, string lpWindowName);

        public const string WindowTitle = "Legends Of Idleon";

        public DelegateCommand StartStopCommand { get; private set; }

        public ReactiveProperty<string> StartStopText { get; set; } = new ReactiveProperty<string>("Start");

        public ReactiveProperty<bool> IsRunning { get; set; } = new ReactiveProperty<bool>(false);

        public ReactiveProperty<bool> IsPreview { get; set; } = new ReactiveProperty<bool>(false);

        private CancellationTokenSource? _cancellationTokenSource;

        public GamingViewModel()
        {
            StartStopCommand = new DelegateCommand(StartStop);
        }

        private void StartStop()
        {
            IntPtr windowHandle = FindWindow(null, WindowTitle);
            if (windowHandle == IntPtr.Zero)
            {
                LogControlHelper.debugLog("[IdleonGaming] Windowが見つかりませんでした。");
                return;
            }

            if (IsRunning.Value)
            {
                // Stop処理
                LogControlHelper.debugLog("[IdleonGaming] 停止します...");

                // GamingHelperの停止処理と解放
                using (var gamingHelper = new GamingHelper(windowHandle))
                {
                    gamingHelper.Stop();
                }

                // キャンセル通知
                _cancellationTokenSource?.Cancel();
                _cancellationTokenSource?.Dispose(); // トークンソースを解放
                _cancellationTokenSource = null;

                LogControlHelper.debugLog("[IdleonGaming] 停止しました。");
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
                        gamingHelper.Start(token, isOverlay:IsPreview.Value);
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
    }
}

#nullable restore