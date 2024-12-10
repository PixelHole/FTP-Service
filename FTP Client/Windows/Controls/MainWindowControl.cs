using FTP_Client.Connection;
using Message_Board.Network;
using Terminal.Gui;

namespace FTP_Client.Windows.Controls;

public class MainWindowControl : MainWindow
{
    public MainWindowControl()
    {
        ConnectEvents();
        ConnectToControlServer();
    }

    private void Test()
    {
        string ready = ServerConnection.ReceiveFromServer();
        
        if (ready != NetworkFlags.ReadyFlag) return;
        
        // string filePath = "..\\public files\\some text file.txt";
        string filePath = "..\\img-300687.jpg";
        string cmd = $"RETR{NetworkFlags.SeparatorFlag}{filePath}";

        ServerConnection.SendToServer(cmd);

        string result = ServerConnection.ReceiveFromServer();
        
        if (result != NetworkFlags.FileTransferFlag) return;

        // ServerConnection.ReceiveFileFromServer("M:\\FTP Client repo\\tst.txt");
        ServerConnection.ReceiveFileFromServer("M:\\FTP Client repo\\img.jpeg");
    }
    
    
    private void ConnectEvents()
    {
        ServerConnection.OnControlConnected += OnControlConnected;
        ServerConnection.OnControlDisconnected += OnControlDisconnected;
    }
    private void ConnectToControlServer()
    {
        Thread connectionThread = new Thread(() =>
        {
            ServerConnection.InitiateConnection();
            // Test();
        });
        connectionThread.Start();
    }
    
    private void OnControlConnected()
    {
        SetConnectionStatus(true);
    }
    private void OnControlDisconnected()
    {
        SetConnectionStatus(false);
    }
}