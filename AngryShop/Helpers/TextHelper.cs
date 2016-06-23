using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using AngryShop.Entities;
using AngryShop.Items.Enums;

namespace AngryShop.Helpers
{
    /// <summary>
    /// Helper class for all text operations custom logic
    /// </summary>
    static class TextHelper
    {
        /// <summary> Returns a list of unique substrings (words) from the whole text string </summary>
        public static IEnumerable<ListItemWord> GetListOfUniqueWords(string text)
        {
            var regex = new Regex(@"(\b\w{2,}\b)", RegexOptions.Compiled);
            var words = regex.Matches(text);
            var listWords = new List<string>();
            if (DataManager.Configuration.ToHideIgnoredWords)
            {
                listWords.AddRange(from Match match in words where !DataManager.IgnoredWords.Contains(match.Value) select match.Value);
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

        /// <summary> Replaces substrings in text with new ones and returns new whole text string </summary>
        public static string GetNewTextForSending(string text, IEnumerable<ListItemWord> items)
        {
            foreach (var listItemWord in items)
            {
                var regex = new Regex(string.Format(@"\b({0})\b", listItemWord.Word), RegexOptions.Compiled);
                text = regex.Replace(text, listItemWord.WordEdited);
                listItemWord.Word = listItemWord.WordEdited;
                listItemWord.EditorVisibility = Visibility.Collapsed;
            }
            return text;
        }
    }
}
