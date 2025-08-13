using System;
using System.IO;
using FTP_Server.Database.DataTypes;
using FTP_Server.File_System.Access_Management;
using Newtonsoft.Json;

namespace FTP_Server.File_System
{
    public class File : SystemFile
    {
        [JsonProperty] public string Extension { get; private set; }


        [JsonConstructor]
        public File(string name, string path, AccessType accessType, User authorizedUser, string extension) 
            : base(name, path, accessType, authorizedUser)
        {
            Extension = extension;
        }
        public File(string path, AccessType accessType, User authUser) : base(accessType, authUser, path)
        {
            GetDataFromPath(path);
        }

        private void GetDataFromPath(string path)
        {
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