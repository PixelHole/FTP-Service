namespace FTP_Server.Database.DataTypes
{
    public class User
    {
        private string Username { get; set; }
        private string Password { get; set; }


        public User(string username, string password)
        {
            Username = username;
            Password = password;
        }

        public bool CheckInfo(string username, string password) => CheckUsername(username) && CheckPassword(password);
        public bool CheckUsername(string username) => string.Equals(username, Username);
        private bool CheckPassword(string password) => string.Equals(password, Password);
    }
}