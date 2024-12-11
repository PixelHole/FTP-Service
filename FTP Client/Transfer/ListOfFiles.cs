using Newtonsoft.Json;

namespace FileInformation;

public class ListOfFiles
{
    [JsonProperty] public FileData[] FilesList { get; private set; }


    [JsonConstructor]
    public ListOfFiles(FileData[] filesList)
    {
        FilesList = filesList;
    }
}