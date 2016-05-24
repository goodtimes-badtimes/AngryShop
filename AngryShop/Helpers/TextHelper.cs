using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AngryShop.Items.Enums;

namespace AngryShop.Helpers
{
    static class TextHelper
    {
        public static List<string> GetListOfUniqueWords(string text)
        {
            text = removeCharacters(text);
            var words = text.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            var listWords = new List<string>();
            if (DataManager.Configuration.ToHideCommonWords)
            {
                foreach (var word in words)
                {
                    if (!DataManager.CommonWords.Contains(word))
                        listWords.Add(word);
                }
            }
            else
            {
                listWords.AddRange(words);
            }

            var selectedWords =
                listWords.GroupBy(p => p)
                    .Select(p => new { p.Key, Count = p.Count() })
                    .Where(p => p.Key.Length >= 2 && p.Count >= DataManager.Configuration.FrequencyThreshold);

            if (DataManager.Configuration.SortOrderIsAscending)
            {
                if (DataManager.Configuration.SortOrderType == SortOrderTypes.ByFrequency)
                    listWords = selectedWords.OrderBy(p => p.Count).Select(p => p.Key).ToList();
                else if (DataManager.Configuration.SortOrderType == SortOrderTypes.Alphabetical)
                    listWords = selectedWords.OrderBy(p => p.Key).Select(p => p.Key).ToList();
            }
            else if (DataManager.Configuration.SortOrderIsDescending)
            {
                if (DataManager.Configuration.SortOrderType == SortOrderTypes.ByFrequency)
                    listWords = selectedWords.OrderByDescending(p => p.Count).Select(p => p.Key).ToList();
                else if (DataManager.Configuration.SortOrderType == SortOrderTypes.Alphabetical)
                    listWords = selectedWords.OrderByDescending(p => p.Key).Select(p => p.Key).ToList();
            }

            return listWords;
        }

        private static string removeCharacters(string input)
        {
            var sb = new StringBuilder();
            foreach (char c in input)
            {
                if (char.IsLetterOrDigit(c) || c == '\'') sb.Append(c);
                else if (char.IsWhiteSpace(c)) sb.Append(' ');
            }

            return sb.ToString();
        }
    }
}
