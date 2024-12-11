using System.Collections.ObjectModel;
using System.Data;
using FTP_Client.Windows.Controls;
using Terminal.Gui;

namespace FTP_Client.Windows;

public class MainWindow : Window
{
    private MainWindowControl Control { get; set; }
    
    private Window AccountLoginDialog { get; set; }
    
    private TextField UsernameField { get; set; }
    private TextField PasswordField { get; set; }
    private Button AccOkBtn { get; set; }
    private Button AccCancelBtn { get; set; }
    

    private Window FileTransferDialog { get; set; }
    private ProgressBar TransferProgressBar { get; set; }

    private Label AccountStatus { get; set; }
    private Label ConnectionStatus { get; set; }
    private TableView LocalFilesList { get; set; }
    private TableView ServerFilesList { get; set; }

    public MainWindow()
    {
        SetupElements();
        ConnectEvents();
        
        Control = new MainWindowControl(this);
    }

    private void SetupElements()
    {
        SetupMainWindow();
        SetupAccountDialog();
        SetupFileTransferDialog();
    }
    private void SetupMainWindow()
    {
        var menu = new MenuBar();
        menu.Menus = new[]
        {
            new MenuBarItem("File", new []
            {
                new MenuItem("Restart", "", RestartHandler),
                new MenuItem("Exit", "", ExitHandler),
            }),
            new MenuBarItem("Account", new []
            {
                new MenuItem("Login", "", OpenAccountDialog),
                new MenuItem("Logout", "", OpenLogoutDialog)
            }),
            new MenuBarItem("Server", new []
            {
                new MenuItem("Reconnect", "", ReconnectHandler),
                new MenuItem("Refresh", "", RefreshHandler),
                new MenuItem("Disconnect", "", DisconnectHandler),
            })
        };
        
        ConnectionStatus = new Label()
        {
            Text = "Connection Status : X",
            X = 0,
            Y = Pos.Bottom(menu)
        };
        Label separator = new Label()
        {
            Text = "|",
            X = Pos.Right(ConnectionStatus) + 4,
            Y = Pos.Y(ConnectionStatus)
        };
        AccountStatus = new Label()
        {
            Text = "Account Status : Not logged in",
            X = Pos.Right(separator) + 4,
            Y = Pos.Y(ConnectionStatus)
        };

        ServerFilesList = new TableView()
        {
            X = 0,
            Y = Pos.Bottom(ConnectionStatus),
            Width = Dim.Percent(60),
            Height = Dim.Percent(80),
            Title = "Server Files",
            BorderStyle = LineStyle.Rounded,
            CellActivationKey = KeyCode.Enter,
            Style = new TableStyle()
            {
                ShowHorizontalBottomline = true
            }
        };

        LocalFilesList = new TableView()
        {
            X = Pos.Right(ServerFilesList),
            Y = Pos.Y(ServerFilesList),
            Width = Dim.Fill(),
            Height = Dim.Height(ServerFilesList),
            Title = "Local Files",
            BorderStyle = LineStyle.Rounded,
            CellActivationKey = KeyCode.Enter,
            Style = new TableStyle()
            {
                ShowHorizontalBottomline = true
            }
        };

        Add(menu, ConnectionStatus, separator, AccountStatus, ServerFilesList, LocalFilesList);
    }
    private void SetupAccountDialog()
    {
        AccountLoginDialog = new Dialog()
        {
            X = Pos.Align(Alignment.Center),
            Y = Pos.Align(Alignment.Center),
            Width = 60,
            Height = 12,
            Title = "Account Login",
            BorderStyle = LineStyle.Rounded,
            ShadowStyle = ShadowStyle.Transparent,
            Visible = false
        };

        Label usernameLabel = new Label()
        {
            X = 4,
            Y = 2,
            Text = "Username"
        };
        UsernameField = new TextField()
        {
            X = Pos.Right(usernameLabel) + 1,
            Y = Pos.Y(usernameLabel),
            Width = Dim.Fill() - 4
        };
        Label passwordLabel = new Label()
        {
            X = Pos.X(usernameLabel),
            Y = Pos.Y(usernameLabel) + 2,
            Text = "Password"
        };
        PasswordField = new TextField()
        {
            X = Pos.X(UsernameField),
            Y = Pos.Y(passwordLabel),
            Width = Dim.Width(UsernameField)
        };
        
        AccCancelBtn = new Button()
        {
            X = Pos.Align(Alignment.Center),
            Y = Pos.Align(Alignment.End) - 1,
            Text = "Cancel"
        };

        AccCancelBtn.Accept += (sender, args) => CloseAccountDialog();
        
        AccOkBtn = new Button()
        {
            X = Pos.Align(Alignment.Center),
            Y = Pos.Y(AccCancelBtn),
            Text = "Ok"
        };

        AccOkBtn.Accept += (sender, args) => LoginHandler();

        AccountLoginDialog.Add(usernameLabel, UsernameField, passwordLabel, PasswordField, AccCancelBtn, AccOkBtn);
        
        Add(AccountLoginDialog);
    }
    private void SetupFileTransferDialog()
    {
        FileTransferDialog = new Dialog()
        {
            X = Pos.Align(Alignment.Center),
            Y = Pos.Align(Alignment.Center),
            Width = 40,
            Height = 6,
            BorderStyle = LineStyle.Rounded,
            ShadowStyle = ShadowStyle.Transparent,
            Visible = false
        };

        TransferProgressBar = new ProgressBar()
        {
            X = Pos.Align(Alignment.Center),
            Y = Pos.Align(Alignment.Center),
            Width = Dim.Width(FileTransferDialog) - 4,
            Height = 1,
            ProgressBarStyle = ProgressBarStyle.MarqueeBlocks,
            ProgressBarFormat = ProgressBarFormat.SimplePlusPercentage
        };

        TransferProgressBar.Fraction = 0.4f;

        FileTransferDialog.Add(TransferProgressBar);

        Add(FileTransferDialog);
    }
    private void ConnectEvents()
    {
        ServerFilesList.CellActivated += ServerCellActionHandler;
        LocalFilesList.CellActivated += LocalCellActionHandler;
        
        Closing += OnClosing;
    }


