using System.Data;
using Dapper.Contrib.Extensions;
using ITS.Core;
using ITS.Interfaces;

namespace ITS.SQL;

public abstract class BaseRepository<TEntity> : IDataRepository<TEntity> where TEntity : class
{
    internal readonly string connectionString;

    protected BaseRepository(string connectionString) => this.connectionString = connectionString;

    protected IDbConnection Connection { get; }

    public async Task<PaginatedList<TEntity>> SearchAsync(int page, int pageSize, string query = "") => throw new NotImplementedException();

    public virtual Task<PaginatedList<TEntity>> GetAsync(int page, int pageSize) => throw new NotImplementedException();

    public virtual async Task<List<TEntity>> GetAsync()
    {
        using var currentConnection = Connection;
        return (await currentConnection.GetAllAsync<TEntity>()).ToList();
    }

    public virtual async Task<bool> DeleteAsync(string entityId) => throw new NotImplementedException();
 
    public virtual async Task<bool> UpdateAsync(TEntity entity) => throw new NotImplementedException();
 
    public virtual async Task<TEntity> InsertAsync(TEntity entity) => throw new NotImplementedException();

    public virtual async Task<TEntity> DetailsAsync(string entityId) => throw new NotImplementedException();

    public virtual async Task<int> GetCountAsync() => throw new NotImplementedException();
}