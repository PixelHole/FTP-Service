using System.Collections.Generic;
using System.IO;
using System.Linq;
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
                    AddSubfolder(folder);
                }
            }

            if (files == null) return;
            
            foreach (var file in files)
            {
                AddFile(file);
            }
        }
        public Folder(AccessType accessType, User authUser, string path, bool index) : base(accessType, authUser, path)
        {
            if (index) IndexThisFolder();
        }
        
        
        public bool AddSubfolder(Folder folder)
        {
            if (Subfolders.Contains(folder)) return false;
            folder.SetParent(this);
            Subfolders.Add(folder);
            return true;
        }
        public bool RemoveSubfolder(Folder folder)
        {
            if (Subfolders.Remove(folder))
            {
                folder.SetParent(null);
                return true;
            }
            return false;
        }
        public bool AddFile(File file)
        {
            if (Files.Contains(file)) return false;
            file.SetParent(this);
            Files.Add(file);
            return true;
        }
        public bool RemoveFile(File file)
        {
            if (Files.Remove(file))
            {
                file.SetParent(null);
                return true;
            }
            return false;
        }


        public Folder FindSubfolder(Queue<string> path)
        {
            string target = path.Dequeue();
            
            if (path.Count >= 1)
            {
                Folder targetSub = Subfolders.Find(folder => folder.Name == target);

                return targetSub?.FindSubfolder(path);
            }

            return Subfolders.Find(folder => folder.Name == target);
        }
        public File FindFile(Queue<string> folderPath, string fileName)
        {
            var parent = folderPath.Count == 0 ? this : FindSubfolder(folderPath);

            if (parent == null) return null;

            return parent.Files.Find(file => file.Name == fileName);
        }

        public void FlushData()
        {
            foreach (var subfolder in Subfolders)
            {
                subfolder.FlushData();
            }

            Subfolders.Clear();

            foreach (var file in Files)
            {
                file.FlushData();
            }
            
            Files.Clear();
            
            Parent = null;
        }

        public List<SystemFile> GetAllSubFiles(User auth = null)
        {
            List<SystemFile> files = Subfolders.Where(folder => folder.CanBeReadByUser(auth)).Cast<SystemFile>().ToList();

            files.AddRange(Files.Where(file => file.CanBeReadByUser(auth)).Cast<SystemFile>());

            return files;
        }
        
        public void IndexThisFolder()
        {
            Subfolders.Clear();
            Files.Clear();
            
            string[] files = Directory.GetFiles(Path);

            foreach (var filePath in files)
            {
                File file = new File(filePath, AccessType, AuthorizedUser);
                AddFile(file);
            }

            string[] subfolders = Directory.GetDirectories(Path);

            foreach (var subfolderPath in subfolders)
            {
                Folder folder = new Folder(AccessType, AuthorizedUser, subfolderPath, true);
                AddSubfolder(folder);
            }
        }
    }
}