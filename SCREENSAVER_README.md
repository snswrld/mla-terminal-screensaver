# Network RSS Screensaver

A Windows screensaver that displays RSS feeds in terminal-style green text while monitoring network activity.

## Features

- **RSS Feed Display**: Shows scrolling news from multiple RSS sources in green terminal text
- **Network Monitoring**: Captures network packets (Wireshark) and connection states (netstat)
- **Power Management**: Only runs when on AC power, respects Windows power settings
- **Lock Screen Integration**: Activates when screen locks or screensaver timeout occurs
- **Logging**: Saves network captures and logs to Documents/NetworkScreensaverLogs
- **ESC Key Failsafe**: Press ESC to safely exit and save all captures
- **Network Alerts**: Shows suspicious connections when screensaver exits

## Requirements

- Windows 11
- .NET 8.0 Runtime
- Administrator privileges (for network monitoring)
- Optional: Wireshark/tshark for packet capture

## Installation

1. **Build the screensaver**:
   ```cmd
   dotnet build NetworkScreensaver.csproj -c Release
   ```

2. **Install as administrator**:
   ```cmd
   # Run as Administrator
   install.bat
   ```

3. **Configure Windows**:
   - Right-click desktop → Personalize → Lock screen → Screen saver settings
   - Select "Network Screensaver" from dropdown
   - Set timeout as desired
   - Click "Apply"

## Network Monitoring Setup

### Wireshark (Optional)
1. Install Wireshark from https://www.wireshark.org/
2. Ensure `tshark.exe` is in your PATH
3. The screensaver will automatically capture packets when active

### Logs Location
All logs are saved to: `%USERPROFILE%\Documents\NetworkScreensaverLogs\`
- `capture_YYYYMMDD_HHMMSS.pcap` - Wireshark packet captures
- `netstat_YYYYMMDD_HHMMSS.txt` - Network connection logs

## Power Management

The screensaver automatically:
- Detects if running on battery power
- Exits immediately if on battery (configurable)
- Respects Windows power plan screensaver settings

## RSS Sources

Default feeds (configurable in RSSFeedReader.cs):
- BBC News
- CNN
- Reuters

## Customization

### Adding RSS Feeds
Edit `RSSFeedReader.cs` and add URLs to the `_feedUrls` list:
```csharp
_feedUrls = new List<string>
{
    "https://your-rss-feed.com/rss.xml",
    // Add more feeds here
};
```

### Changing Appearance
Modify `ScreensaverForm.cs`:
- `BackColor` - Background color
- `ForeColor` - Text color  
- `_terminalFont` - Font and size
- `_scrollTimer.Interval` - Scroll speed

## Usage

### Safe Exit
- **ESC Key**: Press ESC to safely exit screensaver and save all network captures
- **Any Key/Mouse**: Other input also exits but ESC is recommended for clean shutdown

### Network Alerts
- When screensaver exits, you'll see an alert if suspicious network connections were detected
- Alert excludes RSS feed connections and local/system connections
- Shows up to 10 connections with protocol, addresses, and ports

## Troubleshooting

### Screensaver doesn't appear in list
- Ensure you ran installation as Administrator
- Check that the .scr file is in System32 folder
- Restart Windows

### Network monitoring not working
- Run as Administrator
- Install Wireshark for packet capture
- Check Windows Firewall settings

### Screensaver exits immediately
- Check if on battery power
- Verify RSS feeds are accessible
- Check Windows Event Viewer for errors

### No network alerts showing
- Ensure network monitoring is enabled in config
- Check if connections are being filtered as local/system
- Verify Administrator privileges for network access

## Security Notes

- Requires Administrator privileges for network monitoring
- Network logs may contain sensitive information
- RSS feeds are fetched over HTTPS when possible
- Packet captures are stored locally only

## Uninstallation

1. Change screensaver in Windows settings
2. Delete: `%SystemRoot%\System32\NetworkScreensaver.scr`
3. Delete logs: `%USERPROFILE%\Documents\NetworkScreensaverLogs\`