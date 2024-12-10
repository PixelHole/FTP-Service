using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using FTP_Server.Database;
using FTP_Server.Database.DataTypes;
using FTP_Server.File_System;
using FTP_Server.File_System.Access_Management;
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

        private Folder CurrentDirectory { get; set; } = FileManager.RootDirectory;
        
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
        
        //      Sending and receiving files
        public string SendFileToClientAsync(string filePath)
        {
            Thread sendThread = new Thread(() =>
            {
                File file = FileManager.GetFileByPath(filePath);

                if (file == null)
                {
                    NetworkCommunication.SendOverNetwork(ControlSocket, NetworkFlags.FileOperationFailureFlag);
                    return;
                }
                
                string result = NetworkCommunication.SendFileOverNetwork(DataSocket, filePath);
                NetworkCommunication.SendOverNetwork(ControlSocket, result);
            });
            sendThread.Start();
            return NetworkFlags.FileTransferFlag;
        }
        public string ReceiveFileFromClientAsync(string filePath)
        {
            Thread receiveThread = new Thread(() =>
            {
                File file = FileManager.CreateFile(filePath, false, UserInfo, AccessType.PrivateBoth);

                if (file == null)
                {
                    NetworkCommunication.SendOverNetwork(ControlSocket, NetworkFlags.FileOperationFailureFlag);
                    return;
                }
                
                string result = NetworkCommunication.ReceiveFileFromNetwork(DataSocket, filePath);
                NetworkCommunication.SendOverNetwork(ControlSocket, result);
            });
            receiveThread.Start();
            return NetworkFlags.FileTransferFlag;
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
        public string DeleteDirectory(string path)
        {
            return !FileManager.DeleteFolder(path, UserInfo)
                ? NetworkFlags.FileOperationFailureFlag
                : NetworkFlags.FileOperationSuccessFlag;
        }
        public string CreateDirectory(string path)
        {
            return FileManager.CreateFolder(path, UserInfo, AccessType.PrivateBoth) == null
                ? NetworkFlags.FileOperationFailureFlag
                : NetworkFlags.FileOperationSuccessFlag;
        }
        
        //      Directory Manipulation
        public string ListDirectory(string path)
        {
            return FileManager.GetListOfFolder(path, UserInfo);
        }
        public string GetCurrentDirectoryPath()
        {
            return FileManager.SystemToRootRelativePath(CurrentDirectory.Path);
        }
        public string ChangeDirectory(string path)
        {
            Folder newDir = FileManager.GetFolderByPath(path);

            if (newDir == null) return NetworkFlags.FileOperationFailureFlag;

            CurrentDirectory = newDir;

            return NetworkFlags.FileOperationSuccessFlag;
        }
        public string GoToParentDirectory()
        {
            var parent = CurrentDirectory.Parent;

            if (parent != null) CurrentDirectory = parent;;

            return NetworkFlags.FileOperationSuccessFlag;
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