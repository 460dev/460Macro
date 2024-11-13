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

            Console.WriteLine("[IdleonGaming] 終了する場合はコンソールを閉じてください");

            // ウィンドウタイトルを指定してウィンドウハンドルを探す
            IntPtr windowHandle = FindWindow(null, WindowTitle);

            if (windowHandle == IntPtr.Zero)
            {
                LogControlHelper.debugLog("[IdleonGaming] Windowが見つかりませんでした。");
                return;
            }

            // ウィンドウをアクティブにする
            SetForegroundWindow(windowHandle);
            LogControlHelper.debugLog("[IdleonGaming] ウィンドウをアクティブにしました。");

            // ウィンドウの位置とサイズを設定
            MoveWindow(windowHandle, 0, 0, 800, 480, true);
            LogControlHelper.debugLog("[IdleonGaming] ウィンドウの位置とサイズを設定しました。");

            // ウィンドウの位置とサイズを取得
            if (GetWindowRect(windowHandle, out RECT bounds))
            {
                LogControlHelper.debugLog("[IdleonGaming] ウィンドウの位置とサイズを取得しました。");
                LogControlHelper.debugLog($"[IdleonGaming] x: {bounds.Left}, y: {bounds.Top}");
                LogControlHelper.debugLog($"[IdleonGaming] width: {bounds.Right - bounds.Left}, height: {bounds.Bottom - bounds.Top}");
            }
            else
            {
                LogControlHelper.debugLog("[IdleonGaming] ウィンドウの位置とサイズの取得に失敗しました。");
            }

            while (true)
            {
                #region harvest all 
                Rect targetHarvestAllRect = new Rect
                (
                    X: bounds.Left + 533,
                    Y: bounds.Top + 41,
                    Width: 81,
                    Height: 23
                );
                ComparisonImageOption harvestImageOption = new(threshold: 0.5);

                var processResult = MacroProcess(targetHarvestAllRect, HarvestAllImagePath, harvestImageOption);
                #endregion

                #region sprinkler max
                Rect targetSprinklerMaxRect = new Rect
                (
                    X: bounds.Left + 92,
                    Y: bounds.Top + 45,
                    Width: 52,
                    Height: 16
                );
                ComparisonImageOption sprinklerMaxImageOption = new(threshold: 0.5, scale: 0.8);

                var sprinklerMaxResult = MacroProcess(targetSprinklerMaxRect, SprinklerMaxImagePath, sprinklerMaxImageOption, mouseClick: false);

                if (!sprinklerMaxResult)
                {
                    #region chemical
                    Rect targetChemicalRect = new Rect(
                        X: bounds.Left + 27,
                        Y: bounds.Top + 36,
                        Width: 583,
                        Height: 364
                    );
                    ComparisonImageOption chemicalImageOption = new(threshold: 0.4, scale: 0.9);

                    var chemicalResult = MacroProcess(targetChemicalRect, ChemicalImagePath, chemicalImageOption);
                    #endregion
                }
                #endregion

                #region 2020
                Rect target2020Rect = new Rect
                (
                    X: bounds.Left + 49,
                    Y: bounds.Top + 47,
                    Width: 38,
                    Height: 14
                );
                ComparisonImageOption number2020ImageOption = new(threshold: 0.9);

                var number2020Result = MacroProcess(target2020Rect, Number2020ImagePath, number2020ImageOption, mouseClick: false);

                if (!number2020Result)
                {
                    #region sprinkler
                    Rect targetSprinklerRect = new Rect(
                        X: bounds.Left + 529,
                        Y: bounds.Top + 76,
                        Width: 68,
                        Height: 52
                    );
                    ComparisonImageOption sprinklerImageOption = new(threshold: 0.5);

                    var sprinklerResult = MacroProcess(targetSprinklerRect, SprinklerImagePath, sprinklerImageOption);
                    #endregion
                }
                #endregion

                #region squirrel
                Rect targetSquirrelRect = new Rect
                (
                    X: bounds.Left + 31,
                    Y: bounds.Top + 59,
                    Width: 575,
                    Height: 199
                );
                ComparisonImageOption squirrelImageOption = new(threshold: 0.25);

                var squirrelResult = MacroProcess(targetSquirrelRect, SquirrelImagePath, squirrelImageOption);
                #endregion

                #region shovel
                Rect targetShovelRect = new Rect
                (
                    X: bounds.Left + 527,
                    Y: bounds.Top + 326,
                    Width: 69,
                    Height: 69
                );
                ComparisonImageOption shovelImageOption = new(threshold: 0.5);

                var shovelResult = MacroProcess(targetShovelRect, ShovelNoEffectImagePath, shovelImageOption);
                #endregion

                #region dangeon cancel button
                Rect targetCancelButtonRect = new Rect
                (
                    X: bounds.Left + 26,
                    Y: bounds.Top + 267,
                    Width: 25,
                    Height: 15
                );
                ComparisonImageOption cancelButtonImageOption = new(threshold: 0.8);

                var cancelButtonResult = MacroProcess(targetCancelButtonRect, CancelBottunImagePath, cancelButtonImageOption);
                #endregion

                // 待機
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
                var centerX = targetRect.X + (resultMat.ImageRect.X + resultMat.ImageRect.Width) / 2;
                var centerY = targetRect.Y + (resultMat.ImageRect.Y + resultMat.ImageRect.Height) / 2;
                LogControlHelper.debugLog($"[IdleonGaming] x: {centerX}, y: {centerY}");

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