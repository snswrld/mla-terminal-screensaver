using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NetworkScreensaver
{
    public partial class ScreensaverForm : Form
    {
        private readonly RSSFeedReader _rssReader;
        private readonly NetworkMonitor _networkMonitor;
        private readonly Timer _scrollTimer;
        private readonly Timer _feedUpdateTimer;
        private List<string> _scrollingLines = new();
        private int _currentLineIndex = 0;
        private int _displayedLines = 0;
        private readonly Font _terminalFont;

        public ScreensaverForm()
        {
            try
            {
                InitializeComponent();
                
                var config = ConfigManager.GetConfig();
                
                // Check if we should run (not on battery)
                if (PowerManager.IsOnBatteryPower() && !config.Settings.RunOnBattery)
                {
                    Close();
                    return;
                }
            
            _rssReader = new RSSFeedReader();
            _networkMonitor = new NetworkMonitor();
            
            // Setup terminal-style appearance
            BackColor = Color.Black;
            ForeColor = Color.Lime;
            FormBorderStyle = FormBorderStyle.None;
            WindowState = FormWindowState.Maximized;
            TopMost = true;
            Cursor = Cursors.Default;
            
            _terminalFont = new Font("Consolas", 12, FontStyle.Regular);
            Font = _terminalFont;
            
            // Calculate how many lines fit on screen (with safety check)
            _displayedLines = _terminalFont != null && _terminalFont.Height > 0 ? Height / _terminalFont.Height - 2 : 20;
            
            // Setup timers
            _scrollTimer = new Timer { Interval = config.Settings.ScrollSpeedMs };
            _scrollTimer.Tick += ScrollTimer_Tick;
            
            _feedUpdateTimer = new Timer { Interval = config.Settings.UpdateIntervalMinutes * 60000 };
            _feedUpdateTimer.Tick += async (s, e) => await LoadFeedsAsync();
            
            // Start monitoring and load initial data
            _ = Task.Run(async () =>
            {
                try
                {
                    if (config.Settings.EnableNetworkMonitoring)
                    {
                        await _networkMonitor.StartMonitoringAsync();
                    }
                }
                catch
                {
                    // Continue without network monitoring if it fails
                }
                
                await LoadFeedsAsync();
                
                Invoke(() =>
                {
                    _scrollTimer.Start();
                    _feedUpdateTimer.Start();
                });
            });
            
            // Handle input to exit
            KeyPreview = true;
            KeyDown += OnKeyDown;
            MouseMove += OnMouseMove;
            MouseClick += (s, e) => GracefulClose();
            }
            catch (Exception ex)
            {
                // Log error and show basic screensaver
                try
                {
                    var logDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "NetworkScreensaverLogs");
                    Directory.CreateDirectory(logDir);
                    File.WriteAllText(Path.Combine(logDir, $"error_{DateTime.Now:yyyyMMdd_HHmmss}.txt"), ex.ToString());
                }
                catch { }
                
                // Show minimal screensaver
                BackColor = Color.Black;
                ForeColor = Color.Lime;
                FormBorderStyle = FormBorderStyle.None;
                WindowState = FormWindowState.Maximized;
                TopMost = true;
                
                KeyPreview = true;
                KeyDown += (s, e) => Close();
                MouseMove += (s, e) => Close();
                MouseClick += (s, e) => Close();
            }
        }

        private Point _lastMousePosition = Point.Empty;
        
        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                GracefulClose();
            }
            else
            {
                GracefulClose();
            }
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (_lastMousePosition != Point.Empty)
            {
                if (Math.Abs(e.X - _lastMousePosition.X) > 5 || Math.Abs(e.Y - _lastMousePosition.Y) > 5)
                {
                    GracefulClose();
                }
            }
            _lastMousePosition = e.Location;
        }

        private void GracefulClose()
        {
            try
            {
                _scrollTimer?.Stop();
                _feedUpdateTimer?.Stop();
                
                // Get network connections before stopping monitoring
                var connections = _networkMonitor?.GetActiveConnections() ?? new List<NetworkConnection>();
                
                _networkMonitor?.StopMonitoring();
                
                // Log all connections to file
                if (connections.Any())
                {
                    LogConnectionsToFile(connections);
                    ShowNetworkAlert(connections);
                }
            }
            catch { }
            finally
            {
                Close();
            }
        }

        private void ShowNetworkAlert(List<NetworkConnection> connections)
        {
            var rssHosts = GetRSSHosts();
            var suspiciousConnections = connections.Where(c => 
                !rssHosts.Contains(c.RemoteAddress) && 
                !IsLocalOrSystemConnection(c)).ToList();
            
            if (suspiciousConnections.Any())
            {
                var message = "Network connections detected while screen was locked:\n\n";
                foreach (var conn in suspiciousConnections.Take(10))
                {
                    var safeProtocol = conn.Protocol?.Replace("\n", "").Replace("\r", "") ?? "UNKNOWN";
                    var safeLocalAddr = conn.LocalAddress?.Replace("\n", "").Replace("\r", "") ?? "UNKNOWN";
                    var safeLocalPort = conn.LocalPort?.Replace("\n", "").Replace("\r", "") ?? "0";
                    var safeRemoteAddr = conn.RemoteAddress?.Replace("\n", "").Replace("\r", "") ?? "UNKNOWN";
                    var safeRemotePort = conn.RemotePort?.Replace("\n", "").Replace("\r", "") ?? "0";
                    var safeState = conn.State?.Replace("\n", "").Replace("\r", "") ?? "UNKNOWN";
                    
                    message += $"{safeProtocol} {safeLocalAddr}:{safeLocalPort} -> {safeRemoteAddr}:{safeRemotePort} ({safeState})\n";
                }
                
                if (suspiciousConnections.Count > 10)
                {
                    message += $"\n... and {suspiciousConnections.Count - 10} more connections";
                }
                
                message += "\n\nFull connection details saved to NetworkScreensaverLogs folder.";
                
                MessageBox.Show(message, "Network Activity Alert", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private HashSet<string> GetRSSHosts()
        {
            var hosts = new HashSet<string>();
            var config = ConfigManager.GetConfig();
            
            foreach (var feed in config.Feeds.Where(f => f.Enabled))
            {
                try
                {
                    var uri = new Uri(feed.Url);
                    hosts.Add(uri.Host);
                }
                catch { }
            }
            
            return hosts;
        }

        private bool IsLocalOrSystemConnection(NetworkConnection conn)
        {
            return conn.RemoteAddress.StartsWith("127.") ||
                   conn.RemoteAddress == "::1" ||
                   conn.LocalPort == "0"; // Only filter loopback and invalid ports
        }

        private void LogConnectionsToFile(List<NetworkConnection> connections)
        {
            try
            {
                var logDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "NetworkScreensaverLogs");
                Directory.CreateDirectory(logDir);
                
                var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                var logFile = Path.Combine(logDir, $"connections_{timestamp}.txt");
                
                var rssHosts = GetRSSHosts();
                var logContent = $"Network Connections Detected - {DateTime.Now:yyyy-MM-dd HH:mm:ss}\n";
                logContent += $"Screensaver Session Duration: {DateTime.Now}\n\n";
                
                logContent += "ALL DETECTED CONNECTIONS:\n";
                logContent += new string('=', 50) + "\n";
                
                foreach (var conn in connections)
                {
                    var isRss = rssHosts.Contains(conn.RemoteAddress);
                    var isLocal = IsLocalOrSystemConnection(conn);
                    var flags = $"[{(isRss ? "RSS" : "")}{(isLocal ? "LOCAL" : "")}{(!isRss && !isLocal ? "SUSPICIOUS" : "")}]";
                    
                    logContent += $"{flags} {conn.Protocol} {conn.LocalAddress}:{conn.LocalPort} -> {conn.RemoteAddress}:{conn.RemotePort} ({conn.State})\n";
                }
                
                File.WriteAllText(logFile, logContent);
            }
            catch { }
        }

        private async Task LoadFeedsAsync()
        {
            try
            {
                await _rssReader.LoadFeedsAsync();
                var lines = _rssReader.GetScrollingText();
                
                if (lines.Any())
                {
                    _scrollingLines = lines;
                    _currentLineIndex = 0;
                }
            }
            catch { }
        }

        private void ScrollTimer_Tick(object sender, EventArgs e)
        {
            if (!_scrollingLines.Any()) return;
            
            Invalidate(); // Trigger repaint
            
            _currentLineIndex++;
            if (_currentLineIndex >= _scrollingLines.Count)
            {
                _currentLineIndex = 0;
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            try
            {
                base.OnPaint(e);
                
                var font = _terminalFont ?? SystemFonts.DefaultFont;
                
                if (!_scrollingLines.Any())
                {
                    // Show loading message
                    var loadingText = "Loading RSS feeds...";
                    var size = e.Graphics.MeasureString(loadingText, font);
                    var x = Math.Max(0, (Width - size.Width) / 2);
                    var y = Math.Max(0, (Height - size.Height) / 2);
                    
                    e.Graphics.DrawString(loadingText, font, Brushes.Lime, x, y);
                    return;
                }
            
            // Draw scrolling text
            var yPosition = 10;
            var lineHeight = font.Height;
            
            for (int i = 0; i < _displayedLines && i < _scrollingLines.Count; i++)
            {
                var lineIndex = (_currentLineIndex + i) % _scrollingLines.Count;
                var line = _scrollingLines[lineIndex];
                
                // Wrap long lines
                var wrappedLines = WrapText(line, e.Graphics, font, Width - 20);
                
                foreach (var wrappedLine in wrappedLines)
                {
                    if (yPosition + lineHeight > Height) break;
                    
                    e.Graphics.DrawString(wrappedLine, font, Brushes.Lime, 10, yPosition);
                    yPosition += lineHeight;
                }
            }
            
            // Draw status info in corner
            var statusText = $"Network Monitor Active | {DateTime.Now:HH:mm:ss}";
            var statusSize = e.Graphics.MeasureString(statusText, font);
            e.Graphics.DrawString(statusText, font, Brushes.DarkGreen, 
                Width - statusSize.Width - 10, Height - statusSize.Height - 10);
            }
            catch
            {
                // Fallback: draw simple error message
                try
                {
                    e.Graphics.DrawString("Screensaver Error - Press ESC to exit", SystemFonts.DefaultFont, Brushes.Red, 10, 10);
                }
                catch { }
            }
        }

        private List<string> WrapText(string text, Graphics graphics, Font font, float maxWidth)
        {
            var lines = new List<string>();
            
            if (string.IsNullOrEmpty(text))
            {
                lines.Add("");
                return lines;
            }
            
            var words = text.Split(' ');
            var currentLine = "";
            
            foreach (var word in words)
            {
                var testLine = string.IsNullOrEmpty(currentLine) ? word : $"{currentLine} {word}";
                var size = graphics.MeasureString(testLine, font);
                
                if (size.Width > maxWidth && !string.IsNullOrEmpty(currentLine))
                {
                    lines.Add(currentLine);
                    currentLine = word;
                }
                else
                {
                    currentLine = testLine;
                }
            }
            
            if (!string.IsNullOrEmpty(currentLine))
            {
                lines.Add(currentLine);
            }
            
            return lines;
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            try
            {
                _scrollTimer?.Stop();
                _scrollTimer?.Dispose();
                _feedUpdateTimer?.Stop();
                _feedUpdateTimer?.Dispose();
                _networkMonitor?.StopMonitoring();
                _rssReader?.Dispose();
                _terminalFont?.Dispose();
            }
            catch { }
            
            base.OnFormClosed(e);
        }

        private void InitializeComponent()
        {
            SuspendLayout();
            
            AutoScaleDimensions = new SizeF(6F, 13F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 600);
            Name = "ScreensaverForm";
            Text = "Network Screensaver";
            
            ResumeLayout(false);
        }
    }
}