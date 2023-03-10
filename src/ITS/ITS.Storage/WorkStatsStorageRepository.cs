using System.Diagnostics;
using ITS.Core;
using ITS.Interfaces;
using ITS.Models;
using Newtonsoft.Json;

namespace ITS.Storage;

public class WorkStatsStorageRepository : IWorkStatsRepository
{
    private readonly string fileName;
    public WorkStatsStorageRepository(string fileName) => this.fileName = fileName;

    public async Task<bool> GenerateStatsAsync(WorkTaskStats stats)
    {
        var allWorkStatsText = await File.ReadAllTextAsync(fileName);
        try
        {
            var allWorksStats = JsonConvert.DeserializeObject<List<WorkTaskStats>>(allWorkStatsText);
            if (allWorksStats.Any(taskStats =>
                    taskStats.DateCreated.ToShortDateString() == stats.DateCreated.ToShortDateString()))
                return false;

            allWorksStats.Add(stats);
            var fileToWriteInto = JsonConvert.SerializeObject(allWorksStats);
            await File.WriteAllTextAsync(fileName, fileToWriteInto);

            return true;
        }
        catch (Exception e)
        {
            Debug.WriteLine(e);
            return false;
        }
    }

    public async Task<List<WorkTaskStats>> GetAllAsync()
    {
        var allWorkStatsText = await File.ReadAllTextAsync(fileName);
        var allWorksStats = JsonConvert.DeserializeObject<List<WorkTaskStats>>(allWorkStatsText);
        return allWorksStats;
    }

    public async Task<PaginatedList<WorkTaskStats>> GetStatsAsync(DateTime from, DateTime to)
    {
        var allWorkStatsText = await File.ReadAllTextAsync(fileName);
        var allWorksStats = JsonConvert.DeserializeObject<List<WorkTaskStats>>(allWorkStatsText);
        var allStatsInBetween = allWorksStats.Where(stats =>
            stats.DateCreated >= from && stats.DateCreated <= to).ToList();
        return PaginatedList<WorkTaskStats>.Create(allStatsInBetween, 1, 15);
    }
}