using ITS.Models;

namespace TTA.Models;

public interface IContentService<T> where T:ContentModel
{
    Task<T> GetAsync(string id);
    Task<bool> SaveAsync(T contentModel);
    Task<bool> DeleteAsync(string id);
    Task<List<T>> GetListAsync();
}