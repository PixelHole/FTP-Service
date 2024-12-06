using System;
using System.IO;
using FTP_Server.File_System;

namespace FTP_Server
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            FileManager.LoadFromFile();
            Console.WriteLine(FileManager.RootDirectory.Name);
            // FileManager.SaveToFile();
        }
    }
}