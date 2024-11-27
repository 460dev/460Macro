using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace IdleonMacroController.Views.Bubble
{
    /// <summary>
    /// Interaction logic for Bubble
    /// </summary>
    public partial class Bubble : UserControl
    {
        public Bubble()
        {
            InitializeComponent();
        }

        // PreviewTextInput イベントハンドラー: リアルタイムで入力を制御
        private void UpgradeTimeMinTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // 数字のみ許可する正規表現
            e.Handled = !Regex.IsMatch(e.Text, "^[0-9]+$");
        }

        // Pasting イベントハンドラー: ペースト時の入力を制御
        private void UpgradeTimeMinTextBox_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(DataFormats.Text))
            {
                string text = (string)e.DataObject.GetData(DataFormats.Text);

                // 数字以外の場合はキャンセル
                if (!Regex.IsMatch(text, "^[0-9]+$"))
                {
                    e.CancelCommand();
                }
            }
            else
            {
                e.CancelCommand();
            }
        }

        // TextChanged イベントハンドラー: 範囲外の値を修正
        private void UpgradeTimeMinTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox textBox && !string.IsNullOrEmpty(textBox.Text))
            {
                // 現在の入力値を数値に変換
                if (int.TryParse(textBox.Text, out int value))
                {
                    // 最小値1、最大値60に制限
                    if (value < 1)
                    {
                        textBox.Text = "1";
                    }
                    else if (value > 60)
                    {
                        textBox.Text = "60";
                    }

                    // キャレット位置を末尾に設定
                    textBox.CaretIndex = textBox.Text.Length;
                }
            }
        }

        // 入力値の検証
        private bool IsValidInput(string input)
        {
            // 数値であり、1以上60以下であれば有効
            return int.TryParse(input, out int value) && value >= 1 && value <= 60;
        }
    }
}
