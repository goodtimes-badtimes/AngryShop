using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using AngryShop.Items.Enums;

namespace AngryShop.Helpers
{
    static class TextHelper
    {
        public static IEnumerable<string> GetListOfUniqueWords(string text)
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
                    .Select(p => new { p.Key, Count = p.Count() })
                    .Where(p => p.Count >= DataManager.Configuration.FrequencyThreshold);

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

        public static string GetNewTextForSending(string text, string oldSubstring, string newSubstring)
        {
            //text = text.Replace(string.Format("{0}", oldSubstring), string.Format(" {0} ", newSubstring));

            var regex = new Regex(string.Format(@"\b({0})\b", oldSubstring), RegexOptions.Compiled);
            text = regex.Replace(text, newSubstring);

            return text;
        }
    }
}
