using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.ServiceModel.Syndication;
using System.Threading.Tasks;
using System.Xml;

namespace NetworkScreensaver
{
    public class RSSFeedReader
    {
        private readonly List<RssFeed> _feeds;
        private readonly HttpClient _httpClient;
        private readonly int _maxItemsPerFeed;
        private List<FeedItem> _items = new();

        public RSSFeedReader()
        {
            _httpClient = new HttpClient();
            _httpClient.Timeout = TimeSpan.FromSeconds(10);
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "NetworkScreensaver/1.0");
            
            var config = ConfigManager.GetConfig();
            _feeds = config.Feeds.Where(f => f.Enabled).ToList();
            _maxItemsPerFeed = config.Settings.MaxItemsPerFeed;
        }

        public async Task LoadFeedsAsync()
        {
            var allItems = new List<FeedItem>();
            
            foreach (var feed in _feeds)
            {
                try
                {
                    var items = await LoadFeedAsync(feed.Url, feed.Name);
                    allItems.AddRange(items.Take(_maxItemsPerFeed));
                }
                catch { } // Continue with other feeds if one fails
            }
            
            _items = allItems.OrderByDescending(x => x.PublishDate).ToList();
        }

        private async Task<List<FeedItem>> LoadFeedAsync(string url, string sourceName)
        {
            var items = new List<FeedItem>();
            
            try
            {
                var response = await _httpClient.GetStringAsync(url);
                using var reader = XmlReader.Create(new System.IO.StringReader(response));
                var feed = SyndicationFeed.Load(reader);
                
                foreach (var item in feed.Items)
                {
                    var title = SanitizeText(item.Title?.Text ?? "No Title");
                    var summary = SanitizeText(item.Summary?.Text ?? "");
                    
                    items.Add(new FeedItem
                    {
                        Title = title,
                        Summary = summary,
                        PublishDate = item.PublishDate.DateTime,
                        Source = sourceName
                    });
                }
            }
            catch { }
            
            return items;
        }

        public List<string> GetScrollingText()
        {
            var lines = new List<string>();
            
            foreach (var item in _items)
            {
                lines.Add($"[{item.Source}] {item.Title}");
                if (!string.IsNullOrEmpty(item.Summary))
                {
                    lines.Add($"    {item.Summary}");
                }
                lines.Add(""); // Empty line for spacing
            }
            
            return lines;
        }

        private static string SanitizeText(string text)
        {
            if (string.IsNullOrEmpty(text)) return "";
            
            // Remove HTML tags and decode entities
            text = System.Text.RegularExpressions.Regex.Replace(text, "<.*?>", "");
            text = System.Net.WebUtility.HtmlDecode(text);
            
            // Remove control characters and normalize whitespace
            text = System.Text.RegularExpressions.Regex.Replace(text, @"[\x00-\x1F\x7F]", "");
            text = System.Text.RegularExpressions.Regex.Replace(text, @"\s+", " ");
            
            return text.Trim();
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }

    public class FeedItem
    {
        public string Title { get; set; } = "";
        public string Summary { get; set; } = "";
        public DateTime PublishDate { get; set; }
        public string Source { get; set; } = "";
    }
}