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
            
            switch (split[0])
            {
                case "user" :
                    return UsernameCommand(split);
                case "pass" :
                    return PasswordCommand(split);
                case "list" :
                    return ListCommand(split);
                    
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
        private string RetrieveFileCommand()
        {
            throw new NotImplementedException();
        }
        private string StoreFileCommand()
        {
            throw new NotImplementedException();
        }
        private string DeleteDirectoryCommand(string[] cmd)
        {
            return !CheckArgumentCount(cmd, 2) ? NetworkFlags.InvalidCommandFlag : Client.DeleteDirectory(cmd[1]);
        }
        private string MakeDirectoryCommand(string[] cmd)
        {
            return !CheckArgumentCount(cmd, 2) ? NetworkFlags.InvalidCommandFlag : Client.CreateDirectory(cmd[1]);
        }
        private string DeleteFile(string[] cmd)
        {
            throw new NotImplementedException();
        }

        private bool CheckArgumentCount(string[] cmd, int count) => cmd.Length == count;
    }
}