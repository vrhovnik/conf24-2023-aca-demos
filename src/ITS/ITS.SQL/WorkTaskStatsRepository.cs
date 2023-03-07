using ITS.Core;
using ITS.Interfaces;
using ITS.Models;

namespace ITS.SQL;

public class WorkTaskStatsRepository : BaseRepository<WorkTaskStats>, IWorkStatsRepository
{
    public WorkTaskStatsRepository(string connectionString) : base(connectionString)
    {
    }

    public PaginatedList<WorkTask> GetStatsAsync(DateTime from, DateTime to)
    {
        throw new NotImplementedException();
    }
}