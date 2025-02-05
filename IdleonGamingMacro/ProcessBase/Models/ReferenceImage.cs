﻿using ProcessBase.Helpers;
using OpenCvSharp;
using System.IO;

namespace ProcessBase.Models
{
    public class ReferenceImage
    {
        private readonly string _imagePath;

        public Mat Image { get; private set; } = new Mat();

        public ReferenceImage(string fileName)
        {
            _imagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images", fileName);
            LoadImage();
        }

        public ReferenceImage(byte[] imageBytes)
        {
            _imagePath = string.Empty;
            LoadImage(imageBytes);
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

        private void LoadImage(byte[] imageBytes)
        {
            if (imageBytes != null && imageBytes.Length > 0)
            {
                Image = Mat.FromImageData(imageBytes, ImreadModes.Color);
            }
            else
            {
                throw new ArgumentException("画像データが空です。");
            }
        }

        // クラスを破棄するときにリソースを解放
        public void Dispose()
        {
            Image?.Dispose();
        }
    }
}
