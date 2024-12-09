using System.Collections.Generic;
using FTP_Server.Database.DataTypes;

namespace FTP_Server.Database
{
    public static class UserDatabase
    {
        public static List<User> Users { get; private set; } = new List<User>(new []
        {
            new User("nima", "nima"),
            new User("arya", "arya")
        });


        public static bool AddUser(User user)
        {
            if (Users.Contains(user)) return false;
            Users.Add(user);
            return true;
        }

        public static bool RemoveUser(User user)
        {
            return Users.Remove(user);
        }

        public static User FindUserByLoginInfo(string username, string password)
        {
            return Users.Find(user => user.CheckInfo(username, password));
        }
        public static User FindUserByUsername(string username)
        {
            return Users.Find(user => user.CheckUsername(username));
        }
    }
}