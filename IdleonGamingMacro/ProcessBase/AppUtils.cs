using ProcessBase.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace ProcessBase
{
    class AppUtils
    {
        /// <summary>
        ///  exeファイルの実行場所を取得します(存在しない場合は作成します)
        /// </summary>
        public static string GetCurrentAppDir()
        {
            string? path = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

            // null対策としてフォールバックを用意
            if (string.IsNullOrEmpty(path))
            {
                throw new InvalidOperationException("アプリケーションディレクトリを取得できませんでした。");
            }

            return path;
        }
    }
}
