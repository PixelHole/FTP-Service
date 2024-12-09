using System;
using System.IO;
using FTP_Server.Database.DataTypes;
using FTP_Server.File_System.Access_Management;
using Newtonsoft.Json;

namespace FTP_Server.File_System
{
    public abstract class SystemFile
    {
        public string Name { get; protected set; }
        public string Path { get; protected set; }
        public AccessType AccessType { get; private set; }
        public User AuthorizedUser { get; private set; }
        public Folder Parent { get; protected set; }


        protected SystemFile(string name, string path) : this(name, path, AccessType.PublicBoth, null)
        {
            Name = name;
            Path = path;
        }
        protected SystemFile(string name, string path, AccessType accessType, User authorizedUser)
        {
            Name = name;
            Path = path;
            SetAccessType(accessType);
            SetAuthorization(authorizedUser);
        }
        protected SystemFile(AccessType accessType, User authUser, string path)
        {
            Path = path;

            string[] splitPath = path.Split('\\');

            Name = splitPath[splitPath.Length - 1];

            SetAccessType(accessType);

            SetAuthorization(authUser);
        }


        public void SetAccessType(AccessType accessType) => AccessType = accessType;
        public void SetAuthorization(User user) => AuthorizedUser = user;
        public void ClearAuthorization() => SetAuthorization(null);

        public bool CanBeModifiedByUser(User user)
        {
            if (AccessType == AccessType.PublicBoth) return true;

            if (AccessType == AccessType.PrivateBoth && AuthorizedUser.Equals(user)) return true;

            return false;
        }

        public bool CanBeReadByUser(User user)
        {
            if (AccessType == AccessType.PrivateBoth || AccessType == AccessType.PublicReadOnly) return true;

            if (AuthorizedUser.Equals(user)) return true;

            return false;
        }

        public void SetParent(Folder parent) => Parent = parent;
    }
}