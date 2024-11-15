using IdleonGamingMacro.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace IdleonGamingMacro.Events
{
    internal static class BackGroundMouseClicker
    {
        [DllImport("user32.dll")]
        private static extern IntPtr FindWindow(string? lpClassName, string lpWindowName);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        private const uint WM_LBUTTONDOWN = 0x0201; // マウス左ボタンダウン
        private const uint WM_LBUTTONUP = 0x0202;   // マウス左ボタンアップ

        public static async Task SendClickToWindowAsync(string windowTitle, int x, int y)
        {
            IntPtr hWnd = FindWindow(null, windowTitle); // タイトルでウィンドウ検索
            if (hWnd == IntPtr.Zero)
            {
                LogControlHelper.debugLog($"[Error] ウィンドウ '{windowTitle}' が見つかりません。");
                return;
            }

            // バックグラウンドでクリックを送信
            await Task.Run(() =>
            {
                try
                {
                    IntPtr lParam = (IntPtr)((y << 16) | x);
                    SendMessage(hWnd, WM_LBUTTONDOWN, IntPtr.Zero, lParam);
                    SendMessage(hWnd, WM_LBUTTONUP, IntPtr.Zero, lParam);

                    LogControlHelper.debugLog($"[IdleonGaming] ウィンドウ '{windowTitle}' にクリックを送信しました。位置: ({x}, {y})");
                }
                catch (Exception ex)
                {
                    LogControlHelper.debugLog($"[Error] SendClickToWindowAsync 内で例外発生: {ex.Message}");
                }
            });
        }
    }
}
