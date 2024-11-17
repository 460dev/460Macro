using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using ProcessBase.Events;
using ProcessBase.Helpers;
using ProcessBase.Models;
using OpenCvSharp;

namespace ProcessBase
{
    class IdleonGamingMacro
    {
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

            IntPtr windowHandle = WindowAPIHelper.FindWindow(null, WindowTitle);
            if (windowHandle == IntPtr.Zero)
            {
                LogControlHelper.debugLog("[IdleonGaming] Windowが見つかりませんでした。");
                return;
            }

            var gamingHelper = new GamingHelper(windowHandle: windowHandle);

            // スタート
            var cancellationTokenSource = new CancellationTokenSource();
            CancellationToken token = cancellationTokenSource.Token;
            gamingHelper.Start(token, isOverlay: true);
        }
    }

}