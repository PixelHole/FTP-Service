using FileInformation;

namespace FTP_Client.LocalRepository;

public static class LocalRepositoryManager
{
    public static List<FileData> LocalFiles { get; private set; } = new List<FileData>();
    public static string LocalRepositoryPath { get; private set; } = "M:\\FTP Client repo";
    
    
    
    public static void IndexLocalRepo()
    {
        foreach (var directory in Directory.GetDirectories(LocalRepositoryPath))
        {
            AddFile(new FileData(directory, true));
        }

        foreach (var file in Directory.GetFiles(LocalRepositoryPath))
        {
            AddFile(new FileData(file, false));
        }
    }
    public static bool AddFile(FileData file)
    {
        if (LocalFiles.Contains(file)) return false;
        LocalFiles.Add(file);
        return true;
    }
    public static bool RemoveFile(FileData file)
    {
        return LocalFiles.Remove(file);
    }

    public static bool IndexFileAt(string path)
    {
        return AddFile(new FileData(path, false));
    }
}