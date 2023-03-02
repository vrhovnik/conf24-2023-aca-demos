using ITS.Core;
using ITS.Models;

namespace ITS.Interfaces;

public interface IWorkTaskRepository : IDataRepository<WorkTask>
{
    public Task<PaginatedList<WorkTask>> WorkTasksForUserAsync(string userIdentificator,
        int pageIndex = 1,
        int pageSize = 10,
        string query = "");
    
    public Task<PaginatedList<WorkTask>> SearchAsync(int pageIndex = 1,
        int pageSize = 10,
        bool isPublic = true,
        string query = "");
    
    public Task<PaginatedList<WorkTask>> SearchCompletedAsync(int pageIndex = 1,
        int pageSize = 10,
        string query = "");

    public Task<bool> CompleteTaskAsync(string workTaskId);

    public Task<PaginatedList<WorkTask>> GetTasksFromAsync(DateTime from, DateTime to);
}