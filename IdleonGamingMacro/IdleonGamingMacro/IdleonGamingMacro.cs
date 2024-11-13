using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using IdleonGamingMacro.Events;
using IdleonGamingMacro.Helpers;
using IdleonGamingMacro.Models;
using OpenCvSharp;

namespace IdleonGamingMacro
{
    class IdleonGamingMacro
    {
        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern IntPtr FindWindow(string? lpClassName, string lpWindowName);

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll")]
        private static extern bool MoveWindow(IntPtr hWnd, int x, int y, int nWidth, int nHeight, bool bRepaint);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        private const uint WM_LBUTTONDOWN = 0x0201; // マウス左ボタンダウン
        private const uint WM_LBUTTONUP = 0x0202;   // マウス左ボタンアップ

        public const string WindowTitle = "Legends Of Idleon"; // 定数

        private const string HarvestAllImagePath = "harvest_all.png";
        private const string SprinklerMaxImagePath = "sprinkler_max_transparent.png";
        //private const string ChemicalImagePath = "chemical_transparent.png";
        private const string ChemicalImagePath = "chemical.png";
        private const string Number2020ImagePath = "20_20.png";
        private const string SprinklerImagePath = "sprinkler.png";
        private const string ShovelNoEffectImagePath = "shovel_no_effect.png";
        private const string SquirrelImagePath = "squirrel_transparent.png";
        private const string CancelBottunImagePath = "cancel_button.png";

        private const int gameX = 6;
        private const int gameY = 37;
        private const int gamingWidth = 600;
        private const int gamingHeight = 400;

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        static void Main()
        {
            // ログファイルを整理する
            new LogControlHelper().Run();

            IdleonGaming();
        }

        static void IdleonGaming()
        {
            Console.WriteLine("[IdleonGaming] IdleonGaming開始");
            LogControlHelper.infoLog("[IdleonGaming] IdleonGaming開始");

            IntPtr windowHandle = FindWindow(null, WindowTitle);
            if (windowHandle == IntPtr.Zero)
            {
                LogControlHelper.debugLog("[IdleonGaming] Windowが見つかりませんでした。");
                return;
            }

            SetForegroundWindow(windowHandle);
            MoveWindow(windowHandle, 0, 0, 800, 480, true);
            GetWindowRect(windowHandle, out RECT bounds);

            while (true)
            {
                ExecuteMacroProcess(bounds, gameX, gameY, gamingWidth, gamingHeight, HarvestAllImagePath, 0.7);           // Harvest all
                bool sprinklerMaxResult = ExecuteMacroProcess(bounds, 0, 0, 800, 480, SprinklerMaxImagePath, 0.7, scale: 0.8, mouseClick: false); // Sprinkler max

                if (!sprinklerMaxResult)
                {
                    ExecuteMacroProcess(bounds, gameX, gameY, gamingWidth, gamingHeight, ChemicalImagePath, 0.4, scale: 0.9); // Chemical
                }

                bool number2020Result = ExecuteMacroProcess(bounds, gameX, gameY, gamingWidth, gamingHeight, Number2020ImagePath, 0.9, mouseClick: false); // 2020 number

                if (!number2020Result)
                {
                    ExecuteMacroProcess(bounds, gameX, gameY, gamingWidth, gamingHeight, SprinklerImagePath, 0.7); // Sprinkler
                }

                ExecuteMacroProcess(bounds, gameX, gameY, gamingWidth, gamingHeight, SquirrelImagePath, 0.5);     // Squirrel
                ExecuteMacroProcess(bounds, 527, 326, 69, 69, ShovelNoEffectImagePath, 0.5); // Shovel
                ExecuteMacroProcess(bounds, gameX, gameY, gamingWidth, gamingHeight, CancelBottunImagePath, 0.8);   // Cancel button

                LogControlHelper.debugLog("[IdleonGaming] 0.1秒待機");
                Thread.Sleep(100);
            }
        }

        private static bool MacroProcess(Rect targetRect, string imagePath, ComparisonImageOption imageOption, bool mouseClick = true)
        {
            ReferenceImage referenceImage = new(imagePath);
            CroppedImage croppedImage = new(targetRect.X, targetRect.Y, targetRect.Width, targetRect.Height);

            var openCVSharpHelper = new OpenCVSharpHelper();
            var resultMat = openCVSharpHelper.ComparisonImage(referenceImage.Image, croppedImage.Image, imageOption);

            if (resultMat.Status)
            {
                // 中心座標を計算
                LogControlHelper.debugLog("[IdleonGaming] " + imagePath + " が見つかりました。");
                var centerX = targetRect.X + (resultMat.ImageRect.X + (resultMat.ImageRect.Width / 2));
                var centerY = targetRect.Y + (resultMat.ImageRect.Y + (resultMat.ImageRect.Height / 2));
                LogControlHelper.debugLog($"[IdleonGaming] x: {centerX}, y: {centerY}");

                // 左下をクリックしないようにする
                if (centerX >= 30 && centerX <= 135 && centerY >= 310)
                {
                    return false;
                }

                // クリック有効だったら
                if (mouseClick)
                {
                    // 座標をクリックする
                    //MouseClicker.ClickAt(centerX, centerY);

                    // バックグラウンドでクリックする
                    SendClickToWindowAsync(WindowTitle, centerX, centerY).ConfigureAwait(true);
                    LogControlHelper.debugLog($"[IdleonGaming] 座標 ({centerX}, {centerY}) をクリックしました。");
                }

                return true;
            }
            else
            {
                LogControlHelper.debugLog("[IdleonGaming] " + imagePath + " が見つかりませんでした。");

                return false;
            }
        }

        private static bool ExecuteMacroProcess(RECT bounds, int offsetX, int offsetY, int width, int height, string imagePath, double threshold, double scale = 1.0, bool mouseClick = true)
        {
            Rect targetRect = new Rect(
                X: bounds.Left + offsetX,
                Y: bounds.Top + offsetY,
                Width: width,
                Height: height
            );

            ComparisonImageOption imageOption = new(threshold: threshold, scale: scale);
            return MacroProcess(targetRect, imagePath, imageOption, mouseClick);
        }

        // 特定のウィンドウにクリックイベントを送るメソッド
        private static async Task SendClickToWindowAsync(string windowTitle, int x, int y)
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
                // ウィンドウを前面に持ってくる（必須ではありません）
                //SetForegroundWindow(hWnd);

                // クリック位置をパラメータに変換
                IntPtr lParam = (IntPtr)((y << 16) | x);

                // マウス左ボタンダウンとアップメッセージを送信
                SendMessage(hWnd, WM_LBUTTONDOWN, IntPtr.Zero, lParam);
                SendMessage(hWnd, WM_LBUTTONUP, IntPtr.Zero, lParam);

                LogControlHelper.debugLog($"[IdleonGaming] ウィンドウ '{windowTitle}' にクリックを送信しました。位置: ({x}, {y})");
            });
        }

    }

}