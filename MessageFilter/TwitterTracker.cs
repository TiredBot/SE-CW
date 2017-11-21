using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageFilter
{
    static class TwitterTracker
    {
        public static Dictionary<string, int> TrendingList = new Dictionary<string, int>();
        public static List<string> MentionsList = new List<string>();

        public static void CreateTrending()
        {
            foreach (string s in Tweet.HashtagsList)
            {
                if (TrendingList.ContainsKey(s))
                {
                    TrendingList[s] = TrendingList[s] + 1;
                }
                else
                {
                    TrendingList.Add(s, 1);
                }
            }
        }
    }
}
