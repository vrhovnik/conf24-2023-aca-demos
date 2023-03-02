using ITS.Models;

namespace ITS.Interfaces;

public interface ITagRepository : IDataRepository<Tag>
{
    public Task<List<Tag>> GetAllAsync();
}