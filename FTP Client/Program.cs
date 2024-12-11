using FTP_Client.Windows;
using FTP_Client.Windows.Controls;
using Terminal.Gui;

namespace FTP_Client;

class Program
{
    public static void Main(string[] args)
    {
        SessionData.Main = Thread.CurrentThread;

        Application.Init();
        
        while (SessionData.RestartRequested)
        {
            SessionData.RestartRequested = false;
            
            SessionData.MainInstance = new MainWindow();
            Application.Run(SessionData.MainInstance);
        }

        Application.Shutdown();
    }
}

