using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace NetworkScreensaver
{
    public class ConfigManager
    {
        private static readonly string ConfigPath = GetConfigPath();
        private static Config _config = null!;
        
        private static string GetConfigPath()
        {
            // Try application directory first, then System32 for installed screensaver
            var appPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "rss_feeds.json");
            if (File.Exists(appPath)) return appPath;
            
            var systemPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "rss_feeds.json");
            if (File.Exists(systemPath)) return systemPath;
            
            return appPath; // Default to app directory
        }

        public static Config GetConfig()
        {
            if (_config == null)
            {
                LoadConfig();
            }
            return _config ?? new Config();
        }

        private static void LoadConfig()
        {
            try
            {
                if (File.Exists(ConfigPath))
                {
                    var json = File.ReadAllText(ConfigPath);
                    _config = JsonSerializer.Deserialize<Config>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                }
            }
            catch
            {
                // Use default config if loading fails
            }

_config = _config ?? new Config();
        }
    }

    public class Config
    {
        public List<RssFeed> Feeds { get; set; } = new()
        {
            new RssFeed { Name = "BBC News", Url = "https://feeds.bbci.co.uk/news/rss.xml", Enabled = true },
            new RssFeed { Name = "CNN", Url = "https://rss.cnn.com/rss/edition.rss", Enabled = true },
            new RssFeed { Name = "Reuters", Url = "https://feeds.reuters.com/reuters/topNews", Enabled = true }
        };

        public Settings Settings { get; set; } = new();
    }

    public class RssFeed
    {
        public string Name { get; set; } = "";
        public string Url { get; set; } = "";
        public bool Enabled { get; set; } = true;
    }

    public class Settings
    {
        public int UpdateIntervalMinutes { get; set; } = 5;
        public int ScrollSpeedMs { get; set; } = 100;
        public int MaxItemsPerFeed { get; set; } = 10;
        public bool RunOnBattery { get; set; } = false;
        public bool EnableNetworkMonitoring { get; set; } = true;
        public int NetstatIntervalSeconds { get; set; } = 30;
        public int WiresharkCaptureDurationMinutes { get; set; } = 5;
    }
}