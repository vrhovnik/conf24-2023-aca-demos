using System.Data.SqlClient;
using Dapper;
using ITS.Core;
using ITS.Interfaces;
using ITS.Models;

namespace ITS.SQL;

public class ItsUserRepository : BaseRepository<ItsUser>, IUserRepository
{
    public ItsUserRepository(string connectionString) : base(connectionString)
    {
    }

    public override async Task<List<ItsUser>> GetAsync()
    {
        await using var connection = new SqlConnection(connectionString);
        var sql = "SELECT U.UserId as ItsUserId, U.FullName, U.Email, U.Password FROM Users U";
        var ttaUsers = await connection.QueryAsync<ItsUser>(sql);
        return ttaUsers.ToList();
    }

    public override async Task<ItsUser> InsertAsync(ItsUser entity)
    {
        await using var connection = new SqlConnection(connectionString);
        entity.Password = PasswordHash.CreateHash(entity.Password);
        var item = await connection.ExecuteScalarAsync(
            $"INSERT INTO Users(FullName,Email,Password)VALUES(@{nameof(entity.FullName)},@{nameof(entity.Email)},@{nameof(entity.Password)});SELECT CAST(SCOPE_IDENTITY() as bigint)",
            entity);
        var userId = Convert.ToInt64(item);
        entity.ItsUserId = userId.ToString();

        //add profile data
        await connection.ExecuteAsync(
            $"INSERT INTO UserSetting(EmailNotification, UserId)VALUES(@{nameof(entity.UserSettings.EmailNotification)},@userId)",
            new { entity.UserSettings.EmailNotification, userId });

        return entity;
    }

    public override async Task<ItsUser> DetailsAsync(string entityId)
    {
        await using var connection = new SqlConnection(connectionString);
        var query = "SELECT U.UserId as ItsUserId, U.FullName, U.Email, U.Password FROM Users U WHERE U.UserId=@entityId;" +
                    "SELECT T.* FROM WorkTasks T JOIN WorkTask2Tags FF on FF.WorkTaskId=T.WorkTaskId WHERE T.UserId=@entityId;" +
                    "SELECT F.* FROM UserSetting F WHERE F.UserId=@entityId;";

        var result = await connection.QueryMultipleAsync(query, new { entityId });
        var ttaUser = await result.ReadSingleAsync<ItsUser>();
        ttaUser.Tasks = result.Read<WorkTask>().AsList();
        ttaUser.UserSettings = await result.ReadSingleAsync<UserSettings>();
        return ttaUser;
    }

    public async Task<ItsUser> LoginAsync(string username, string password)
    {
        await using var connection = new SqlConnection(connectionString);
        var item = await connection.QuerySingleOrDefaultAsync<ItsUser>(
            "SELECT U.UserId as ItsUserId, U.FullName, U.Email FROM Users U WHERE U.Email=@username", new { username });

        if (item == null) return null!;

        item = await DetailsAsync(item.ItsUserId);

        return ((PasswordHash.ValidateHash(password, item.Password) ? item : null) ?? null)!;
    }

    public async Task<ItsUser> FindAsync(string email)
    {
        await using var connection = new SqlConnection(connectionString);
        var ttaUsers = await connection.QueryAsync<ItsUser>(
            "SELECT U.UserId as ItsUserId, U.FullName, U.Email " +
            "FROM Users U WHERE U.Email=@email", new { email });
        return ((ttaUsers.Any() ? ttaUsers.ElementAt(0) : null) ?? null)!;
    }
}