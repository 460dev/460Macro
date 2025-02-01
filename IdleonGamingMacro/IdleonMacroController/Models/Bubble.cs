using Reactive.Bindings;
using System.IO;
using System.Windows.Media.Imaging;
using static ProcessBase.Constants.Bubble;

namespace IdleonMacroController.Models
{
    public class Bubble
    {
        public BubbleColorIndex BubbleColor { get; set; }
        public BubblePageIndex BubblePage { get; set; }

        public string IconPath { get; set; }
        public ReactiveProperty<bool> IsChecked { get; set; }
        public ReactiveProperty<BitmapImage> ImageSource { get; set; } // プリロード画像

        public Bubble(string iconPath)
        {
            IconPath = iconPath;
            IsChecked = new ReactiveProperty<bool>(false);
            ImageSource = new ReactiveProperty<BitmapImage>();
        }

        public IdleonMacro.Models.Bubble GetIdleonMacroBubble()
        {
            return new IdleonMacro.Models.Bubble(
                IconPath,
                IsChecked.Value,
                BitmapImageToByteArray(ImageSource.Value),
                BubbleColor,
                BubblePage
            );
        }

        private byte[] BitmapImageToByteArray(BitmapImage bitmapImage)
        {
            using (var stream = new MemoryStream())
            {
                BitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bitmapImage));
                encoder.Save(stream);
                return stream.ToArray();
            }
        }
    }
}
