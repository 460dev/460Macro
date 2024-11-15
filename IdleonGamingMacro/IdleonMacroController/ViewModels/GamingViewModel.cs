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

namespace IdleonMacroController.ViewModels
{
    public class GamingViewModel : BindableBase
    {
        [DllImport("user32.dll")]
        private static extern IntPtr FindWindow(string? lpClassName, string lpWindowName);

        public const string WindowTitle = "Legends Of Idleon"; // 定数

        public DelegateCommand StartStopCommand { get; private set; }

        public ReactiveProperty<string> StartStopText { get; set; } = new ReactiveProperty<string>("Start");

        public ReactiveProperty<bool> IsRunning { get; set; } = new ReactiveProperty<bool>(false);

        private GamingHelper GamingHelper;

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

            if (GamingHelper == null)
            {
                GamingHelper = new GamingHelper(windowHandle: windowHandle);
            }

            if (IsRunning.Value)
            {
                // Stop処理
                GamingHelper.Stop();
                _cancellationTokenSource?.Cancel(); // キャンセルを通知
                LogControlHelper.debugLog("[IdleonGaming] 停止しました。");
                StartStopText.Value = "Start";
            }
            else
            {
                // Start処理
                _cancellationTokenSource = new CancellationTokenSource();
                CancellationToken token = _cancellationTokenSource.Token;


                Task.Run(() => GamingHelper.Start(token));
                LogControlHelper.debugLog("[IdleonGaming] 開始しました。");
                StartStopText.Value = "Stop";
            }

            // 状態を切り替える
            IsRunning.Value = !IsRunning.Value;
        }
    }
}
