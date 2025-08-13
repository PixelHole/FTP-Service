using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using FTP_Server.Server.Client_Session;
using Message_Board.Network;

namespace FTP_Server.Server
{
    public static class LocalServer
    {
        private static Socket ControlListener { get; set; }
        public static Socket DataListener { get; set; }
        private static Thread ListenerThread { get; set; }


        public static void StartService()
        {
            ListenerThread = new Thread(ServiceLoop);
            ListenerThread.Start();
        }

        private static void ServiceLoop()
        {
            Print("Starting service");
            EstablishServer();
            Print("server established, listening for connections");
            ListenForConnections();
        }
        private static void EstablishServer()
        {
            IPEndPoint controlEndPoint = new IPEndPoint(ServerInformation.IpAddress, ServerInformation.FtpControlPort);
            IPEndPoint dataEndPoint = new IPEndPoint(ServerInformation.IpAddress, ServerInformation.FtpDataPort);

            ControlListener = new Socket(ServerInformation.IpAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            DataListener = new Socket(ServerInformation.IpAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            
            ControlListener.Bind(controlEndPoint);
            DataListener.Bind(dataEndPoint);
        }

        private static void ListenForConnections()
        {
            ControlListener.Listen(100);

            while (true)
            {
                Socket controlSocket = ControlListener.Accept();

                ClientManager.AddClient(controlSocket);
                Print($"Client connected : {controlSocket.RemoteEndPoint}");
            }
        }


        private static void Print(string msg)
        {
            Console.WriteLine($"[Listener Server] : {msg}");
        }
    }
}