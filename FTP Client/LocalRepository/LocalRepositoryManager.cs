using FileInformation;

namespace FTP_Client.LocalRepository;

public static class LocalRepositoryManager
{
    public static List<FileItem> LocalFiles { get; private set; } = new List<FileItem>();
    public static string LocalRepositoryPath { get; private set; } = "M:\\FTP Client repo";
    
    
    
    public static void IndexLocalRepo()
    {
        foreach (var directory in Directory.GetDirectories(LocalRepositoryPath))
        {
            AddFile(new FileItem(directory, true));
        }

        foreach (var file in Directory.GetFiles(LocalRepositoryPath))
        {
            AddFile(new FileItem(file, false));
        }
    }
    public static bool AddFile(FileItem file)
    {
        if (LocalFiles.Contains(file)) return false;
        LocalFiles.Add(file);
        return true;
    }
    public static bool RemoveFile(FileItem file)
    {
        return LocalFiles.Remove(file);
    }

}