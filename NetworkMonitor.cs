using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace NetworkScreensaver
{
    public class NetworkMonitor
    {
        private readonly string _logDirectory;
        private readonly Settings _settings;
        private Process _wiresharkProcess = null!;
        private bool _isMonitoring;
        private readonly List<NetworkConnection> _capturedConnections = new();
        private readonly HashSet<string> _initialConnections = new();

    public NetworkMonitor()
    {
        try
        {
            // Use Windows 11 compliant LocalApplicationData path
            var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            if (string.IsNullOrEmpty(localAppData))
                throw new InvalidOperationException("LocalApplicationData path not available");
                
            _logDirectory = Path.Combine(localAppData, "NetworkScreensaver", "Logs");
            Directory.CreateDirectory(_logDirectory);
        }
        catch (Exception ex)
        {
            // Fallback to temp directory with proper error logging
            try
            {
                _logDirectory = Path.Combine(Path.GetTempPath(), "NetworkScreensaver", "Logs");
                Directory.CreateDirectory(_logDirectory);
                
                // Log the error for debugging
                var errorLog = Path.Combine(_logDirectory, "error_log.txt");
                File.AppendAllText(errorLog, $"{DateTime.Now}: Failed to use LocalApplicationData: {ex.Message}\n");
            }
            catch
            {
                // Final fallback - use current directory
                _logDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Logs");
                Directory.CreateDirectory(_logDirectory);
            }
        }
        
        _settings = ConfigManager.GetConfig().Settings;
    }




        public async Task StartMonitoringAsync()
        {
            if (_isMonitoring) return;
            
            // Capture initial connections
            await CaptureInitialConnections();
            
            _isMonitoring = true;
            
            // Start Wireshark capture
            _ = Task.Run(StartWiresharkCapture);
            
            // Start netstat logging
            _ = Task.Run(StartNetstatLogging);
        }

        public void StopMonitoring()
        {
            _isMonitoring = false;
            
            try
            {
                if (_wiresharkProcess != null)
                {
                    _wiresharkProcess.Kill();
                    _wiresharkProcess.Dispose();
                }
            }
            catch { }
        }

        public List<NetworkConnection> GetActiveConnections()
        {
            return _capturedConnections.ToList();
        }

        private async Task CaptureInitialConnections()
        {
            try
            {
                var connections = await GetCurrentConnections();
                foreach (var conn in connections)
                {
                    _initialConnections.Add($"{conn.Protocol}:{conn.LocalAddress}:{conn.LocalPort}:{conn.RemoteAddress}:{conn.RemotePort}");
                }
            }
            catch { }
        }

        private async Task<List<NetworkConnection>> GetCurrentConnections()
        {
            var connections = new List<NetworkConnection>();
            
            try
            {
                // Try netstat first
                var startInfo = new ProcessStartInfo
                {
                    FileName = "netstat",
                    Arguments = "-an",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true
                };
                
                using var process = Process.Start(startInfo);
                if (process != null)
                {
                    var output = await process.StandardOutput.ReadToEndAsync();
                    connections = ParseNetstatOutput(output);
                }
            }
            catch
            {
                // Fallback to WMI if netstat fails
                connections = GetConnectionsViaWMI();
            }
            
            return connections;
        }
        
        private List<NetworkConnection> GetConnectionsViaWMI()
        {
            var connections = new List<NetworkConnection>();
            
            try
            {
                using var searcher = new System.Management.ManagementObjectSearcher("SELECT * FROM Win32_PerfRawData_Tcpip_NetworkInterface");
                foreach (System.Management.ManagementObject obj in searcher.Get())
                {
                    // Basic network interface monitoring without admin privileges
                    var name = obj["Name"]?.ToString() ?? "Unknown";
                    if (name != "Loopback Pseudo-Interface 1")
                    {
                        connections.Add(new NetworkConnection
                        {
                            Protocol = "WMI",
                            LocalAddress = Environment.MachineName,
                            LocalPort = "0",
                            RemoteAddress = name,
                            RemotePort = "0",
                            State = "ACTIVE"
                        });
                    }
                }
            }
            catch { }
            
            return connections;
        }

        private List<NetworkConnection> ParseNetstatOutput(string output)
        {
            var connections = new List<NetworkConnection>();
            var lines = output.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            
            foreach (var line in lines)
            {
                try
                {
                    var parts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length >= 4 && (parts[0] == "TCP" || parts[0] == "UDP"))
                    {
                        var localParts = parts[1].Split(':');
                        var remoteParts = parts[2].Split(':');
                        
                        if (localParts.Length >= 2 && remoteParts.Length >= 2)
                        {
                            connections.Add(new NetworkConnection
                            {
                                Protocol = parts[0],
                                LocalAddress = string.Join(":", localParts.Take(localParts.Length - 1)),
                                LocalPort = localParts.Last(),
                                RemoteAddress = string.Join(":", remoteParts.Take(remoteParts.Length - 1)),
                                RemotePort = remoteParts.Last(),
                                State = parts.Length > 3 ? parts[3] : "UNKNOWN"
                            });
                        }
                    }
                }
                catch { }
            }
            
            return connections;
        }

        private void StartWiresharkCapture()
        {
            try
            {
                var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                var logFile = Path.Combine(_logDirectory, $"network_activity_{timestamp}.txt");
                
                // Use PowerShell Get-NetTCPConnection instead of tshark (no admin required)
                var startInfo = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = $"-Command \"while($true) {{ Get-NetTCPConnection | Out-File -Append '{logFile}'; Start-Sleep 30 }}\"",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };
                
                _wiresharkProcess = Process.Start(startInfo);
            }
            catch
            {
                // PowerShell not available, continue without it
            }
        }

        private async Task StartNetstatLogging()
        {
            while (_isMonitoring)
            {
                try
                {
                    var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                    var logFile = Path.Combine(_logDirectory, $"netstat_{timestamp}.txt");
                    
                    var connections = await GetCurrentConnections();
                    
                    // Track new connections
                    foreach (var conn in connections)
                    {
                        var connKey = $"{conn.Protocol}:{conn.LocalAddress}:{conn.LocalPort}:{conn.RemoteAddress}:{conn.RemotePort}";
                        if (!_initialConnections.Contains(connKey) && 
                            !_capturedConnections.Any(c => c.GetKey() == connKey))
                        {
                            _capturedConnections.Add(conn);
                        }
                    }
                    
                    // Save to log file
                    var output = string.Join("\n", connections.Select(c => 
                        $"{c.Protocol} {c.LocalAddress}:{c.LocalPort} {c.RemoteAddress}:{c.RemotePort} {c.State}"));
                    await File.WriteAllTextAsync(logFile, $"Timestamp: {DateTime.Now}\n\n{output}");
                }
                catch { }
                
                await Task.Delay(_settings.NetstatIntervalSeconds * 1000);
            }
        }
    }
}