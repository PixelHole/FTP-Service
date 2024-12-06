using System;
using System.IO;
using Newtonsoft.Json;

namespace FTP_Server.File_System
{
    public class File : SystemFile
    {
        public string Extension { get; private set; }
        public long Size { get; private set; }
        
        
        [JsonConstructor]
        public File(string name, string extension, long size, string path) : base(name, path)
        {
            Extension = extension;
            Size = size;
        }
        public File(string path) : base(path)
        {
            GetDataFromPath(path);
        }
        
        private void GetDataFromPath(string path)
        {
            FileInfo info = new FileInfo(path);

            Size = info.Length;
            
            string[] SplitName = Name.Split('.');

            try
            {
                Extension = SplitName[SplitName.Length - 1];
            }
            catch (IndexOutOfRangeException)
            {
                Extension = "No Extension";
            }
        }
    }
}