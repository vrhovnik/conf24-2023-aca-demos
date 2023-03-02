using ITS.Models;

namespace ITS.Interfaces;

public interface IUserRepository : IDataRepository<ItsUser>
{
    Task<ItsUser> LoginAsync(string username, string password);
    Task<ItsUser> FindAsync(string email);
}