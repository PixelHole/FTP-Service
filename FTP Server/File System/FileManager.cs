using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using FileInformation;
using FTP_Server.Database.DataTypes;
using FTP_Server.File_System.Access_Management;
using Message_Board.Network;
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


        // file manipulation
        public static File CreateFile(string path, bool createFolders, User auth = null, AccessType accessType = AccessType.PublicBoth)
        {
            string[] splitPath = path.Split('\\');

            string folderPath = path.Substring(0, path.Length - $"\\{splitPath[splitPath.Length - 1]}".Length);

            folderPath = SystemToRootRelativePath(folderPath);

            Folder parent = createFolders ? CreateFolder(folderPath, auth, accessType) : GetFolderByPath(folderPath);

            if (parent == null || !parent.CanBeModifiedByUser(auth)) return null;

            System.IO.File.Create(path);
            
            File file = new File(path, accessType, auth);

            parent.AddFile(file);

            return file;
        }
        public static bool DeleteFile(string path, User auth)
        {
            if (IsSystemPath(path)) path = SystemToRootRelativePath(path);
            
            File file = GetFileByPath(path);

            if (file == null) return false;

            return DeleteFile(file, auth);
        }
        private static bool DeleteFile(File file, User auth)
        {
            if (!file.CanBeModifiedByUser(auth)) return false;
            
            file.Parent.RemoveFile(file);
            
            file.FlushData();
            
            System.IO.File.Delete(file.Path);

            return true;
        }
        
        
        // folder manipulation
        public static Folder CreateFolder(string path, User auth, AccessType accessType)
        {
            if (IsSystemPath(path)) path = SystemToRootRelativePath(path); 
            
            string[] splitPath = path.Split('\\');
            StringBuilder folderPath = new StringBuilder(RootDirectoryPath);

            Folder folder = RootDirectory;

            for (int i = 1; i < splitPath.Length; i++)
            {
                if (!folder.CanBeModifiedByUser(auth)) return null;
                
                var sub = folder.Subfolders.Find(subfolder => subfolder.Name == splitPath[i]);
                
                folderPath.Append($"\\{splitPath[i]}");

                if (sub == null)
                {
                    sub = new Folder(accessType, auth, folderPath.ToString(), false);
                    folder.AddSubfolder(sub);
                    
                    Directory.CreateDirectory(folderPath.ToString());
                }

                folder = sub;
            }

            return folder;
        }
        public static bool DeleteFolder(string path, User auth)
        {
            if (IsSystemPath(path)) path = SystemToRootRelativePath(path);
            
            Folder folder = GetFolderByPath(path);

            if (folder == null) return false;
            
            return DeleteFolder(folder, auth);
        }
        private static bool DeleteFolder(Folder folder, User auth)
        {
            if (!folder.CanBeModifiedByUser(auth) || folder.Parent == null) return false;

            DeleteAllFilesAndFoldersInFolder(folder, auth);

            folder.Parent.RemoveSubfolder(folder);
            
            folder.FlushData();

            Directory.Delete(folder.Path);

            return true;
        }
        private static void DeleteAllFilesAndFoldersInFolder(Folder folder, User auth)
        {
            foreach (var file in folder.Files)
            {
                DeleteFile(file, auth);
            }
            foreach (var subfolder in folder.Subfolders)
            {
                DeleteFolder(subfolder, auth);
            }
        }


        // folder listing
        public static string GetListOfFiles(string path, User auth)
        {
            if (IsSystemPath(path)) path = SystemToRootRelativePath(path);
            
            var folder = GetFolderByPath(path);

            if (folder == null) return string.Empty;

            List<FileItem> filesList = new List<FileItem>();

            foreach (var subfolder in folder.Subfolders)
            {
                if (!subfolder.CanBeReadByUser(auth)) continue;
                
                string rrPath = SystemToRootRelativePath(subfolder.Path);
                filesList.Add(new FileItem(subfolder.Name, "Folder", true, rrPath));
            }

            foreach (var file in folder.Files)
            {
                if (!file.CanBeReadByUser(auth)) continue;
                
                string rrPath = SystemToRootRelativePath(file.Path);
                filesList.Add(new FileItem(file.Name, file.Extension, false, rrPath));
            }

            string json = JsonConvert.SerializeObject(filesList);

            return json;
        }
        
        
        // find
        public static Folder GetFolderByPath(string path)
        {
            if (IsSystemPath(path)) path = SystemToRootRelativePath(path);
            
            if (path == "..") return RootDirectory;
            
            Queue<string> queuePath = new Queue<string>();
            string[] splitPath = path.Split('\\');

            if (splitPath.Length < 2) return null;
            
            for (int i = 1; i < splitPath.Length; i++)
            {
                queuePath.Enqueue(splitPath[i]);
            }

            return RootDirectory.FindSubfolder(queuePath);
        }
        public static File GetFileByPath(string path)
        {
            if (IsSystemPath(path)) path = SystemToRootRelativePath(path);
            
            Queue<string> queuePath = new Queue<string>();
            string[] splitPath = path.Split('\\');
            
            if (splitPath.Length < 2) return null;
            
            for (int i = 1; i < splitPath.Length - 1; i++)
            {
                queuePath.Enqueue(splitPath[i]);
            }

            string fileName = splitPath[splitPath.Length - 1];

            return RootDirectory.FindFile(queuePath, fileName);
        }

        
        // Index saving
        public static void SaveIndexToFile()
        {
            StreamWriter writer = new StreamWriter(SaveFileName);
            
            
            
            writer.Dispose();
            writer.Close();
        }
        public static void LoadIndexFromFile()
        {
            StreamReader reader = new StreamReader(SaveFileName);
            
            
            
            reader.Dispose();
            reader.Close();
        }


        
        // utility functions
        
        
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
        public static bool IsPathRootRelative(string path) => path.StartsWith("..");
        public static bool IsSystemPath(string path) => path.StartsWith(RootDirectoryPath);
    }
}