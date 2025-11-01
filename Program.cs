using System;
using System.Windows.Forms;

namespace NetworkScreensaver
{
    internal static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            try
            {
                System.IO.File.WriteAllText(@"C:\temp\screensaver_debug.txt", $"Starting screensaver at {DateTime.Now}\n");
            }
            catch { }
            
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            if (args.Length > 0)
            {
                string arg = args[0].ToLower().Trim();
                
                if (arg.StartsWith("/s"))
                {
                    // Run screensaver - try minimal first
                    try
                    {
                        Application.Run(new ScreensaverForm());
                    }
                    catch
                    {
                        // Fallback to minimal screensaver
                        Application.Run(new MinimalScreensaver());
                    }
                }
                else if (arg.StartsWith("/c"))
                {
                    // Configuration (not implemented)
                    MessageBox.Show("No configuration options available.", "Network Screensaver");
                }
                else if (arg.StartsWith("/p"))
                {
                    // Preview mode (not implemented for simplicity)
                    Application.Exit();
                }
            }
            else
            {
                // Default to screensaver mode
                try
                {
                    Application.Run(new ScreensaverForm());
                }
                catch
                {
                    // Fallback to minimal screensaver
                    Application.Run(new MinimalScreensaver());
                }
            }
        }
    }
}