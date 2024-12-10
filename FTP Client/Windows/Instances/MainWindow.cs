using FTP_Client.Windows.Controls;
using Terminal.Gui;

namespace FTP_Client.Windows;

public class MainWindow : Window
{
    private Label AccountStatus { get; set; }
    private Label ConnectionStatus { get; set; }
    private ListView LocalFilesList { get; set; }
    private ListView ServerFilesList { get; set; }

    public MainWindow()
    {
        SetupElements();
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
                new MenuItem("Login", "", ExitHandler),
                new MenuItem("Logout", "", ExitHandler)
            }),
            new MenuBarItem("Server", new []
            {
                new MenuItem("Reconnect", "", ExitHandler),
                new MenuItem("Refresh", "", ExitHandler),
                new MenuItem("Disconnect", "", ExitHandler),
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

        ServerFilesList = new ListView()
        {
            X = 0,
            Y = Pos.Bottom(ConnectionStatus),
            Width = Dim.Percent(60),
            Height = Dim.Percent(80),
            Title = "Server Files",
            BorderStyle = LineStyle.Rounded
        };
        LocalFilesList = new ListView()
        {
            X = Pos.Right(ServerFilesList),
            Y = Pos.Y(ServerFilesList),
            Width = Dim.Fill(),
            Height = Dim.Height(ServerFilesList),
            Title = "Local Files",
            BorderStyle = LineStyle.Rounded
        };

        Add(menu, ConnectionStatus, separator, AccountStatus, ServerFilesList, LocalFilesList);
    }

    public void SetConnectionStatus(bool connection)
    {
        string connectionText = connection ? "✓" : "X";
        ConnectionStatus.Text = $"Connection Status : {connectionText}";
    }
    
    private void ExitHandler()
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
}