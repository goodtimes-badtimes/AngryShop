using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace AngryShop.Windows
{
    public partial class WindowEditCommonWords
    {
        public WindowEditCommonWords()
        {
            InitializeComponent();

            var strb = new StringBuilder();
            foreach (var commonWord in DataManager.CommonWords)
            {
                strb.AppendFormat("{0}\n", commonWord);
            }
            TextBoxCommonWords.Text = strb.ToString();
        }

        private void ButtonCancel_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void ButtonOk_OnClick(object sender, RoutedEventArgs e)
        {
            var arrStr = TextBoxCommonWords.Text.Split(new[] { '\n' });
            var listWords = new List<string>();
            for (int i = 0; i < arrStr.Length; i++)
            {
                arrStr[i] = arrStr[i].Trim('\r');
                if (!string.IsNullOrEmpty(arrStr[i]))
                    listWords.Add(arrStr[i]);
            }
            DataManager.CommonWords = listWords.ToArray();
            DataManager.SaveCommonWords();
            Close();
        }
    }
}
