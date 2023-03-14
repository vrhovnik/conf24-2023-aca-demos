using System.Diagnostics;
using System.Text;
using Azure.Storage.Blobs;
using ITS.Core;
using ITS.Interfaces;
using ITS.Models;
using Newtonsoft.Json;

namespace ITS.Storage.Azure;

public class BlobWorkStatsRepository : IWorkStatsRepository
{
    private readonly string storageConnectionString;
    private readonly string containerName;
    private readonly string fileName;

    public BlobWorkStatsRepository(string storageConnectionString, string containerName, string fileName)
    {
        this.storageConnectionString = storageConnectionString;
        this.containerName = containerName;
        this.fileName = fileName;
    }

    public async Task<bool> GenerateStatsAsync(WorkTaskStats stats)
    {
        stats.DateCreated = DateTime.Now;
        var blobServiceClient = new BlobServiceClient(storageConnectionString);
        BlobContainerClient containerClient = await blobServiceClient.CreateBlobContainerAsync(containerName);

        var workTaskStatsList = await GetAllAsync();
        if (workTaskStatsList.Exists(taskStats =>
                taskStats.DateCreated.ToShortDateString() == stats.DateCreated.ToShortDateString()))
            return false;

        workTaskStatsList.Add(stats);
        
        try
        {
            var blobClient = containerClient.GetBlobClient(fileName);
            var data = JsonConvert.SerializeObject(workTaskStatsList);
            var bytes = Encoding.UTF8.GetBytes(data);
            using var ms = new MemoryStream();
            ms.Write(bytes, 0, bytes.Length);
            ms.Position = 0;
            await blobClient.UploadAsync(ms);
        }
        catch (Exception e)
        {
            Debug.WriteLine(e);
            return false;
        }

        return true;
    }

    public async Task<List<WorkTaskStats>> GetAllAsync()
    {
        var blobServiceClient = new BlobServiceClient(storageConnectionString);
        BlobContainerClient containerClient = await blobServiceClient.CreateBlobContainerAsync(containerName);
        var blobClient = containerClient.GetBlobClient(fileName);
        
        var downloadedContent = await blobClient.DownloadContentAsync();
        if (!downloadedContent.HasValue)
            throw new Exception("Download was not successful");
        
        var downloadedTaskList = Encoding.UTF8.GetString(downloadedContent.Value.Content);
        if (string.IsNullOrEmpty(downloadedTaskList))
            throw new Exception("List is empty, check storage");
        var tasks = JsonConvert.DeserializeObject<List<WorkTaskStats>>(downloadedTaskList);
        return tasks;
    }

    public async Task<PaginatedList<WorkTaskStats>> GetStatsAsync(DateTime from, DateTime to)
    {
        var taskStatsList = await GetAllAsync();
        var stats = taskStatsList.Where(stats => stats.DateCreated < from && stats.DateCreated > to);
        return new PaginatedList<WorkTaskStats>(stats, stats.Count(), 0, 15);
    }
}