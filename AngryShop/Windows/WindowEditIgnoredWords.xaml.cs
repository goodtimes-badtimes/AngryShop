using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace AngryShop.Windows
{
    public partial class WindowEditIgnoredWords
    {
        public WindowEditIgnoredWords()
        {
            InitializeComponent();

            var strb = new StringBuilder();
            foreach (var word in DataManager.IgnoredWords)
            {
                strb.AppendFormat("{0}\n", word);
            }
            TextBoxIgnoredWords.Text = strb.ToString();
        }

        private void ButtonCancel_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void ButtonOk_OnClick(object sender, RoutedEventArgs e)
        {
            var arrStr = TextBoxIgnoredWords.Text.Split(new[] { '\n' });
            var listWords = new List<string>();
            for (int i = 0; i < arrStr.Length; i++)
            {
                arrStr[i] = arrStr[i].Trim('\r');
                if (!string.IsNullOrEmpty(arrStr[i]))
                    listWords.Add(arrStr[i]);
            }
            DataManager.IgnoredWords = listWords;
            DataManager.SaveIgnoredWords();
            Close();
        }
    }
}
