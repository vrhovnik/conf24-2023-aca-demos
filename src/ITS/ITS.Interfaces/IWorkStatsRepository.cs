using ITS.Core;
using ITS.Models;

namespace ITS.Interfaces;

public interface IWorkStatsRepository : IDataRepository<WorkTaskStats>
{
    PaginatedList<WorkTask> GetStatsAsync(DateTime from, DateTime to);
}