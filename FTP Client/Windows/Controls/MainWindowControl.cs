using FileInformation;
using FTP_Client.Connection;
using FTP_Client.LocalRepository;
using Message_Board.Network;
using Newtonsoft.Json;
using Terminal.Gui;

namespace FTP_Client.Windows.Controls;

public class MainWindowControl
{
    private MainWindow View { get; set; }
    
    public string CurrentDirectoryPath { get; private set; }
    public FileItem[] CurrentDirectoryContent { get; private set; }
    
    
    public MainWindowControl(MainWindow view)
    {
        View = view;

        Thread setupThread = new Thread(ApplicationSetup);
        setupThread.Start();
    }
    private void ApplicationSetup()
    {
        ConnectEvents();
        ConnectToControlServer();
        GetCurrentDirectory();
        GetCurrentDirectoryContentAndUpdate();
        LocalRepositoryManager.IndexLocalRepo();
        UpdateLocalFilesList();
    }
    
    
    // Commands
    
    //      File selection Interpreter 
    public void OnServerFileSelected(int row)
    {
        if (row == 0)
        {
            GoToParentDirectory();
            return;
        }
        row--;

        FileItem fileItem = CurrentDirectoryContent[row];

        if (fileItem.IsFolder)
        {
            ChangeDirectory(fileItem.Path);
            return;
        }
        
        // receive file from server
    }
    
    //      Directory related

    public void GetListOfCurrentDirectory()
    {
        string cmd = GenerateCommand("List", CurrentDirectoryPath);

        string json = SendCommandToServerAndGetResult(cmd);

        var filesList = JsonConvert.DeserializeObject<FileItem[]>(json);

        if (filesList == null) return;
        
        CurrentDirectoryContent = filesList;
    }
    public void GetCurrentDirectory()
    {
        string cmd = "PWD";

        string newDirectory = SendCommandToServerAndGetResult(cmd);

        CurrentDirectoryPath = newDirectory;
    }
    public void GoToParentDirectory()
    {
        string cmd = "CDUP";

        SendCommandToServerAndGetResult(cmd);
        
        OnServerDirectoryChange();
    }
    public void ChangeDirectory(string path)
    {
        string cmd = GenerateCommand("CWD", path);

        string result = SendCommandToServerAndGetResult(cmd);
        
        if (result == NetworkFlags.FileOperationSuccessFlag) OnServerDirectoryChange();
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
    private List<string[]> listOfFilesToStringList(FileItem[] files)
    {
        List<string[]> content = new List<string[]>();

        foreach (var file in files)
        {
            content.Add(new []{file.Name, file.Extension, file.Path});
        }

        return content;
    }
    private string GenerateCommand(string header, string body) => $"{header}{NetworkFlags.SeparatorFlag}{body}";
    
    
    // Internal
    private void ConnectToControlServer()
    {
        ServerConnection.InitiateConnection();
    }
    private void OnServerDirectoryChange()
    {
        GetCurrentDirectory();
        GetCurrentDirectoryContentAndUpdate();
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
        var content = listOfFilesToStringList(CurrentDirectoryContent);
        
        View.SetServerFilesListContent(content);
    }
    //      Update local files list
    private void UpdateLocalFilesList()
    {
        var content = listOfFilesToStringList(LocalRepositoryManager.LocalFiles.ToArray());
        
        View.SetLocalFilesListContent(content);
    }
}