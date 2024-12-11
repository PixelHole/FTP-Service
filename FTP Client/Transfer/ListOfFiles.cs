using Newtonsoft.Json;

namespace FileInformation;

public class ListOfFiles
{
    [JsonProperty] public FileItem[] FilesList { get; private set; }


    [JsonConstructor]
    public ListOfFiles(FileItem[] filesList)
    {
        FilesList = filesList;
    }
}