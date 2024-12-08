using System.Net;
using System.Net.Sockets;
using System.Threading;
using FTP_Server.Server.Client_Session;
using Message_Board.Network;

namespace FTP_Server.Server
{
    public static class LocalServer
    {
        private static Socket Handler { get; set; }
        private static Thread ListenerThread { get; set; }


        public static void StartService()
        {
            ListenerThread = new Thread(ServiceLoop);
            ListenerThread.Start();
        }

        private static void ServiceLoop()
        {
            EstablishServer();
            ListenForConnections();
        }
        private static void EstablishServer()
        {
            IPEndPoint endPoint = new IPEndPoint(ServerInformation.IpAddress, ServerInformation.ConnectionPort);

            Handler = new Socket(ServerInformation.IpAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            
            Handler.Bind(endPoint);
        }

        private static void ListenForConnections()
        {
            Handler.Listen(10);

            while (true)
            {
                Socket clientSocket = Handler.Accept();

                ClientManager.AddClient(clientSocket);
            }
        }
        
    }
}