using FTP_Client.Windows;
using FTP_Client.Windows.Controls;
using Terminal.Gui;

namespace FTP_Client;

class Program
{
    public static void Main(string[] args)
    {
        Application.Run<MainWindow>();
        Application.Shutdown();
    }
}

