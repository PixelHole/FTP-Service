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
    public FileData[] CurrentDirectoryContent { get; private set; }

    public string AccountUsername { get; private set; } = string.Empty;
    
    
    public MainWindowControl(MainWindow view)
    {
        View = view;

        Thread setupThread = new Thread(ApplicationSetup);
        setupThread.Start();
    }
    private void ApplicationSetup()
    {
        ConnectEvents();
        LocalRepositoryManager.IndexLocalRepo();
        UpdateLocalFilesList();
        ConnectToControlServer();
        GetCurrentDirectory();
        GetCurrentDirContentAndUpdateList();
    }


    // Commands
    
    //      File selection Interpreter 
    public void OnServerFileSelected(int row)
    {
        if (row == 0)
        {
            GoToParentDirectory();
            OnServerDirectoryChange();
            return;
        }
        row--;

        FileData fileData = CurrentDirectoryContent[row];

        if (fileData.IsFolder)
        {
            ChangeDirectory(fileData.Path);
            return;
        }
        int choice = View.ShowConfirmationDialog("Download", "Download this file from the server?");
        
        if (choice == 0) FetchFileFromServer(fileData);
    }
    public void OnLocalFileSelected(int row)
    {
        FileData fileData = LocalRepositoryManager.LocalFiles[row];
        
        SendFileToServer(fileData);
    }
    public void OnServerDeleteAction(int row)
    {
        if (row == 0) return;

        row--;
        
        FileData selected = CurrentDirectoryContent[row];

        if (selected.IsFolder)
        {
            DeleteDirectory(selected.Path);
            return;
        }
        
        DeleteFile(selected.Path);
    }

    //      Directory related
    public void GetListOfCurrentDirectory()
    {
        string cmd = GenerateCommand("List", CurrentDirectoryPath);

        string json = SendCommandToServerAndGetResult(cmd);

        var filesList = JsonConvert.DeserializeObject<FileData[]>(json);

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
    }
    public void ChangeDirectory(string path)
    {
        string cmd = GenerateCommand("CWD", path);

        string result = SendCommandToServerAndGetResult(cmd);
        
        if (result == NetworkFlags.FileOperationSuccessFlag) OnServerDirectoryChange();
    }
    public void GetCurrentDirContentAndUpdateList()
    {
        Thread updateThread = new Thread(() =>
        {
            GetListOfCurrentDirectory();
            UpdateServerFilesList();
        });
        updateThread.Start();
    }
    public void DeleteDirectory(string path)
    {
        string cmd = GenerateCommand("RMD", path);

        string result = SendCommandToServerAndGetResult(cmd);

        if (result == NetworkFlags.FileOperationSuccessFlag)
        {
            GetCurrentDirContentAndUpdateList();
            View.ShowErrorMessage("Success", "Directory deleted");
            return;
        }
        
        View.ShowErrorMessage("Failure", "Couldn't delete directory");
    }
    public void DeleteFile(string path)
    {
        string cmd = GenerateCommand("DELE", path);

        string result = SendCommandToServerAndGetResult(cmd);

        if (result == NetworkFlags.FileOperationSuccessFlag)
        {
            GetCurrentDirContentAndUpdateList();
            View.ShowErrorMessage("Success", "File deleted");
            return;
        }
        
        View.ShowErrorMessage("Failure", "Couldn't delete file");
    }
    public void CreateDirectory(string name)
    {
        string cmd = GenerateCommand("MKD", GenerateFileServerPath(name));
        
        string result = SendCommandToServerAndGetResult(cmd);

        if (result == NetworkFlags.FileOperationSuccessFlag)
        {
            GetCurrentDirContentAndUpdateList();
            View.ShowErrorMessage("Success", "Folder created");
            return;
        }
        
        View.ShowErrorMessage("Failure", "Couldn't create folder");
    }
    
    
    //      Account
    public void Logout()
    {
        if (string.IsNullOrEmpty(AccountUsername)) return;
        
        string cmd = "LOGO";

        string result = SendCommandToServerAndGetResult(cmd);
        
        if (result == NetworkFlags.ExecutionSuccessFlag) View.SetAccountStatus(false);

        AccountUsername = string.Empty;

        GetCurrentDirContentAndUpdateList();
    }
    public bool Login(string username, string password)
    {
        string cmd = GenerateCommand("USER", username);

        string result = SendCommandToServerAndGetResult(cmd);
        
        if (result != NetworkFlags.UsernameAcceptedFlag) return false;

        cmd = GenerateCommand("PASS", password);

        result = SendCommandToServerAndGetResult(cmd);
        
        if (result != NetworkFlags.LoginSuccessFlag) return false;
        
        AccountUsername = username;
        View.SetAccountStatus(true);

        GetCurrentDirContentAndUpdateList();
        
        return true;
    }
    
    private string SendCommandToServerAndGetResult(string cmd)
    {
        ServerConnection.SendToServer(cmd);

        return ServerConnection.ReceiveFromServer();
    }
    
    //      Session
    public void TerminateApplication()
    {
        ServerConnection.TerminateSession();
        Application.RequestStop(SessionData.MainInstance);
    }
    public void RestartApplication()
    {
        SessionData.RestartRequested = true;
        Application.RequestStop(SessionData.MainInstance);
    }

    //      File
    public void SendFileToServer(FileData fileData)
    {
        string cmd = GenerateCommand("STOR", GenerateFileServerPath(fileData.Name));

        string res = SendCommandToServerAndGetResult(cmd);
        
        if (res != NetworkFlags.FileTransferFlag) return;
        
        View.OpenTransferDialog("Uploading...");

        bool transferResult = ServerConnection.SendFileToServer(fileData.Path);
        
        if (transferResult) UploadSuccessHandler();
        else UploadFailedHandler();
        
        View.CloseTransferDialog();
    }
    public void FetchFileFromServer(FileData fileData)
    {
        string cmd = GenerateCommand("RETR", fileData.Path);

        string result = SendCommandToServerAndGetResult(cmd);

        if (result != NetworkFlags.FileTransferFlag) return;
        
        View.OpenTransferDialog("Downloading...");

        var transferResult = ServerConnection.ReceiveFileFromServer(GenerateFileSavePath(fileData.Name));

        if (transferResult)
        {
            DownloadSuccessHandler(fileData.Path);
        }
        else
        {
            DownloadFailedHandler();
        }

        View.CloseTransferDialog();
    }
    

    // utility
    private void ConnectEvents()
    {
        ServerConnection.OnControlConnected += OnControlConnected;
        ServerConnection.OnControlDisconnected += OnControlDisconnected;
    }
    private List<string[]> listOfFilesToStringList(FileData[] files)
    {
        List<string[]> content = new List<string[]>();

        foreach (var file in files)
        {
            content.Add(new []{file.Name, file.Extension, file.Path});
        }

        return content;
    }
    private string GenerateCommand(string header, string body) => $"{header}{NetworkFlags.SeparatorFlag}{body}";
    private string GenerateFileSavePath(string fileName) => LocalRepositoryManager.LocalRepositoryPath + "\\" + fileName;
    private string GenerateFileServerPath(string fileName) => CurrentDirectoryPath + "\\" + fileName;


    // Internal
    private void ConnectToControlServer()
    {
        ServerConnection.InitiateConnection();
    }
    private void OnServerDirectoryChange()
    {
        GetCurrentDirectory();
        GetCurrentDirContentAndUpdateList();
    }
    
    private void DownloadFailedHandler()
    {
        View.ShowErrorMessage("Fail", "Failed to download file");
    }
    private void DownloadSuccessHandler(string path)
    {
        LocalRepositoryManager.IndexFileAt(path);
        View.ShowErrorMessage("Success", "Successfully downloaded file");
    }
    private void UploadFailedHandler()
    {
        View.ShowErrorMessage("Fail", "Failed to upload file");
    }
    private void UploadSuccessHandler()
    {
        View.ShowErrorMessage("Success", "Successfully uploaded file");
        GetCurrentDirContentAndUpdateList();
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
    public void UpdateLocalFilesList()
    {
        var content = listOfFilesToStringList(LocalRepositoryManager.LocalFiles.ToArray());
        
        View.SetLocalFilesListContent(content);
    }
}