    // UI updates
    
    //      File Lists
    public void SetServerFilesListContent(List<string[]> content)
    {
        DataTable serverDt = new DataTable();
        serverDt.Columns.Add("Name");
        serverDt.Columns.Add("Extension");
        serverDt.Columns.Add("Path");

        serverDt.Rows.Add("...", "", "");
        
        foreach (var row in content)
        {
            serverDt.Rows.Add(row[0], row[1], row[2]);
        }
        
        ServerFilesList.Table = new DataTableSource(serverDt);
    }
    public void SetLocalFilesListContent(List<string[]> content)
    {
        DataTable localDt = new DataTable();
        localDt.Columns.Add("Name");
        localDt.Columns.Add("Extension");
        localDt.Columns.Add("Path");

        foreach (var row in content)
        {
            localDt.Rows.Add(row[0], row[1], row[2]);
        }
        
        LocalFilesList.Table = new DataTableSource(localDt);
    }
    
    //      Status Labels
    public void SetConnectionStatus(bool connection)
    {
        string connectionText = connection ? "✓" : "X";
        ConnectionStatus.Text = $"Connection Status : {connectionText}";
    }
    public void SetAccountStatus(bool logged)
    {
        string accountText = logged ? $"Logged in as {Control.AccountUsername}" : "Not Logged in";
        AccountStatus.Text = $"Account Status : {accountText}";
    }
    
    //      Open and close Dialogs
    public void OpenAccountDialog()
    {
        AccountLoginDialog.X = Pos.Align(Alignment.Center);
        AccountLoginDialog.Y = Pos.Align(Alignment.Center);
        Add(AccountLoginDialog);
    }
    public void CloseAccountDialog()
    {
        UsernameField.Text = "";
        PasswordField.Text = "";
        
        Remove(AccountLoginDialog);
    }
    public void OpenTransferDialog(string title)
    {
        FileTransferDialog.X = Pos.Align(Alignment.Center);
        FileTransferDialog.Y = Pos.Align(Alignment.Center);

        FileTransferDialog.Title = title;

        FileTransferDialog.Visible = true;
    }
    public void CloseTransferDialog()
    {
        TransferProgressBar.Fraction = 0;

        FileTransferDialog.Visible = false;
    }
    private void OpenLogoutDialog()
    {
        int choice = ShowConfirmationDialog("Log out", "Are you sure?");
        
        if (choice == 0) LogoutHandler();
    }


    // input response
    private void ServerCellActionHandler(object? sender, CellActivatedEventArgs e)
    {
        int choice = ShowConfirmationDialog("Download", "Download this file from the server?");
        
        if (choice == 0) Control.OnServerFileSelected(e.Row);
    }
    private void LocalCellActionHandler(object? sender, CellActivatedEventArgs e)
    {
        int choice = ShowConfirmationDialog("Upload", "upload this file to the server?");
        
        if (choice == 0) Control.OnLocalFileSelected(e.Row);
    }

    private void LoginHandler()
    {
        if (string.IsNullOrEmpty(UsernameField.Text))
        {
            ShowErrorMessage("Error", "please enter a username");
            return;
        }
        if (string.IsNullOrEmpty(PasswordField.Text))
        {
            ShowErrorMessage("Error", "please enter a password");
            return;
        }

        bool result = Control.Login(UsernameField.Text, PasswordField.Text);

        if (!result)
        {
            ShowErrorMessage("Notice", "Invalid login information");
            return;
        }
        
        CloseAccountDialog();
    }
    private void LogoutHandler()
    {
        Control.Logout();
    }
    
    private void ReconnectHandler()
    {
        
    }
    private void RefreshHandler()
    {
        Control.GetCurrentDirContentAndUpdateList();
    }
    private void DisconnectHandler()
    {
        
    }
    
    private void ExitHandler()
    {
        int choice = ShowConfirmationDialog("Exit", "Are you sure?");
        
        if (choice == 0) Control.TerminateApplication();
    }
    private void RestartHandler()
    {
        Control.RestartApplication();
    }
    
    //  Error box
    public void ShowErrorMessage(string title, string msg)
    {
        MessageBox.Query(title, msg, "Ok");
    }
    public int ShowConfirmationDialog(string title, string msg)
    {
        return MessageBox.Query(title, msg, "Yes", "No");
    }
    
    
    //  Internal event
    private void OnClosing(object? sender, ToplevelClosingEventArgs e)
    { 
        if (!SessionData.RestartRequested) ExitHandler();
    }
}