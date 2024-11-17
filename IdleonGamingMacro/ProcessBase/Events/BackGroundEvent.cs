using ProcessBase.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ProcessBase.Events
{
    public static class BackGroundEvent
    {
        private const uint WM_LBUTTONDOWN = 0x0201; // マウス左ボタンダウン
        private const uint WM_LBUTTONUP = 0x0202;   // マウス左ボタンアップ

        private const uint WM_KEYDOWN = 0x0100;     // キーダウン
        private const uint WM_KEYUP = 0x0101;       // キーアップ

        public static async Task SendClickToWindowAsync(IntPtr windowHandle, int x, int y)
        {
            // バックグラウンドでクリックを送信
            await Task.Run(() =>
            {
                try
                {
                    IntPtr lParam = (IntPtr)((y << 16) | x);
                    WindowAPIHelper.SendMessage(windowHandle, WM_LBUTTONDOWN, IntPtr.Zero, lParam);
                    WindowAPIHelper.SendMessage(windowHandle, WM_LBUTTONUP, IntPtr.Zero, lParam);

                    LogControlHelper.debugLog($"[IdleonGaming] ウィンドウ '{windowHandle}' にクリックを送信しました。位置: ({x}, {y})");
                }
                catch (Exception ex)
                {
                    LogControlHelper.debugLog($"[Error] SendClickToWindowAsync 内で例外発生: {ex.Message}");
                }
            });
        }

        public static async Task SendKeyToWindowAsync(IntPtr windowHandle, char key)
        {
            await Task.Run(() =>
            {
                try
                {
                    // キーコードの取得（ASCIIコードをそのまま使用）
                    IntPtr keyCode = (IntPtr)key;

                    // キーダウンイベント送信
                    WindowAPIHelper.SendMessage(windowHandle, WM_KEYDOWN, keyCode, IntPtr.Zero);
                    // キーアップイベント送信
                    WindowAPIHelper.SendMessage(windowHandle, WM_KEYUP, keyCode, IntPtr.Zero);

                    LogControlHelper.debugLog($"[IdleonGaming] ウィンドウ '{windowHandle}' にキー '{key}' を送信しました。");
                }
                catch (Exception ex)
                {
                    LogControlHelper.debugLog($"[Error] SendKeyToWindowAsync 内で例外発生: {ex.Message}");
                }
            });
        }
    }
}
