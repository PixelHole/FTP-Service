using System;
using System.IO;
using FTP_Server.Database.DataTypes;
using FTP_Server.File_System.Access_Management;
using Newtonsoft.Json;

namespace FTP_Server.File_System
{
    public abstract class SystemFile
    {
        [JsonProperty] public string Name { get; protected set; }
        [JsonProperty] public string Path { get; protected set; }
        [JsonProperty] public AccessType AccessType { get; private set; }
        [JsonProperty] public User AuthorizedUser { get; private set; }
        [JsonIgnore] public Folder Parent { get; protected set; }


        protected SystemFile(string name, string path) : this(name, path, AccessType.PublicBoth, null)
        {
            Name = name;
            Path = path;
        }
        [JsonConstructor]
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
            if (AccessType is AccessType.PrivateReadOnly or AccessType.PublicReadOnly) return false;
            
            if (AccessType == AccessType.PublicBoth || AuthorizedUser == null) return true;

            if (user != null && AuthorizedUser.Equals(user)) return true;

            return false;
        }

        public bool CanBeReadByUser(User user)
        {
            if (AccessType == AccessType.PublicBoth ||
                AccessType == AccessType.PublicReadOnly ||
                AuthorizedUser == null)
                return true;

            if (user == null && AuthorizedUser != null) return false;
            
            if (AuthorizedUser.Equals(user)) return true;

            return false;
        }

        public void SetParent(Folder parent) => Parent = parent;
    }
}