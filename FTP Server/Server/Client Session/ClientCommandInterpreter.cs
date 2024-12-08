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
            
            switch (split[0])
            {
                default:
                    return NetworkFlags.InvalidCommandFlag;
            }
        }
    }
}