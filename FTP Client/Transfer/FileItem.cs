using Message_Board.Network;
using Newtonsoft.Json;

namespace FileInformation;

public class FileItem
{
    [JsonProperty] public string Name { get; private set; }
    [JsonProperty] public string Extension { get; private set; }
    [JsonProperty] public bool IsFolder { get; private set; }
    [JsonProperty] public string Path { get; private set; }


    [JsonConstructor]
    public FileItem(string name, string extension, bool isFolder, string path)
    {
        Name = name;
        Extension = extension;
        IsFolder = isFolder;
        Path = path;
    }
    public FileItem(string path, bool isFolder)
    {
        string[] splitPath = path.Split('\\');
        IsFolder = isFolder;
        Name = splitPath[splitPath.Length - 1];
        if (IsFolder)
        {
            Extension = "Folder";
            return;
        }

        string[] splitName = Name.Split('.');
        Extension = splitName[splitName.Length - 1];
    }

    public override string ToString()
    {
        return $"{Name}{NetworkFlags.SeparatorFlag}{Extension}{NetworkFlags.SeparatorFlag}" +
               $"{Path}{NetworkFlags.SeparatorFlag}";
    }
    public override bool Equals(object? obj)
    {
        if (obj is FileItem item)
            return string.Equals(Name, item.Name)
                   && string.Equals(Extension, item.Extension)
                   && string.Equals(Path, item.Path)
                   && IsFolder == item.IsFolder;
        return base.Equals(obj);
    }
}