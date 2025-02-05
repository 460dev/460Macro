﻿using ProcessBase.Events;
using ProcessBase.Models;
using OpenCvSharp;
using NLog.Config;
using System.Diagnostics;

#pragma warning disable CA1416

namespace ProcessBase.Helpers
{
    public class OpenCVSharpHelper
    {
        public enum CheckBorderType
        {
            None = 0,
            In = 1,
            Out = 2,
        }

        public ImageResult CheckImage(Rect borderRect, Rect targetRect, ReferenceImage referenceImage, ComparisonImageOption imageOption, CheckBorderType checkBorderType = CheckBorderType.None)
        {
            ImageResult imageResult = new()
            {
                Status = false
            };

            CroppedImage croppedImage = new(targetRect.X, targetRect.Y, targetRect.Width, targetRect.Height);

            var resultMat = ComparisonImage(referenceImage.Image, croppedImage.Image, imageOption);

            if (resultMat.Status)
            {
                // 中心座標を計算
                LogControlHelper.debugLog("[IdleonGaming] 対象の画像が見つかりました。");
                imageResult.X = targetRect.X + (resultMat.ImageRect.X + (resultMat.ImageRect.Width / 2));
                imageResult.Y = targetRect.Y + (resultMat.ImageRect.Y + (resultMat.ImageRect.Height / 2));
                imageResult.OverlayX = targetRect.X + resultMat.ImageRect.X;
                imageResult.OverlayY = targetRect.Y + resultMat.ImageRect.Y;
                imageResult.Width = resultMat.ImageRect.Width;
                imageResult.Height = resultMat.ImageRect.Height;

                LogControlHelper.debugLog($"[IdleonGaming] x: {imageResult.X}, y: {imageResult.Y}");
                if (!CheckBorder(borderRect, imageResult, checkBorderType))
                {
                    return imageResult;
                }

                imageResult.Status = true;
            }
            else
            {
                LogControlHelper.debugLog("[IdleonGaming] 対象の画像が見つかりませんでした。");
            }

            return imageResult;
        }

