using System;
using FTP_Server.File_System;
using FTP_Server.Server;

namespace FTP_Server
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            FileManager.StartService();
        }
    }
}