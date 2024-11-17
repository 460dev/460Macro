using ProcessBase.Helpers;
using Prism.Commands;
using Prism.Mvvm;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Media.Imaging;

namespace IdleonMacroController.ViewModels
{
    public class DebugModeViewModel : BindableBase
    {
        public const string WindowTitle = "Legends Of Idleon";

        public DelegateCommand GetWindowStatusCommand { get; private set; }

        public ReactiveProperty<int> WindowX { get; set; } = new ReactiveProperty<int>();
        public ReactiveProperty<int> WindowY { get; set; } = new ReactiveProperty<int>();
        public ReactiveProperty<int> WindowWidth { get; set; } = new ReactiveProperty<int>();
        public ReactiveProperty<int> WindowHeight{ get; set; } = new ReactiveProperty<int>();

        public DebugModeViewModel()
        {
            GetWindowStatusCommand = new DelegateCommand(GetWindowStatus);
        }

        private void GetWindowStatus()
        {
            IntPtr windowHandle = WindowAPIHelper.FindWindow(null, WindowTitle);
            if (windowHandle == IntPtr.Zero)
            {
                LogControlHelper.debugLog("[IdleonGaming] Windowが見つかりませんでした。");
                return;
            }

            WindowAPIHelper.GetWindowRect(windowHandle, out WindowAPIHelper.RECT bounds);

            WindowX.Value = bounds.Left;
            WindowY.Value = bounds.Top;
            WindowWidth.Value = bounds.Right - bounds.Left;
            WindowHeight.Value = bounds.Bottom - bounds.Top;
        }
    }
}