        public ImageStatus ComparisonImage(Mat templateMat, Mat targetMat, ComparisonImageOption imageOption)
        {
            var imageStatus = new ImageStatus();

            // スケーリング適用後のターゲット画像とテンプレート画像を作成
            Mat scaledTarget = new Mat();
            Mat scaledTemplate = new Mat();

            // スケール適用時にサイズとチャンネル数をデバッグ出力
            LogControlHelper.debugLog($"[Debug] Initial Target Image Size: {targetMat.Width}x{targetMat.Height}, Channels: {targetMat.Channels()}");
            LogControlHelper.debugLog($"[Debug] Template Image Size: {templateMat.Width}x{templateMat.Height}, Channels: {templateMat.Channels()}");

            // グレースケール変換でチャンネルを一致させる
            if (targetMat.Channels() > 1)
            {
                Cv2.CvtColor(targetMat, targetMat, ColorConversionCodes.BGR2GRAY);
                LogControlHelper.debugLog("[Debug] Converted Target Image to Grayscale.");
            }
            if (templateMat.Channels() > 1)
            {
                Cv2.CvtColor(templateMat, templateMat, ColorConversionCodes.BGR2GRAY);
                LogControlHelper.debugLog("[Debug] Converted Template Image to Grayscale.");
            }

            // スケールが1.0未満の場合、縮小
            if (imageOption.Scale < 1.0)
            {
                Cv2.Resize(targetMat, scaledTarget, new Size(), imageOption.Scale, imageOption.Scale, InterpolationFlags.Linear);
                Cv2.Resize(templateMat, scaledTemplate, new Size(), imageOption.Scale, imageOption.Scale, InterpolationFlags.Linear);
                LogControlHelper.debugLog($"[Debug] Scaled Target Image Size: {scaledTarget.Width}x{scaledTarget.Height}");
                LogControlHelper.debugLog($"[Debug] Scaled Template Image Size: {scaledTemplate.Width}x{scaledTemplate.Height}");
            }
            else
            {
                // スケーリング不要の場合、元の画像を使用
                scaledTarget = targetMat;
                scaledTemplate = templateMat;
            }

            // サイズ確認：scaledTargetがscaledTemplateよりも大きいかどうか
            if (scaledTarget.Width < scaledTemplate.Width || scaledTarget.Height < scaledTemplate.Height)
            {
                LogControlHelper.debugLog("[Error] Template image is larger than the target image.");
                return imageStatus;
            }

            // --- 前処理: ぼかしとエッジ検出 ---
            // ガウシアンブラーでノイズ除去
            if (imageOption.NoiseCancellation)
            {
                Cv2.GaussianBlur(scaledTarget, scaledTarget, new Size(7, 7), 0);
                Cv2.GaussianBlur(scaledTemplate, scaledTemplate, new Size(7, 7), 0);
                LogControlHelper.debugLog("[Debug] Applied GaussianBlur to remove noise.");
            }

            // Cannyエッジ検出を適用して輪郭を抽出
            if (imageOption.Edge)
            {
                Cv2.Canny(scaledTarget, scaledTarget, 20, 70);
                Cv2.Canny(scaledTemplate, scaledTemplate, 20, 70);
                LogControlHelper.debugLog("[Debug] Applied Canny edge detection to highlight contours.");

                //// デバッグ出力
                //Cv2.ImWrite("scaled_target.png", scaledTarget);
                //Cv2.ImWrite("scaled_template.png", scaledTemplate);
            }

            // テンプレートマッチングを実行し、結果を resultMat に格納
            using (Mat resultMat = new Mat())
            {
                Cv2.MatchTemplate(scaledTarget, scaledTemplate, resultMat, TemplateMatchModes.CCoeffNormed);

                // MinMaxLoc で resultMat から一致位置を取得
                OpenCvSharp.Point minloc, maxloc;
                double minval, maxval;
                Cv2.MinMaxLoc(resultMat, out minval, out maxval, out minloc, out maxloc);

                LogControlHelper.debugLog($"[Debug] Min Value: {minval}, Max Value: {maxval}");

                if (maxval >= imageOption.Threshold)
                {
                    LogControlHelper.debugLog("[IdleonGaming] Matched!!");

                    // 検出された位置を元のサイズに戻して計算
                    int originalX = (int)(maxloc.X / imageOption.Scale);
                    int originalY = (int)(maxloc.Y / imageOption.Scale);

                    // 検出された位置に基づいて座標を取得
                    Rect matchRect = new Rect(originalX, originalY, templateMat.Width, templateMat.Height);
                    imageStatus.ImageRect = matchRect;
                    imageStatus.Status = true;

                    // ログ出力
                    LogControlHelper.debugLog($"[IdleonGaming] Match Rectangle at Original Size: {imageStatus.ImageRect}");
                }
                else
                {
                    LogControlHelper.debugLog("[IdleonGaming] No Match Found.");
                    imageStatus.Status = false;
                }
            }

            return imageStatus;
        }

        private bool CheckBorder(Rect borderRect, ImageResult imageResult, CheckBorderType checkBorderType = CheckBorderType.None)
        {
            switch (checkBorderType)
            {
                case CheckBorderType.None:
                    return true;
                case CheckBorderType.In:
                    // 境界内の場合の条件
                    if (imageResult.X < borderRect.Left || imageResult.X > borderRect.Right)
                    {
                        return false;
                    }

                    if (imageResult.Y < borderRect.Top || imageResult.Y > borderRect.Bottom)
                    {
                        return false;
                    }

                    return true;
                case CheckBorderType.Out:
                    // 境界外の場合の条件
                    if (imageResult.X >= borderRect.Left && imageResult.X <= borderRect.Right &&
                        imageResult.Y >= borderRect.Top && imageResult.Y <= borderRect.Bottom)
                    {
                        return false;
                    }

                    return true;
                default:
                    return true;

            }
        }
    }
}

#pragma warning restore CA1416