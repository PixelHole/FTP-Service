using System;
using System.Net.Sockets;
using System.Threading;
using FTP_Server.Database;
using FTP_Server.Database.DataTypes;
using FTP_Server.File_System;
using Message_Board.Network;

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

        
        private string TempUsernameStorage { get; set; } = string.Empty;
        private User TempUser { get; set; } = null;

        private Folder CurrentDirectory { get; set; } = null;
        
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
        
        public string SendFileToClient(string filePath)
        {
            // use separate thread
            return NetworkCommunication.SendFileOverNetwork(DataSocket, filePath);
        }
        public string ReceiveFileFromClient(string filePath)
        {
            // use separate thread
            return NetworkCommunication.ReceiveFileFromNetwork(DataSocket, filePath);
        }
        
        //      Authorization
        public string Login(string data, bool mode)
        {
            switch (mode)
            {
                case false:
                {
                    if (string.IsNullOrEmpty(TempUsernameStorage)) return NetworkFlags.InvalidCommandFlag;
                    
                    if (!TempUser.CheckInfo(TempUsernameStorage, data)) return NetworkFlags.InvalidLoginInfoFlag;

                    UserInfo = TempUser;

                    TempUser = null;
                    TempUsernameStorage = string.Empty;
                    
                    return NetworkFlags.LoginSuccessFlag;
                }
                
                case true:
                {
                    TempUsernameStorage = data;
                    TempUser = UserDatabase.FindUserByUsername(data);

                    if (TempUser != null) return NetworkFlags.UsernameAcceptedFlag;
                
                    TempUsernameStorage = string.Empty;
                    return NetworkFlags.InvalidLoginInfoFlag;
                }
            }

            return string.Empty;
        }
        public string Logout()
        {
            UserInfo = null;
            // just in case ↓
            TempUser = null;
            TempUsernameStorage = string.Empty;
            // just in case ↑
            return NetworkFlags.ExecutionSuccessFlag;
        }
        public string CloseControlSocket()
        {
            ShutdownControlSocket();
            Logout();
            return NetworkFlags.ExecutionSuccessFlag;
        }
        
        //      File Manipulation
        public string DeleteFileOrDirectory(string path)
        {
            throw new NotImplementedException();
        }
        public string CreateDirectory(string name, string path) 
        {
            throw new NotImplementedException();
        }
        
        //      Directory Manipulation
        public string ListCurrentDirectory()
        {
            throw new NotImplementedException();
        }
        public string GetCurrentDirectory()
        {
            return FileManager.SystemToRootRelativePath(CurrentDirectory.Path);
        }
        public string ChangeDirectory(string path)
        {
            throw new NotImplementedException();
        }
        public string GoToParentDirectory()
        {
            throw new NotImplementedException();
        }


        
        // internal functions
        public void ShutdownControlSocket()
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