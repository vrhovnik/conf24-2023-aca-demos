using ITS.Core;

namespace ITS.Interfaces;

public interface IDataRepository<T> where T : class
{
    Task<PaginatedList<T>> SearchAsync(int page, int pageSize, string query = "");
    Task<PaginatedList<T>> GetAsync(int page, int pageSize);
    Task<List<T>> GetAsync();
    Task<bool> DeleteAsync(string entityId);
    Task<bool> UpdateAsync(T entity);
    Task<T> InsertAsync(T entity);
    Task<T> DetailsAsync(string entityId);
    Task<int> GetCountAsync();
}