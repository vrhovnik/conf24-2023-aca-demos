namespace ITS.Core;

public class AzureStorageOptions
{
    public string StorageConnectionString { get; set; }
    public string Container { get; set; } = "files";
    public string FileName { get; set; } = "WorkStats.dat";
}