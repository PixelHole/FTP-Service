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
            ClientThread.Start();

            Print("client service started");
        }

        private void ServiceLoop()
        {
            NetworkCommunication.SendOverNetwork(ControlSocket, NetworkFlags.ReadyFlag);
            
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
                DataSocket = Connection.EstablishDataConnection();

                if (DataSocket == null)
                {
                    NetworkCommunication.SendOverNetwork(ControlSocket, NetworkFlags.ConnectionFailed);
                    return;
                }
                
                File file = FileManager.GetFileByPath(filePath);

                if (file == null)
                {
                    NetworkCommunication.SendOverNetwork(ControlSocket, NetworkFlags.FileOperationFailureFlag);
                    return;
                }
                
                Print("Sending file to client...");
                
                string result = NetworkCommunication.SendFileOverNetwork(DataSocket, file.Path);
                
                if (result == NetworkFlags.TransferSuccessFlag) Print("Upload success");
                else Print("Upload failed");

                DataSocket.Shutdown(SocketShutdown.Both);
                DataSocket.Close();

                DataSocket = null;
            });
            sendThread.Start();
            
            return NetworkFlags.FileTransferFlag;
        }
        public string ReceiveFileFromClientAsync(string filePath)
        {
            Thread receiveThread = new Thread(() =>
            {
                DataSocket = Connection.EstablishDataConnection();

                if (DataSocket == null)
                {
                    NetworkCommunication.SendOverNetwork(ControlSocket, NetworkFlags.ConnectionFailed);
                    return;
                }
                
                AccessType access = UserInfo == null ? AccessType.PublicBoth : AccessType.PrivateBoth; 
                
                File file = FileManager.CreateFile(filePath, false, UserInfo, access);

                if (file == null)
                {
                    NetworkCommunication.SendOverNetwork(ControlSocket, NetworkFlags.FileOperationFailureFlag);
                    return;
                }
                
                Print("Receiving file from client");
                
                string result = NetworkCommunication.ReceiveFileFromNetwork(DataSocket, file.Path);

                if (result != NetworkFlags.TransferSuccessFlag)
                {
                    Print("Download failed");
                    FileManager.DeleteFile(filePath, UserInfo);
                }else Print("Download success");
                

                DataSocket.Shutdown(SocketShutdown.Both);
                DataSocket.Close();

                DataSocket = null;
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

                    Print($"user logged in as {TempUsernameStorage}");
                    
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
            
            Print("User logged out");
            
            return NetworkFlags.ExecutionSuccessFlag;
        }
        public string CloseControlSocket()
        {
            EndService();
            Logout();
            return NetworkFlags.ExecutionSuccessFlag;
        }

        //      File Manipulation
        public string DeleteDirectory(string path)
        {
            var result = FileManager.DeleteFolder(path, UserInfo);

            if (!result)
            {
                Print($"Tried to delete folder but failed\n\tpath : {path}");
                return NetworkFlags.FileOperationFailureFlag;
            }
            
            Print($"Deleted folder at path : {path}");
            return NetworkFlags.FileOperationSuccessFlag;
        }
        public string CreateDirectory(string path)
        {
            AccessType access = UserInfo == null ? AccessType.PublicBoth : AccessType.PrivateBoth; 
            
            var folder = FileManager.CreateFolder(path, UserInfo, access);

            if (folder == null)
            {
                Print($"wanted to create a directory but failed\n\tpath : {path}");
                return NetworkFlags.FileOperationFailureFlag;
            }

            Print($"Created folder at : {folder.Path}");
            return NetworkFlags.FileOperationSuccessFlag;
        }

        public string DeleteFile(string path)
        {
            var result = FileManager.DeleteFile(path, UserInfo);
            
            if (!result)
            {
                Print($"Tried to delete file but failed\n\tpath : {path}");
                return NetworkFlags.FileOperationFailureFlag;
            }
            
            Print($"Deleted file at path : {path}");
            return NetworkFlags.FileOperationSuccessFlag;
        }
        
        //      Directory Manipulation
        public string ListDirectory(string path)
        {
            Print($"Requested list of files at {path}");
            return FileManager.GetListOfFiles(path, UserInfo);
        }
        public string GetCurrentDirectoryPath()
        {
            Print("Fetched current directory");
            return FileManager.SystemToRootRelativePath(CurrentDirectory.Path);
        }
        public string ChangeDirectory(string path)
        {
            if (!FileManager.IsPathRootRelative(path)) return NetworkFlags.FileOperationFailureFlag;
            
            Folder newDir = FileManager.GetFolderByPath(path);

            if (newDir == null) return NetworkFlags.FileOperationFailureFlag;

            Print($"Changed directory\n\tfrom : {CurrentDirectory.Path}\n\tto : {newDir.Path}");
            
            CurrentDirectory = newDir;

            return NetworkFlags.FileOperationSuccessFlag;
        }
        public string GoToParentDirectory()
        {
            var parent = CurrentDirectory.Parent;

            if (parent != null)
            {
                Print($"Went to parent directory\n\tPath : {parent?.Path}");
                CurrentDirectory = parent;
            }
            else
            {
                Print("Tried to go to parent directory but it was inaccessible");
            }

            return NetworkFlags.FileOperationSuccessFlag;
        }
        
        
        // internal functions
        public void EndService()
        {
            Print("client service ended");
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


        private void Print(string msg)
        {
            Console.WriteLine($"[{Id.ToString()}] : {msg}");
        }
    }
}