using System;
using System.IO;
using FTP_Server.Database.DataTypes;
using FTP_Server.File_System.Access_Management;
using Newtonsoft.Json;

namespace FTP_Server.File_System
{
    public class File : SystemFile
    {
        public string Extension { get; private set; }
        public long Size { get; private set; }
        
        
        [JsonConstructor]
        public File(string name, string path, AccessType accessType, User authorizedUser, string extension, long size) 
            : base(name, path, accessType, authorizedUser)
        {
            Extension = extension;
            Size = size;
        }
        public File(string path, AccessType accessType, User authUser) : base(accessType, authUser, path)
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

        public void FlushData()
        {
            Parent = null;
        }
    }
}