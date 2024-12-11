using System.Collections.ObjectModel;
using System.Data;
using FTP_Client.Windows.Controls;
using Terminal.Gui;

namespace FTP_Client.Windows;

public class MainWindow : Window
{
    private MainWindowControl Control { get; set; }
    
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
        var menu = new MenuBar();
        menu.Menus = new[]
        {
            new MenuBarItem("File", new []
            {
                new MenuItem("Exit", "", ExitHandler)
            }),
            new MenuBarItem("Account", new []
            {
                new MenuItem("Login", "", LoginHandler),
                new MenuItem("Logout", "", LogoutHandler)
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
    private void ConnectEvents()
    {
        ServerFilesList.CellActivated += CellActionHandler;
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

        localDt.Rows.Add("...", "", "");
        
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
        string accountText = logged ? "Logged in" : "Not Logged in";
        AccountStatus.Text = $"Account Status : {accountText}";
    }
    

    // input response
    private void CellActionHandler(object? sender, CellActivatedEventArgs e)
    {
        Control.OnServerFileSelected(e.Row);
    }
    
    
    private void ChangeDirectory()
    {
        
    }
    
    private void LoginHandler()
    {
        
    }
    private void LogoutHandler()
    {
        
    }
    
    private void ReconnectHandler()
    {
        
    }
    private void RefreshHandler()
    {
        
    }
    private void DisconnectHandler()
    {
        
    }
    
    private void ExitHandler()
    {
        
    }
}