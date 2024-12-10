using System;
using Message_Board.Network;

namespace FTP_Server.Server.Client_Session
{
    public class ClientCommandInterpreter
    {
        private Client Client { get; set; }


        public ClientCommandInterpreter(Client client)
        {
            Client = client;
        }

        public string GetCommandResult(string cmd)
        {
            string[] split = cmd.Split(NetworkFlags.SeparatorFlag.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

            if (split.Length == 0) return NetworkFlags.InvalidCommandFlag;

            string header = split[0].ToLower();
            
            switch (header)
            {
                case "user" :
                    return UsernameCommand(split);
                case "pass" :
                    return PasswordCommand(split);
                case "list" :
                    return ListCommand(split);
                case "retr" :
                    return RetrieveFileCommand(split);
                case "stor" :
                    return StoreFileCommand(split);
                case "dele" :
                    return DeleteFileCommand(split);
                case "mkd" :
                    return MakeDirectoryCommand(split);
                case "rmd" :
                    return RemoveDirectoryCommand(split);
                case "pwd" :
                    return CurrentDirectoryPathCommand(split);
                case "cwd" :
                    return ChangeDirectoryCommand(split);
                case "cdup" :
                    return GoToParentDirectoryCommand(split);
                case "quit" :
                    return QuitCommand(split);


                default:
                    return NetworkFlags.InvalidCommandFlag;
            }
        }
        

        private string UsernameCommand(string[] cmd)
        {
            return !CheckArgumentCount(cmd, 2) ? NetworkFlags.InvalidCommandFlag : Client.Login(cmd[1], true);
        }
        private string PasswordCommand(string[] cmd)
        {
            return !CheckArgumentCount(cmd, 2) ? NetworkFlags.InvalidCommandFlag : Client.Login(cmd[1], false);
        }
        private string ListCommand(string[] cmd)
        {
            return !CheckArgumentCount(cmd, 2) ? NetworkFlags.InvalidCommandFlag : Client.ListDirectory(cmd[1]);
        }
        private string RetrieveFileCommand(string[] cmd)
        {
            return !CheckArgumentCount(cmd, 2) ? NetworkFlags.InvalidCommandFlag : Client.SendFileToClientAsync(cmd[1]);
        }
        private string StoreFileCommand(string[] cmd)
        {
            return !CheckArgumentCount(cmd, 2) ? NetworkFlags.InvalidCommandFlag : Client.ReceiveFileFromClientAsync(cmd[1]);
        }
        private string DeleteFileCommand(string[] cmd)
        {
            return !CheckArgumentCount(cmd, 2) ? NetworkFlags.InvalidCommandFlag : Client.DeleteDirectory(cmd[1]);
        }
        private string RemoveDirectoryCommand(string[] cmd)
        {
            return !CheckArgumentCount(cmd, 2) ? NetworkFlags.InvalidCommandFlag : Client.DeleteDirectory(cmd[1]);
        }
        private string MakeDirectoryCommand(string[] cmd)
        {
            return !CheckArgumentCount(cmd, 2) ? NetworkFlags.InvalidCommandFlag : Client.CreateDirectory(cmd[1]);
        }
        private string CurrentDirectoryPathCommand(string[] cmd)
        {
            return !CheckArgumentCount(cmd, 1) ? NetworkFlags.InvalidCommandFlag : Client.GetCurrentDirectoryPath();
        }
        private string ChangeDirectoryCommand(string[] cmd)
        {
            return !CheckArgumentCount(cmd, 2) ? NetworkFlags.InvalidCommandFlag : Client.ChangeDirectory(cmd[1]);
        }
        private string GoToParentDirectoryCommand(string[] cmd)
        {
            return !CheckArgumentCount(cmd, 1) ? NetworkFlags.InvalidCommandFlag : Client.GoToParentDirectory();
        }
        private string QuitCommand(string[] cmd)
        {
            return !CheckArgumentCount(cmd, 1) ? NetworkFlags.InvalidCommandFlag : Client.CloseControlSocket();
        }

        private bool CheckArgumentCount(string[] cmd, int count) => cmd.Length == count;
    }
}