using System;
using FTP_Server.File_System;
using FTP_Server.Server;
using Message_Board.Network;

namespace FTP_Server
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            FileManager.StartService();
            LocalServer.StartService();
        }
    }
}