using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;
using System.Diagnostics;
using NLog;
using NLog.Config;
using NLog.Targets;

namespace IdleonGamingMacro.Helpers
{
    /// <summary>
    /// logファイルを操作するクラス
    /// </summary>
    public class LogControlHelper
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private static string _appVersion = "";

        public LogControlHelper()
        {
            // exeファイルの実行場所を取得し、設定する
            Directory.SetCurrentDirectory(AppUtils.GetCurrentAppDir());

            //// LoggingConfigurationを生成 
            //var config = new LoggingConfiguration();

            //// FileTargetを生成し LoggingConfigurationに設定 
            //var fileTarget = new FileTarget();
            //config.AddTarget("file", fileTarget);

            //// fileTargetのプロパティを設定
            //fileTarget.Name = "f";
            //fileTarget.FileName = "${basedir}/log/${shortdate}.log";
            //fileTarget.Layout = "${longdate} [${uppercase:${level}}] ${message}";

            //// LoggingRuleを定義
            //var rule1 = new LoggingRule("*", LogLevel.Debug, fileTarget);
            //config.LoggingRules.Add(rule1);

            //// 設定を有効化
            //LogManager.Configuration = config;
        }

        /// <summary>
        /// エラーログを出力します
        /// </summary>
        /// <param name="message"></param>
        /// <param name="memberName"></param>
        /// <param name="sourceLineNumber"></param>
        public static void errorLog(String message,
            [System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
            [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0)
        {
            //一つ前のスタックを取得
            StackFrame callerFrame = new StackFrame(1);
            //クラス名
            string className = callerFrame.GetMethod().ReflectedType.FullName;

            logger.Error("[version : " + _appVersion + "][" + className + ":L" + sourceLineNumber + ", " + memberName + "] " + message);
        }

        /// <summary>
        /// インフォログを出力します
        /// </summary>
        /// <param name="message"></param>
        /// <param name="memberName"></param>
        /// <param name="sourceLineNumber"></param>
        public static void infoLog(String message,
            [System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
            [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0)
        {
            //一つ前のスタックを取得
            StackFrame callerFrame = new StackFrame(1);
            //クラス名
            string className = callerFrame.GetMethod().ReflectedType.FullName;

            logger.Info("[version : " + _appVersion + "][" + className + ":L" + sourceLineNumber + ", " + memberName + "] " + message);
        }

        /// <summary>
        /// デバッグログを出力します
        /// </summary>
        /// <param name="message"></param>
        /// <param name="memberName"></param>
        /// <param name="sourceLineNumber"></param>
        public static void debugLog(String message,
            [System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
            [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0)
        {
#if DEBUG
            //一つ前のスタックを取得
            StackFrame callerFrame = new StackFrame(1);
            //クラス名
            string className = callerFrame.GetMethod().ReflectedType.FullName;

            logger.Debug("[version : " + _appVersion + "][" + className + ":L" + sourceLineNumber + ", " + memberName + "] " + message);
#endif
        }

        /// <summary>
        /// logファイルを移動し、削除する
        /// </summary>
        public void Run()
        {
            Move();
            Delete();
        }

        // logフォルダ内の1週間前に作成されたlogファイルをoldフォルダへ移動する
        private void Move()
        {
            // フォルダが存在しない場合、作成する
            if (!Directory.Exists(@"log\old"))
            {
                Directory.CreateDirectory(@"log\old");
            }

            // logフォルダ内のlogファイルを取得する
            List<string> logFileNames = Search("log");

            foreach (string logFileName in logFileNames)
            {
                // 日数間隔を取得する
                int days = Days(logFileName);

                // 1週間より前に作成されたログファイルをoldフォルダへ移動
                if (days >= 7)
                {
                    File.Move(logFileName, @"log\old\" + logFileName.Replace(@"log\", ""));
                }
            }
        }

        // oldフォルダ内の2週間前に作成されたlogファイルを削除する
        private void Delete()
        {
            // oldフォルダ内のlogファイルを取得する
            List<string> logFileNames = Search(@"log\old");

            foreach (string logFileName in logFileNames)
            {
                // 日数間隔を取得する
                int days = Days(logFileName);

                // 2週間より前に作成されたログファイルを削除する
                if (days >= 14)
                {
                    File.Delete(logFileName);
                }
            }
        }

        /// <summary>
        /// logファイルを取得する
        /// </summary>
        /// <param name="dName">フォルダ名</param>
        /// <returns>logファイル一覧</returns>
        private List<string> Search(string dName)
        {
            // ファイル名格納用のListを宣言する
            List<string> logFileNames = new List<string>();

            // 指定したフォルダ内のファイル名を取得する
            logFileNames.AddRange(Directory.GetFiles(dName)
                        .Where(fName => Regex.IsMatch(fName, "[0-9]{8}.log"))
                        .ToArray());

            return logFileNames;
        }

        /// <summary>
        /// 日数間隔を計算し、取得する
        /// </summary>
        /// <param name="logFileName">ファイル名</param>
        /// <returns>日数間隔</returns>
        private int Days(string logFileName)
        {
            // ファイル名から日付部分を切り取る
            string strLogDate = Regex.Match(logFileName, "[0-9]{8}").Value; ;

            // フォーマットを指定してDateTimeに変換する
            DateTime logDateTime = DateTime.ParseExact(strLogDate, "yyyyMMdd",
                System.Globalization.DateTimeFormatInfo.InvariantInfo,
                System.Globalization.DateTimeStyles.NoCurrentDateDefault);

            // 日数間隔を計算する
            TimeSpan span = DateTime.Now - logDateTime;

            return span.Days;
        }

        /// <summary>
        /// アプリのバージョンを取得します
        /// </summary>
        public static void GetAppVersion()
        {
            //自分自身のAssemblyを取得
            System.Reflection.Assembly asm = System.Reflection.Assembly.GetExecutingAssembly();
            //バージョンの取得
            System.Version ver = asm.GetName().Version;
            _appVersion = ver.ToString();
        }
    }
}
