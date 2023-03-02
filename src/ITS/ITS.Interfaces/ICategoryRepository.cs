using ITS.Models;

namespace ITS.Interfaces;

public interface ICategoryRepository : IDataRepository<Category>
{
        public Task<List<Category>> GetAllAsync();
}