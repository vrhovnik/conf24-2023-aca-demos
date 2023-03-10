using ITS.Core;
using ITS.Models;

namespace ITS.Interfaces;

public interface IWorkStatsRepository
{
    Task<bool> GenerateStatsAsync(WorkTaskStats stats);
    Task<List<WorkTaskStats>> GetAllAsync();
    Task<PaginatedList<WorkTaskStats>> GetStatsAsync(DateTime from, DateTime to);
}