using System.Data;
using System.Data.SqlClient;
using Dapper;
using ITS.Interfaces;
using ITS.Models;

namespace ITS.SQL;

public class ProfileSettingsService : IProfileSettingsService
{
    private  readonly string connectionString;
    private IDbConnection connection;

    public ProfileSettingsService(string connectionString) => this.connectionString = connectionString;

    public async Task<UserSettings> GetAsync(string id)
    {
        await using var connection = new SqlConnection(connectionString);

        var query =
            "SELECT U.UserSettingId as Id, U.EmailNotification, U.UserId FROM UserSetting U WHERE U.UserId=@id;" +
            "SELECT U.UserId as TTAUserId, U.FullName, U.Email, U.Password FROM Users U WHERE U.UserId=@id;";

        var result = await connection.QueryMultipleAsync(query, new { id });
        var ttaUserSettings = await result.ReadSingleAsync<UserSettings>();
        ttaUserSettings.User = await result.ReadSingleAsync<ItsUser>();

        return ttaUserSettings;
    }

    public async Task<bool> SaveAsync(UserSettings contentModel)
    {
        await using var connection = new SqlConnection(connectionString);
        return await connection.ExecuteAsync(
            $"UPDATE UserSetting SET EmailNotification=@{nameof(UserSettings.EmailNotification)} WHERE UserSettingId=@{nameof(UserSettings.Id)}",
            contentModel) > 0;
    }

    public async Task<bool> DeleteAsync(string id)
    {
        await using var connection = new SqlConnection(connectionString);
        return await connection.ExecuteAsync("DELETE FROM UserSetting WHERE UserSettingId=@id", new { id }) > 0;
    }

    public async Task<List<UserSettings>> GetListAsync()
    {
        await using var connection = new SqlConnection(connectionString);

        var ttaUserSettingsEnumerable = await connection.QueryAsync<UserSettings>(
            "SELECT U.UserSettingId as Id, U.EmailNotification, U.UserId FROM UserSetting U ");
        return ttaUserSettingsEnumerable.ToList();
    }
}