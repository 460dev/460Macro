using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace IdleonGamingMacro.Events
{
    internal class MouseClicker
    {
        // マウスイベント用の定数
        private const int MOUSEEVENTF_LEFTDOWN = 0x02;
        private const int MOUSEEVENTF_LEFTUP = 0x04;

        [DllImport("user32.dll")]
        private static extern bool SetCursorPos(int X, int Y);

        [DllImport("user32.dll")]
        private static extern void mouse_event(int dwFlags, int dx, int dy, int dwData, int dwExtraInfo);

        // 指定座標をクリックするメソッド
        public static void ClickAt(int x, int y)
        {
            // カーソルを指定位置に移動
            SetCursorPos(x, y);

            // マウスの左クリック操作をシミュレート
            mouse_event(MOUSEEVENTF_LEFTDOWN, x, y, 0, 0);
            mouse_event(MOUSEEVENTF_LEFTUP, x, y, 0, 0);
        }
    }
}
