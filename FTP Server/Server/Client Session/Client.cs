using System;
using System.Net.Sockets;
using System.Threading;
using FTP_Server.Database.DataTypes;

namespace FTP_Server.Server.Client_Session
{
    public class Client
    {
        private User UserInfo { get; set; }
        public Socket ControlSocket { get; set; }
        public Socket DataSocket { get; set; }
        private Thread ClientThread { get; set; }
        public Guid Id { get; private set; } = new Guid();
        public bool IsRunning { get; private set; } = true;

        private ClientConnection Connection { get; set; }
        private ClientCommandInterpreter CommandInterpreter { get; set; }

        
        public Client(Socket controlSocket)
        {
            ControlSocket = controlSocket;
            Connection = new ClientConnection(this);
            CommandInterpreter = new ClientCommandInterpreter(this);

            ClientThread = new Thread(ServiceLoop) {Name = "Client Thread"};
        }

        private void ServiceLoop()
        {
            while (IsRunning)
            {
                string cmd = Connection.GetCommandFromClient();
                
                if (!IsRunning) break;

                string result = CommandInterpreter.GetCommandResult(cmd);

                Connection.SendMessageToClient(result);
            }
        }

        
        // Client Commands
        
        public string SendFileToClient()
        {
            // 
            
            throw new NotImplementedException();
        }
        public string ReceiveFileFromClient()
        {
            throw new NotImplementedException();
        }

        public void Shutdown()
        {
            ControlSocket.Shutdown(SocketShutdown.Both);
            ControlSocket.Close();
            
            ClientManager.RemoveClient(this);

            IsRunning = false;
        }

        public override bool Equals(object obj)
        {
            if (obj is Client client) return Id.Equals(client.Id);
            return base.Equals(obj);
        }
    }
}