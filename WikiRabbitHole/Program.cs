using HtmlAgilityPack;

namespace WikiLoopLength
{
    public class Program
    {
        private static readonly string _baseUrl = "https://en.wikipedia.org";
        private static readonly Random _random = new();

        public static void Main()
        {
            switch (LoopArticles())
            {
                case 1:
                    Console.WriteLine("Loop detected");
                    break;
                case 2:
                    Console.WriteLine("No valid links found");
                    break;
            }
        }

        public static int LoopArticles()
        {
            var client = new HtmlWeb();

            HtmlDocument doc;
            var link = "/wiki/Special:Random";

            var pages = new List<string>();

            while (true)
            {
                doc = client.Load($"{_baseUrl}{link}");
                var title = GetTitle(doc);
                pages.Add(link);

                Console.WriteLine($"{pages.Count} {title}");

                var newLinks = GetValidLinks(doc);
                var oldLink = link;
                while (oldLink.Trim() == link.Trim()
                    && pages.Contains(link))
                {
                    link = newLinks[_random.Next(0, newLinks.Count)]
                        .GetAttributeValue("href", "");
                }
                
                if (pages.Distinct().Count() != pages.Count)
                {
                    return 1;
                }
                else if (link.Length == 0)
                {
                    return 2;
                }
            }
        }

        public static string GetTitle(HtmlDocument doc)
        {
            return doc.GetElementbyId("firstHeading").InnerText;
        }

        public static List<HtmlNode> GetValidLinks(HtmlDocument doc)
        {
            var content = doc.GetElementbyId("mw-content-text");
            var links = content
                .Descendants()
                .Where(n => n.Name == "a"
                    && !n.Ancestors()
                        .Where(a => a.HasClass("infobox"))
                        .Any()
                    && n.Attributes.Contains("title")
                    && n.InnerText != "listen")
                .ToList();

            foreach (var link in new List<HtmlNode>(links))
            {
                var url = link.GetAttributeValue("href", "");
                if (!url.StartsWith("/wiki/")
                    || url.Contains(':')
                        && !url.Contains(":_"))
                {
                        links.Remove(link);
                }
            }

            return links;
        }
    }
}