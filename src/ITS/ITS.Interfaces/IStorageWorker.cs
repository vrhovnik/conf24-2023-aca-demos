namespace ITS.Interfaces;

public interface IStorageWorker
{
    string Container { get; set; }
    string ConnectionString { get; }
    Task<string> GetFileUrlAsync(string name, bool validate);
    Task<bool> IsValidAsync(string name);
    Task<bool> UploadFileAsync(string name, Stream data);
    Task<bool> UploadFileAsync(string name, string data);
    Task<bool> DeleteFileAsync(string name);
    Task<Stream> DownloadFileAsync(string name);
    Task<string> DownloadAsStringAsync(string name);
    Task<List<Stream>> GetListAsync();
}