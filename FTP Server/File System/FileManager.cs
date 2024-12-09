using System;
using System.Collections.Generic;
using System.IO;
using FTP_Server.File_System.Access_Management;
using Newtonsoft.Json;

namespace FTP_Server.File_System
{
    public static class FileManager
    {
        private static string SaveFileName { get; } = "Indices.json";
        private static string RootDirectoryPath { get; } = "M:\\FTP server Root";
        public static Folder RootDirectory { get; private set; }
        public static string DirectoryRootPath { get; private set; } = "";



        public static void StartService()
        {
            RootDirectory = new Folder(AccessType.PublicBoth, null, RootDirectoryPath, true);
        }
        public static void IndexAllFilesInRootDirectory()
        {
            RootDirectory.IndexThisFolder();
        }


        public static Folder GetFolderByPath(string path)
        {
            Queue<string> queuePath = new Queue<string>();
            string[] splitPath = path.Split('\\');

            if (splitPath.Length < 2) return null;
            
            for (int i = 2; i < splitPath.Length; i++)
            {
                queuePath.Enqueue(splitPath[i]);
            }

            return RootDirectory.FindSubfolder(queuePath);
        }
        public static File GetFileByPath(string path)
        {
            Queue<string> queuePath = new Queue<string>();
            string[] splitPath = path.Split('\\');
            
            if (splitPath.Length < 2) return null;
            
            for (int i = 2; i < splitPath.Length - 1; i++)
            {
                queuePath.Enqueue(splitPath[i]);
            }

            string fileName = splitPath[splitPath.Length - 1];

            return RootDirectory.FindFile(queuePath, fileName);
        }

        public static void SaveToFile()
        {
            StreamWriter writer = new StreamWriter(SaveFileName);
            
            
            
            writer.Dispose();
            writer.Close();
        }
        public static void LoadFromFile()
        {
            StreamReader reader = new StreamReader(SaveFileName);
            
            
            
            reader.Dispose();
            reader.Close();
        }

        public static string RootRelativeToSystemPath(string rrPath)
        {
            string sysPath = rrPath.Substring(2, rrPath.Length - 2);
            sysPath = RootDirectoryPath + sysPath;

            return sysPath;
        }
        public static string SystemToRootRelativePath(string path)
        {
            string relPath = path.Substring(RootDirectoryPath.Length, path.Length - RootDirectoryPath.Length);
            relPath = ".." + relPath;

            return relPath;
        }
        public static bool IsPathValid(string path) => path.StartsWith(RootDirectoryPath);
    }
}