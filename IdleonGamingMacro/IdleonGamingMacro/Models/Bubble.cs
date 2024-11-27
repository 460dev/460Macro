using static ProcessBase.Constants.Bubble;

namespace IdleonMacro.Models
{
    public class Bubble
    {
        public BubbleColorIndex BubbleColor { get; set; }
        public BubblePageIndex BubblePage { get; set; }

        public string IconPath { get; set; }
        public bool IsChecked { get; set; }
        public byte[] ImageSource { get; set; } // プリロード画像

        public Bubble(string iconPath)
        {
            IconPath = iconPath;
            IsChecked = false;
            ImageSource = new byte[0];
        }

        public Bubble(string iconPath, bool isChecked, byte[] imageSource, BubbleColorIndex bubbleColor, BubblePageIndex bubblePageIndex)
        {
            IconPath = iconPath;
            IsChecked = isChecked;
            ImageSource = imageSource;
            BubbleColor = bubbleColor;
            BubblePage = bubblePageIndex;
        }
    }
}
