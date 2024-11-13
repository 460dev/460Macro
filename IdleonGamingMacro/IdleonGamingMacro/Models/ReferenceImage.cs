using IdleonGamingMacro.Helpers;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IdleonGamingMacro.Models
{
    internal class ReferenceImage
    {
        private readonly string _imagePath;

        public Mat Image { get; private set; }

        public ReferenceImage(string fileName)
        {
            _imagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images", fileName);
            LoadImage();
        }

        // 画像を読み込むメソッド
        private void LoadImage()
        {
            LogControlHelper.debugLog($"[ReferenceImage] 読み込みパス: {_imagePath}");

            if (File.Exists(_imagePath))
            {
                Image = Cv2.ImRead(_imagePath, ImreadModes.Grayscale);
                LogControlHelper.debugLog($"[ReferenceImage] 画像 '{_imagePath}' を読み込みました。");
            }
            else
            {
                LogControlHelper.debugLog($"[ReferenceImage] エラー: 画像 '{_imagePath}' が見つかりません。");
            }
        }

        // クラスを破棄するときにリソースを解放
        public void Dispose()
        {
            Image?.Dispose();
        }
    }
}
