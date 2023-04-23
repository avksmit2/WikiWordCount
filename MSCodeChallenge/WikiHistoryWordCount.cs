using HtmlAgilityPack;
using System.Linq;

namespace WikiHistoryWordCount
{
    class Program 
    {
        public static void Main()
        {
            string url = "https://en.wikipedia.org/wiki/Microsoft#History";
            int wordCount = 10;
            List<string> excludeWords = new List<string>();

            try
            {            
                //Get HTML from page
                var urlHtml = CallUrl(url).Result;
                //Get innerText for p tags within history section of page
                string htmlWords = ParseHtml(urlHtml);
                //count individual words with wordCount and any excludeWords
                var wordList = countWords(htmlWords, wordCount, excludeWords);
            }

            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

        }

        private static async Task<string> CallUrl(string fullUrl)
        {
            HttpClient client = new HttpClient();
            var response = await client.GetStringAsync(fullUrl);
            return response;
        }

        private static string ParseHtml(string urlHtml)
        {
            string innerWords = "";

            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(urlHtml);
            
            //Determine starting and ending position of history section
            int startingPosition = htmlDoc.DocumentNode.SelectSingleNode("//*[@id='History']").Line;
            int endingPosition = htmlDoc.DocumentNode.SelectSingleNode("//*[@id='Corporate_affairs']").Line;

            //Should only look at nodes within history section that have p tags (specs didn't say whether links or headings should be included so just assumed they shouldn't)
            var nodes = htmlDoc.DocumentNode.Descendants("p")
                .Where(x => x.Line > startingPosition && x.Line < endingPosition);

            foreach (HtmlNode aNode in nodes)
            {
                innerWords += aNode.InnerText;
            }

            return innerWords;
        }

        private static List<KeyValuePair<string, int>> countWords(string words, int maxWords = 0, List<string>? excludeWords = null)
        {
            //Remove anything that is not a letter or space from words
            char[] cleanWords = words.Where(c => (char.IsLetter(c) || char.IsWhiteSpace(c))).ToArray();
            string actualWords = new string(cleanWords);
                        
            var delimiterChars = new char[] { ' ', ',', ':', ';', '(', ')', '\t', '\"', '\r', '{', '}', '[', ']', '=', '/' };
            var parsedWordsList = actualWords
                .Trim()
                .Split(delimiterChars)
                .Where(x => x.Length > 0)
                .Select(x => x.ToLower())
                .GroupBy(x => x)
                .Select(x => new { Word = x.Key, Count = x.Count() })
                .OrderByDescending(x => x.Count)
                .Take(maxWords)
                .ToDictionary(x => x.Word, x => x.Count);

            if (excludeWords != null && excludeWords.Count > 0)
            {
                foreach (var key in parsedWordsList.Keys.Except(excludeWords).ToList())
                {
                    parsedWordsList.Remove(key);
                }
            }

            return parsedWordsList.ToList();
        }
    }


}