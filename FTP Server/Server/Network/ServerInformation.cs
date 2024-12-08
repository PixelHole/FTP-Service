using System.Net;

namespace Message_Board.Network
{
    public static class ServerInformation
    {
        public static IPAddress IpAddress { get; } = Dns.GetHostEntry("localhost").AddressList[0];
        public static int ConnectionPort { get; } = 8080;
        public static int FtpControlPort { get; } = 21;
        public static int FtpDataPort { get; } = 20;
    }
}