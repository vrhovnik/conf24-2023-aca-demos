using ITS.Models;

namespace ITS.Interfaces;

public interface IWorkTaskCommentRepository : IDataRepository<WorkTaskComment>
{
    public Task<List<WorkTaskComment>> GetCommentsForWorkTaskAsync(string workTaskId);
}