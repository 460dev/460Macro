using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using IdleonGamingMacro.Events;
using IdleonGamingMacro.Helpers;
using IdleonGamingMacro.Models;
using OpenCvSharp;

namespace IdleonGamingMacro
{
    class IdleonGamingMacro
    {
        [DllImport("user32.dll")]
        private static extern IntPtr FindWindow(string? lpClassName, string lpWindowName);

        public const string WindowTitle = "Legends Of Idleon";

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

            Console.WriteLine("[IdleonGaming] 終了する場合はコンソールを閉じてください。");

            IntPtr windowHandle = FindWindow(null, WindowTitle);
            if (windowHandle == IntPtr.Zero)
            {
                LogControlHelper.debugLog("[IdleonGaming] Windowが見つかりませんでした。");
                return;
            }

            var gamingHelper = new GamingHelper(windowHandle: windowHandle);

            // スタート
            var cancellationTokenSource = new CancellationTokenSource();
            CancellationToken token = cancellationTokenSource.Token;
            gamingHelper.Start(token);
        }
    }

}