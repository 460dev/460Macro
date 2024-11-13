using IdleonGamingMacro.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace IdleonGamingMacro
{
    class AppUtils
    {
        /// <summary>
        ///  exeファイルの実行場所を取得します(存在しない場合は作成します)
        /// </summary>
        public static string GetCurrentAppDir()
        {
            String path = Path.GetDirectoryName(
                System.Reflection.Assembly.GetExecutingAssembly().Location);

            return path;
        }
    }
}
