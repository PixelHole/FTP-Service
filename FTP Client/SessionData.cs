using FTP_Client.Windows;

namespace FTP_Client;

public static class SessionData
{
    public static Thread Main { get; set; }
    public static MainWindow MainInstance { get; set; }
    public static bool RestartRequested { get; set; } = true;
}