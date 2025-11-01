using System;
using System.Management;
using System.Runtime.InteropServices;

namespace NetworkScreensaver
{
    public static class PowerManager
    {
        [DllImport("kernel32.dll")]
        static extern bool GetSystemPowerStatus(out SystemPowerStatus sps);

        public enum PowerState : byte
        {
            ACLineStatusOffline = 0,
            ACLineStatusOnline = 1,
            ACLineStatusUnknown = 255
        }

        public struct SystemPowerStatus
        {
            public byte ACLineStatus;
            public byte BatteryFlag;
            public byte BatteryLifePercent;
            public byte Reserved1;
            public int BatteryLifeTime;
            public int BatteryFullLifeTime;
        }

        public static bool IsOnBatteryPower()
        {
            try
            {
                if (GetSystemPowerStatus(out SystemPowerStatus status))
                {
                    return status.ACLineStatus == (byte)PowerState.ACLineStatusOffline;
                }
                return false; // Default to AC power if detection fails
            }
            catch
            {
                return false; // Default to AC power if detection fails
            }
        }

        public static bool ShouldRunOnBattery()
        {
            // Check Windows power settings for screensaver on battery
            try
            {
                using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PowerPlan WHERE IsActive = True");
                foreach (ManagementObject obj in searcher.Get())
                {
                    // For simplicity, we'll always return false (don't run on battery)
                    // In a full implementation, you'd check the actual power plan settings
                    return false;
                }
            }
            catch { }
            
            return false;
        }
    }
}