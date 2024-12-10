using System.Net;
using System.Net.Sockets;
using Message_Board.Network;

namespace FTP_Client.Connection;

public class ServerConnection
{
    public delegate void ControlConnectedToServerAction();
    public static event ControlConnectedToServerAction OnControlConnected;

    public delegate void ControlDisconnectedFromServerAction();
    public static event ControlDisconnectedFromServerAction OnControlDisconnected;
    
    public delegate void DataConnectedToServerAction();
    public static event DataConnectedToServerAction OnDataConnected;

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

    // command handling
    public static string SendToServer(string cmd)
    {
        return NetworkCommunication.SendOverNetwork(ControlSocket, cmd);
    }
    public static string ReceiveFromServer()
    {
        return NetworkCommunication.ReceiveFromSocket(ControlSocket);
    }

    public static bool ReceiveFileFromServer(string savePath)
    {
        ConnectToDataServer();

        string result = NetworkCommunication.ReceiveFileFromNetwork(DataSocket, savePath);

        return result == NetworkFlags.TransferSuccessFlag;
    }
    
    
    private static void CreateSockets()
    {
        ControlSocket = new Socket(ServerInformation.IpAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        DataSocket = new Socket(ServerInformation.IpAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
    }
    private static void ConnectToControlServer()
    {
        if (ConnectSocketToEndPoint(ControlSocket, ControlEndPoint)) OnControlConnected();
    }
    private static void ConnectToDataServer()
    {
        if (ConnectSocketToEndPoint(DataSocket, DataEndPoint)) OnDataConnected();
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