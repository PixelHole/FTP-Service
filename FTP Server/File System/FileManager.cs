using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace FTP_Server.File_System
{
    public static class FileManager
    {
        private static string SaveFileName { get; } = "Indices.json";
        private static string RootDirectoryPath { get; } = "M:\\FTP server Root";
        public static Folder RootDirectory { get; private set; } // = new Folder(RootDirectoryPath);
        public static string DirectoryRootPath { get; private set; } = "";


        
        public static void SaveToFile()
        {
            StreamWriter writer = new StreamWriter(SaveFileName);
            
            string raw = JsonConvert.SerializeObject(RootDirectory, Formatting.Indented);
            
            writer.Flush();
            writer.Write(raw);
            
            writer.Dispose();
            writer.Close();
        }
        public static void LoadFromFile()
        {
            StreamReader reader = new StreamReader(SaveFileName);

            string raw = reader.ReadToEnd();

            RootDirectory = JsonConvert.DeserializeObject<Folder>(raw);
            
            reader.Dispose();
            reader.Close();
        }
    }
}