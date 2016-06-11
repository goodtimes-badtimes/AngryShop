using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using AngryShop.Items.Enums;

namespace AngryShop.Helpers
{
    class ListItemWord : INotifyPropertyChanged
    {
        private string _word;
        public string Word { get { return _word; } set { _word = value; OnPropertyChanged("Word"); } }
        public string WordEdited { get; set; }
        public int Count { get; set; }

        private Visibility _visibilityEdited = Visibility.Collapsed;
        public Visibility VisibilityEdited { get { return _visibilityEdited; } set { _visibilityEdited = value; OnPropertyChanged("VisibilityEdited"); } }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;

            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
    static class TextHelper
    {

        public static IEnumerable<ListItemWord> GetListOfUniqueWords(string text)
        {
            var regex = new Regex(@"(\b\w{2,}\b)", RegexOptions.Compiled);
            var words = regex.Matches(text);
            var listWords = new List<string>();
            if (DataManager.Configuration.ToHideCommonWords)
            {
                foreach (Match match in words)
                {
                    if (!DataManager.CommonWords.Contains(match.Value))
                        listWords.Add(match.Value);
                }
            }
            else
            {
                listWords.AddRange(from Match match in words select match.Value);
            }

            var selectedWords =
                listWords.GroupBy(p => p)
                    .Select(p => new ListItemWord { Word = p.Key, WordEdited = p.Key, Count = p.Count() })
                    .Where(p => p.Count >= DataManager.Configuration.FrequencyThreshold);

            if (DataManager.Configuration.SortOrderIsAscending)
            {
                if (DataManager.Configuration.SortOrderType == SortOrderTypes.ByFrequency)
                    selectedWords = selectedWords.OrderBy(p => p.Count);
                else if (DataManager.Configuration.SortOrderType == SortOrderTypes.Alphabetical)
                    selectedWords = selectedWords.OrderBy(p => p.Word);
            }
            else if (DataManager.Configuration.SortOrderIsDescending)
            {
                if (DataManager.Configuration.SortOrderType == SortOrderTypes.ByFrequency)
                    selectedWords = selectedWords.OrderByDescending(p => p.Count);
                else if (DataManager.Configuration.SortOrderType == SortOrderTypes.Alphabetical)
                    selectedWords = selectedWords.OrderByDescending(p => p.Word);
            }

            return selectedWords.ToList();
        }

        public static string GetNewTextForSending(string text, List<ListItemWord> items)
        {
            //text = text.Replace(string.Format("{0}", oldSubstring), string.Format(" {0} ", newSubstring));
            foreach (var listItemWord in items)
            {
                var regex = new Regex(string.Format(@"\b({0})\b", listItemWord.Word), RegexOptions.Compiled);
                text = regex.Replace(text, listItemWord.WordEdited);
                listItemWord.Word = listItemWord.WordEdited;
                listItemWord.VisibilityEdited = Visibility.Collapsed;
            }
            return text;
        }
    }
}
