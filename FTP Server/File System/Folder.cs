using System.Collections.Generic;
using System.IO;
using FTP_Server.Database.DataTypes;
using FTP_Server.File_System.Access_Management;
using Newtonsoft.Json;

namespace FTP_Server.File_System
{
    public class Folder : SystemFile
    {
        public List<Folder> Subfolders { get; private set; } = new List<Folder>();
        public List<File> Files { get; private set; } = new List<File>();


        [JsonConstructor]
        public Folder(Folder[] folders, File[] files, string name, AccessType accessType, User authUser, string path) 
            : base(name, path, accessType, authUser)
        {
            if (folders != null)
            {
                foreach (var folder in folders)
                {
                    Subfolders.Add(folder);
                }
            }

            if (files != null)
            {
                foreach (var file in files)
                {
                    Files.Add(file);
                }
            }
        }
        public Folder(AccessType accessType, User authUser, string path, bool index) : base(accessType, authUser, path)
        {
            if (index) IndexThisFolder();
        }

        public void CreateEmptySubfolder(string folderName)
        {
            string path = $"{Path}\\{folderName}";
            Directory.CreateDirectory(path);
            Subfolders.Add(new Folder(AccessType, AuthorizedUser, path, false));
        }
        
        public bool AddSubfolder(Folder folder)
        {
            if (Subfolders.Contains(folder)) return false;
            Subfolders.Add(folder);
            return true;
        }
        public bool RemoveFolder(Folder folder)
        {
            return Subfolders.Remove(folder);
        }
        public bool AddFile(File file)
        {
            if (Files.Contains(file)) return false;
            Files.Add(file);
            return true;
        }
        public bool RemoveFile(File file)
        {
            return Files.Remove(file);
        }

        public void IndexThisFolder()
        {
            Subfolders.Clear();
            Files.Clear();
            
            string[] files = Directory.GetFiles(Path);

            foreach (var filePath in files)
            {
                Files.Add(new File(AccessType, AuthorizedUser, filePath));
            }

            string[] subfolders = Directory.GetDirectories(Path);

            foreach (var subfolderPath in subfolders)
            {
                Subfolders.Add(new Folder(AccessType, AuthorizedUser, subfolderPath, true));
            }
        }
    }
}