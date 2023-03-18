using Dapr.Client;
using ITS.Core;
using ITS.Interfaces;
using ITS.Models;
using Newtonsoft.Json;

namespace ITS.Storage.Dapr;

public class DaprWorkStatsRepository : IWorkStatsRepository
{
    private readonly string daprStore;
    private readonly string fileName;

    public DaprWorkStatsRepository(string daprStore, string fileName)
    {
        this.daprStore = daprStore;
        this.fileName = fileName;
    }

    public async Task<bool> GenerateStatsAsync(WorkTaskStats stats)
    {
        using var client = new DaprClientBuilder().Build();
        var workTaskStatsList = await GetAllAsync();

        if (workTaskStatsList.Exists(taskStats =>
                taskStats.DateCreated.ToShortDateString() == stats.DateCreated.ToShortDateString()))
            return false;

        workTaskStatsList.Add(stats);
        await client.SaveStateAsync(daprStore, fileName, workTaskStatsList);
        
        return true;
    }

    public async Task<List<WorkTaskStats>> GetAllAsync()
    {
        using var client = new DaprClientBuilder().Build();
        var result = await client.GetStateAsync<List<WorkTaskStats>>(daprStore, fileName);
        return result;
    }

    public async Task<PaginatedList<WorkTaskStats>> GetStatsAsync(DateTime from, DateTime to)
    {
        var taskStatsList = await GetAllAsync();
        var stats = taskStatsList.Where(stats => stats.DateCreated < from && stats.DateCreated > to);
        return new PaginatedList<WorkTaskStats>(stats, stats.Count(), 0, 15);
    }
}