using System;
using System.IO;
using Newtonsoft.Json;

namespace FTP_Server.File_System
{
    public abstract class SystemFile
    {
        public string Name { get; protected set; }
        public string Path { get; protected set; }
        

        protected SystemFile(string name, string path)
        {
            Name = name;
            Path = path;
        }
        protected SystemFile(string path)
        {
            Path = path;

            string[] splitPath = path.Split('\\');

            Name = splitPath[splitPath.Length - 1];
        }
    }
}