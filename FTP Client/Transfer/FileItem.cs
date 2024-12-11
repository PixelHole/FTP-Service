using Message_Board.Network;
using Newtonsoft.Json;

namespace FileInformation;

public class FileItem
{
    public string Name { get; private set; }
    public string Extension { get; private set; }
    public bool IsFolder { get; private set; }
    public string ServerPath { get; private set; }


    [JsonConstructor]
    public FileItem(string name, string extension, bool isFolder, string serverPath)
    {
        Name = name;
        Extension = extension;
        IsFolder = isFolder;
        ServerPath = serverPath;
    }

    public override string ToString()
    {
        return $"{Name}{NetworkFlags.SeparatorFlag}{Extension}{NetworkFlags.SeparatorFlag}" +
               $"{ServerPath}{NetworkFlags.SeparatorFlag}";
    }
}