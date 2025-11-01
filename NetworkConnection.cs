namespace NetworkScreensaver
{
    public class NetworkConnection
    {
        public string Protocol { get; set; } = "";
        public string LocalAddress { get; set; } = "";
        public string LocalPort { get; set; } = "";
        public string RemoteAddress { get; set; } = "";
        public string RemotePort { get; set; } = "";
        public string State { get; set; } = "";

        public string GetKey()
        {
            return $"{Protocol}:{LocalAddress}:{LocalPort}:{RemoteAddress}:{RemotePort}";
        }
    }
}