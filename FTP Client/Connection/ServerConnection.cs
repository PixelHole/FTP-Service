using System.Net;
using System.Net.Sockets;
using Message_Board.Network;

namespace FTP_Client.Connection;

public class ServerConnection
{
    public delegate void ControlConnectedToServerAction();
    public static event ControlConnectedToServerAction? OnControlConnected;

    public delegate void ControlDisconnectedFromServerAction();
    public static event ControlDisconnectedFromServerAction? OnControlDisconnected;
    
    public delegate void DataConnectedToServerAction();
    public static event DataConnectedToServerAction? OnDataConnected;

    public delegate void DataDisconnectedFromServerAction();
    public static event DataDisconnectedFromServerAction OnDataDisconnected;



    public static Socket ControlSocket { get; set; }
    public static Socket DataSocket { get; set; }

    private static IPEndPoint ControlEndPoint { get; set; } 
        = new (ServerInformation.IpAddress, ServerInformation.FtpControlPort);
    private static IPEndPoint DataEndPoint { get; set; } 
        = new (ServerInformation.IpAddress, ServerInformation.FtpDataPort);
    
    
    public static bool IsControlConnected { get; private set; } = false;
    public static bool IsDataConnected { get; private set; } = false;

    public static int ErrorTolerance { get; private set; } = 100;
    public static int ErrorWaitTime { get; private set; } = 500;


    public static void InitiateConnection()
    {
        ConnectInternalEventActions();
        
        CreateSockets();
        while (!IsControlConnected)
        {
            ConnectToControlServer();
        }
    }
    private static void ConnectInternalEventActions()
    {
        OnControlConnected += () => IsControlConnected = true;
        OnControlDisconnected += () => IsControlConnected = false;

        OnDataConnected += () => IsDataConnected = true;
        OnDataDisconnected += () => IsDataConnected = false;
    }
    
    //  Socket handling
    private static void CreateSockets()
    {
        ControlSocket = new Socket(ServerInformation.IpAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        DataSocket = new Socket(ServerInformation.IpAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
    }
    private static void CloseControlConnection()
    {
        if (!ControlSocket.Connected) return;
        ControlSocket.Shutdown(SocketShutdown.Both);
        ControlSocket.Close();
    }
    private static void CloseDataConnection()
    {
        if (!DataSocket.Connected) return;
        DataSocket.Shutdown(SocketShutdown.Both);
        DataSocket.Close();
    }


    // command handling
    public static string SendToServer(string cmd)
    {
        return NetworkCommunication.SendOverNetwork(ControlSocket, cmd);
    }
    public static string ReceiveFromServer()
    {
        return NetworkCommunication.ReceiveFromSocket(ControlSocket);
    }

    //  File handling
    public static bool ReceiveFileFromServer(string savePath)
    {
        ConnectToDataServer();

        string result = NetworkCommunication.ReceiveFileFromNetwork(DataSocket, savePath);

        TerminateDataConnection();
            
        if (result == NetworkFlags.TransferSuccessFlag)
        {
            return true;
        }

        return false;
    }
    public static bool SendFileToServer(string filePath)
    {
        ConnectToDataServer();

        string result = NetworkCommunication.SendFileOverNetwork(DataSocket, filePath);

        TerminateDataConnection();

        if (result == NetworkFlags.TransferSuccessFlag)
        {
            return true; 
        }

        return false;
    }


    //  Connection
    private static void ConnectToControlServer()
    {
        if (!ConnectSocketToEndPoint(ControlSocket, ControlEndPoint)) return;
        string ready = NetworkCommunication.ReceiveFromSocket(ControlSocket);
        if (ready == NetworkFlags.ReadyFlag) OnControlConnected();
    }
    private static void ConnectToDataServer()
    {
        if (ConnectSocketToEndPoint(DataSocket, DataEndPoint)) OnDataConnected();
    }

    private static void TerminateControlConnection()
    {
        try
        {
            if (ControlSocket.Connected) ControlSocket.Disconnect(true);
        }
        catch (Exception)
        {
            ControlSocket = new Socket(ServerInformation.IpAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        }
    }
    private static void TerminateDataConnection()
    {
        try
        {
            if (DataSocket.Connected) DataSocket.Disconnect(true);
        }
        catch (Exception)
        {
            DataSocket = new Socket(ServerInformation.IpAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        }
    }


    public static void TerminateSession()
    {
        TerminateControlConnection();
        TerminateDataConnection();
        
        CloseControlConnection();
        CloseDataConnection();
    }
    
    private static bool ConnectSocketToEndPoint(Socket socket, IPEndPoint endPoint)
    {
        int errorCount = 0;
        
        while (true)
        {
            try
            {
                socket.Connect(endPoint);
                return true;
            }
            catch (Exception)
            {
                if (errorCount == ErrorTolerance)
                {
                    return false;
                }
                
                errorCount++;
                Thread.Sleep(ErrorWaitTime);
            }
        }
    }
}