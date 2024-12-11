using FileInformation;
using FTP_Client.Connection;
using Message_Board.Network;
using Newtonsoft.Json;
using Terminal.Gui;

namespace FTP_Client.Windows.Controls;

public class MainWindowControl
{
    private MainWindow View { get; set; }
    
    public string CurrentDirectoryPath { get; private set; }
    public ListOfFiles CurrentDirectoryContent { get; private set; }
    
    
    public MainWindowControl(MainWindow view)
    {
        View = view;

        Thread setupThread = new Thread(() =>
        {
            ConnectEvents();
            ConnectToControlServer();
            GetCurrentDirectory();
            GetCurrentDirectoryContentAndUpdate();
        });
        setupThread.Start();
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
    
    
    // Commands
    public void GetListOfCurrentDirectory()
    {
        string cmd = $"LIST{NetworkFlags.SeparatorFlag}{CurrentDirectoryPath}";

        string json = SendCommandToServerAndGetResult(cmd);

        var filesList = JsonConvert.DeserializeObject<FileItem[]>(json);

        CurrentDirectoryContent = new ListOfFiles(filesList);
    }
    public void GetCurrentDirectory()
    {
        string cmd = "PWD";

        string newDirectory = SendCommandToServerAndGetResult(cmd);

        CurrentDirectoryPath = newDirectory;
    }
    public void GetCurrentDirectoryContentAndUpdate()
    {
        GetListOfCurrentDirectory();
        UpdateServerFilesList();
    }
    private string SendCommandToServerAndGetResult(string cmd)
    {
        ServerConnection.SendToServer(cmd);

        return ServerConnection.ReceiveFromServer();
    }
    
    
    // utility
    private void ConnectEvents()
    {
        ServerConnection.OnControlConnected += OnControlConnected;
        ServerConnection.OnControlDisconnected += OnControlDisconnected;
    }
    
    
    // Internal
    private void ConnectToControlServer()
    {
        ServerConnection.InitiateConnection();
    }


    // View control
    //      Connection Status Label
    private void OnControlConnected()
    {
        View.SetConnectionStatus(true);
    }
    private void OnControlDisconnected()
    {
        View.SetConnectionStatus(false);
    }
    
    //      Update Server files list
    private void UpdateServerFilesList()
    {
        List<string[]> content = new List<string[]>();

        foreach (var file in CurrentDirectoryContent.FilesList)
        {
            content.Add(new []{file.Name, file.Extension, file.ServerPath});
        }
        
        View.SetServerFilesListContent(content);
    }
}