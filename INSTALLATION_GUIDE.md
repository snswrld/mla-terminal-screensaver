# Network RSS Screensaver - Installation Guide

## Quick Start

1. **Prerequisites**:
   - Windows 11
   - .NET 8.0 SDK or Runtime
   - Administrator privileges

2. **Build & Install**:
   ```cmd
   # Open Command Prompt as Administrator
   cd "c:\Users\d\Code_Proj\Screensaver\mla-terminal-screensaver"
   
   # Run the installation script
   build_and_install.bat
   ```

3. **Configure Windows**:
   - Right-click desktop → Personalize → Lock screen
   - Click "Screen saver settings"
   - Select "NetworkScreensaver" from dropdown
   - Set timeout (e.g., 5 minutes)
   - Click "Apply"

## Detailed Setup

### Step 1: Install .NET 8.0
Download from: https://dotnet.microsoft.com/download/dotnet/8.0
- Choose ".NET Desktop Runtime" for end users
- Choose ".NET SDK" for developers

### Step 2: Optional - Install Wireshark
Download from: https://www.wireshark.org/download.html
- Install with default options
- Ensure "tshark" command-line tool is included
- Add Wireshark to PATH if not automatic

### Step 3: Build the Screensaver
```cmd
# Navigate to project directory
cd "c:\Users\d\Code_Proj\Screensaver\mla-terminal-screensaver"

# Build the project
dotnet build NetworkScreensaver.csproj -c Release
```

### Step 4: Install as Screensaver
```cmd
# Copy to Windows system directory (requires admin)
copy "bin\Release\net8.0-windows\NetworkScreensaver.scr" "%SystemRoot%\System32\"
```

### Step 5: Configure RSS Feeds
Edit `rss_feeds.json` to customize:
```json
{
  "feeds": [
    {
      "name": "Your News Source",
      "url": "https://your-rss-feed.com/rss.xml",
      "enabled": true
    }
  ],
  "settings": {
    "runOnBattery": false,
    "enableNetworkMonitoring": true,
    "scrollSpeedMs": 100
  }
}
```

## Testing

### Test Before Installation
```cmd
# Run screensaver directly
test_screensaver.bat
```

### Test After Installation
1. Right-click desktop → Personalize → Lock screen → Screen saver settings
2. Select "NetworkScreensaver"
3. Click "Preview" button
4. Press any key to exit preview

## Configuration Options

### RSS Feeds (`rss_feeds.json`)
- **feeds**: Array of RSS feed sources
  - `name`: Display name for the source
  - `url`: RSS feed URL
  - `enabled`: Whether to include this feed

### Settings
- **updateIntervalMinutes**: How often to refresh feeds (default: 5)
- **scrollSpeedMs**: Text scroll speed in milliseconds (default: 100)
- **maxItemsPerFeed**: Maximum items per RSS feed (default: 10)
- **runOnBattery**: Whether to run when on battery power (default: false)
- **enableNetworkMonitoring**: Enable packet capture and netstat logging (default: true)
- **netstatIntervalSeconds**: How often to log network connections (default: 30)
- **wiresharkCaptureDurationMinutes**: Duration of each packet capture (default: 5)

## Network Monitoring

### Log Files Location
`%USERPROFILE%\Documents\NetworkScreensaverLogs\`

### File Types
- **capture_YYYYMMDD_HHMMSS.pcap**: Wireshark packet captures
- **netstat_YYYYMMDD_HHMMSS.txt**: Network connection snapshots

### Requirements
- Administrator privileges (for packet capture)
- Wireshark/tshark installed (optional, for packet capture)
- Windows Firewall may need configuration

## Troubleshooting

### Screensaver Not Listed
- Ensure you ran installation as Administrator
- Check if NetworkScreensaver.scr exists in System32
- Restart Windows Explorer: `taskkill /f /im explorer.exe && start explorer.exe`

### Build Errors
```cmd
# Check .NET installation
dotnet --version

# Clean and rebuild
dotnet clean
dotnet build NetworkScreensaver.csproj -c Release
```

### Network Monitoring Issues
- Run Command Prompt as Administrator
- Check if tshark.exe is in PATH: `tshark --version`
- Verify Windows Firewall settings
- Check Event Viewer for application errors

### RSS Feeds Not Loading
- Check internet connection
- Verify RSS feed URLs are accessible
- Check Windows Event Viewer for HTTP errors
- Test feeds manually in browser

### Performance Issues
- Reduce `maxItemsPerFeed` in configuration
- Increase `scrollSpeedMs` for slower scrolling
- Disable network monitoring if not needed
- Check available system memory

## Security Considerations

### Administrator Privileges
Required for:
- Installing screensaver to System32
- Network packet capture
- Low-level system monitoring

### Network Monitoring
- Packet captures may contain sensitive data
- Logs are stored locally only
- Consider data retention policies
- Review captured data before sharing

### RSS Feeds
- Feeds are fetched over internet
- Use HTTPS feeds when possible
- Be cautious with untrusted feed sources
- Monitor network usage

## Uninstallation

1. **Change Screensaver**:
   - Set Windows screensaver to "(None)" or another option

2. **Remove Files**:
   ```cmd
   # Remove screensaver (as Administrator)
   del "%SystemRoot%\System32\NetworkScreensaver.scr"
   
   # Remove logs (optional)
   rmdir /s "%USERPROFILE%\Documents\NetworkScreensaverLogs"
   ```

3. **Clean Registry** (if needed):
   - Usually not required for screensavers
   - Windows will automatically remove invalid entries

## Advanced Configuration

### Custom RSS Parser
Modify `RSSFeedReader.cs` for special feed formats:
```csharp
// Add custom parsing logic in LoadFeedAsync method
```

### Custom Appearance
Modify `ScreensaverForm.cs`:
```csharp
// Change colors, fonts, layout
BackColor = Color.Black;
ForeColor = Color.Lime;
_terminalFont = new Font("Consolas", 12);
```

### Network Capture Filters
Modify `NetworkMonitor.cs`:
```csharp
// Add Wireshark capture filters
Arguments = $"-i any -w \"{captureFile}\" -f \"tcp port 80\" -a duration:{duration}";
```

## Support

For issues or questions:
1. Check Windows Event Viewer for application errors
2. Review log files in NetworkScreensaverLogs folder
3. Test individual components (RSS feeds, network tools)
4. Verify system requirements and permissions