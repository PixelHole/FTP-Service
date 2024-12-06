using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace FTP_Server.File_System
{
    public class Folder : SystemFile
    {
        public List<Folder> Subfolders { get; private set; } = new List<Folder>();
        public List<File> Files { get; private set; } = new List<File>();


        [JsonConstructor]
        public Folder(Folder[] folders, File[] files, string name, string path) : base(name, path)
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

        public Folder(string path) : base(path)
        {
            IndexThisFolder();
        }

        public void IndexThisFolder()
        {
            string[] files = Directory.GetFiles(Path);

            foreach (var file in files)
            {
                Files.Add(new File(file));
            }

            string[] subfolders = Directory.GetDirectories(Path);

            foreach (var subfolder in subfolders)
            {
                Subfolders.Add(new Folder(subfolder));
            }
        }
    }
}