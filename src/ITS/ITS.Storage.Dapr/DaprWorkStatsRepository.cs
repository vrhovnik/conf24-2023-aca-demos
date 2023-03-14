using System.Diagnostics;
using System.Text;
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
        try
        {
            using var client = new DaprClientBuilder().Build();
            var workTaskStatsList = await GetAllAsync();
            if (workTaskStatsList == null)
                workTaskStatsList = new List<WorkTaskStats>();
            
            if (workTaskStatsList.Exists(taskStats =>
                    taskStats.DateCreated.ToShortDateString() == stats.DateCreated.ToShortDateString()))
                return false;
            
            workTaskStatsList.Add(stats);
            var allTasksToSave = JsonConvert.SerializeObject(workTaskStatsList);
            await client.SaveStateAsync(daprStore, fileName, allTasksToSave);
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
        try
        {
            using var client = new DaprClientBuilder().Build();
            var result = await client.GetStateAsync<string>(daprStore, fileName);
            if (string.IsNullOrEmpty(result))
                throw new Exception("List is empty, check storage");
            var tasks = JsonConvert.DeserializeObject<List<WorkTaskStats>>(result);
            return tasks;
        }
        catch (Exception e)
        {
            Debug.WriteLine(e);
            return null;
        }
    }

    public async Task<PaginatedList<WorkTaskStats>> GetStatsAsync(DateTime from, DateTime to)
    {
        var taskStatsList = await GetAllAsync();
        var stats = taskStatsList.Where(stats => stats.DateCreated < from && stats.DateCreated > to);
        return new PaginatedList<WorkTaskStats>(stats, stats.Count(), 0, 15);
    }
}