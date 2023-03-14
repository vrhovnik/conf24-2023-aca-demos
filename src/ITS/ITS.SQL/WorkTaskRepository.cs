using System.Data.SqlClient;
using Dapper;
using ITS.Core;
using ITS.Interfaces;
using ITS.Models;

namespace ITS.SQL;

public class WorkTaskRepository : BaseRepository<WorkTask>, IWorkTaskRepository
{
    public WorkTaskRepository(string connectionString) : base(connectionString)
    {
    }

    public override async Task<List<WorkTask>> GetAsync()
    {
        await using var connection = new SqlConnection(connectionString);
        var sqlQuery =
            "SELECT T.WorkTaskId, T.IsPublic,T.IsCompleted, T.StartDate as [Start], T.EndDate as [End], T.Description, C.CategoryId, C.Name, " +
            "T.UserId as ItsUserId, FF.TagName  " +
            " FROM WorkTasks T JOIN WorkTask2Tags FF on FF.WorkTaskId=T.WorkTaskId " +
            " JOIN Categories C on C.CategoryId=T.CategoryId ";

        var grid = await connection.QueryAsync<WorkTask>(sqlQuery);
        return grid.ToList();
    }

    public override async Task<WorkTask> DetailsAsync(string entityId)
    {
        await using var connection = new SqlConnection(connectionString);

        var query =
            "SELECT W.WorkTaskId, W.IsCompleted, W.StartDate as [Start], W.EndDate as [End],W.IsPublic, W.Description, W.CategoryId," +
            "W.UserId as ItsUserId FROM WorkTasks W WHERE W.WorkTaskId=@entityId;" +
            "SELECT U.UserId as ItsUserId, U.FullName, U.Email, U.Password FROM Users U JOIN WorkTasks FFT on FFT.UserId=U.UserId WHERE FFT.WorkTaskId=@entityId;" +
            "SELECT C.* FROM Categories C JOIN WorkTasks FF on FF.CategoryId=C.CategoryId WHERE FF.WorkTaskId=@entityId;" +
            "SELECT F.* FROM Tags F JOIN WorkTask2Tags WT on WT.TagName=F.TagName WHERE WT.WorkTaskId=@entityId;";

        var result = await connection.QueryMultipleAsync(query, new { entityId });
        var workTask = await result.ReadSingleAsync<WorkTask>();
        workTask.User = await result.ReadSingleAsync<ItsUser>();
        workTask.Category = await result.ReadSingleAsync<Category>();
        workTask.Tags = (await result.ReadAsync<Tag>()).ToList();

        //get all comments with associated users and work tasks
        query =
            "SELECT WTC.WorkTaskCommentId, WTC.Comment,WTC.StartDate, WTC.UserId as ItsUserId,  " +
            "U.FullName,U.Email,WTC.WorkTaskId " +
            "FROM WorkTaskComments WTC " +
            "JOIN dbo.Users U on WTC.UserId = U.UserId " +
            "WHERE WTC.WorkTaskId=@entityId ORDER BY WTC.StartDate DESC;" +
            "SELECT U.FullName,U.UserId as ItsUserId, U.Email FROM Users U JOIN WorkTaskComments FFT on FFT.UserId=U.UserId WHERE FFT.WorkTaskId=@entityId";

        var grid = await connection.QueryMultipleAsync(query, new { entityId });
        var lookup = new Dictionary<string, WorkTaskComment>();

        grid.Read<WorkTaskComment, ItsUser, WorkTaskComment>((workTaskComment, user) =>
        {
            workTaskComment.User = user;
            workTaskComment.AssignedTask = workTask;

            if (!lookup.TryGetValue(workTaskComment.WorkTaskCommentId, out _))
                lookup.Add(workTaskComment.WorkTaskCommentId, workTaskComment);

            return workTaskComment;
        }, splitOn: "ItsUserId");

        workTask.Comments = lookup.Values.ToList();
        return workTask;
    }

    public override async Task<WorkTask> InsertAsync(WorkTask entity)
    {
        await using var connection = new SqlConnection(connectionString);
        var item = await connection.ExecuteScalarAsync(
            "INSERT INTO WorkTasks(Description,CategoryId,StartDate, EndDate,UserId,IsPublic, IsCompleted)VALUES" +
            "(@description,@categoryId,@startDate,@endDate,@userId,@isPublic,@isCompleted);" +
            "SELECT CAST(SCOPE_IDENTITY() as bigint)",
            new
            {
                description = entity.Description,
                categoryId = entity.Category.CategoryId,
                startDate = entity.Start,
                endDate = entity.End,
                userId = entity.User.ItsUserId,
                isPublic = entity.IsPublic,
                isCompleted = entity.IsCompleted
            });

        var workTaskId = Convert.ToInt64(item);
        entity.WorkTaskId = workTaskId.ToString();

        foreach (var tag in entity.Tags)
        {
            await connection.ExecuteAsync(
                "INSERT INTO WorkTask2Tags(WorkTaskId,TagName)VALUES(@workTaskId,@currentTag)",
                new { workTaskId, currentTag = tag.TagName });
        }

        return entity;
    }

    public override async Task<bool> UpdateAsync(WorkTask entity)
    {
        await using var connection = new SqlConnection(connectionString);
        var workTaskId = entity.WorkTaskId;
        var item = await connection.ExecuteAsync(
            "UPDATE WorkTasks SET Description=@description,CategoryId=@categoryId,StartDate=@startDate," +
            "EndDate=@endDate,UserId=@userId,IsPublic=@isPublic,IsCompleted=@isCompleted WHERE WorkTaskId=@workTaskId",
            new
            {
                description = entity.Description,
                categoryId = entity.Category.CategoryId,
                startDate = entity.Start,
                endDate = entity.End,
                userId = entity.User.ItsUserId,
                isPublic = entity.IsPublic,
                isCompleted = entity.IsCompleted,
                workTaskId
            });

        if (item < 0) return false;

        item = await connection.ExecuteAsync("DELETE FROM WorkTask2Tags WHERE WorkTaskId=@workTaskId",
            new { workTaskId });

        if (item <= 0) return true;

        foreach (var tag in entity.Tags)
        {
            await connection.ExecuteAsync(
                "INSERT INTO WorkTask2Tags(WorkTaskId,TagName)VALUES(@workTaskId,@currentTag)",
                new { workTaskId, currentTag = tag.TagName });
        }

        return true;
    }

    public override async Task<bool> DeleteAsync(string entityId)
    {
        await using var connection = new SqlConnection(connectionString);
        var item = await connection.ExecuteAsync(
            $"DELETE FROM WorkTasks WHERE WorkTaskId=@entityId", new { entityId });

        if (item < 0) return false;

        await connection.ExecuteAsync("DELETE FROM WorkTask2Tags WHERE WorkTaskId=@workTaskId",
            new { entityId });

        await connection.ExecuteAsync("DELETE FROM WorkTaskComments WHERE WorkTaskId=@workTaskId",
            new { entityId });

        return true;
    }

    public async Task<WorkTask> MostActiveTaskAsync(string userId = "")
    {
        await using var connection = new SqlConnection(connectionString);

        var query = "SELECT TOP 1 W.WorkTaskId " +
                    "FROM dbo.WorkTasks W " +
                    "JOIN dbo.WorkTaskComments WTC on W.WorkTaskId = WTC.WorkTaskId " +
                    "WHERE WTC.UserId=@userId " +
                    "ORDER BY (SELECT COUNT(*) FROM dbo.WorkTaskComments WC WHERE WC.UserId = W.UserId) DESC, W.UserId DESC";

        var workTaskComment = string.Empty;
        if (string.IsNullOrEmpty(userId))
        {
            query = "SELECT TOP 1 WTC.WorkTaskId " +
                    "FROM (select W.WorkTaskId, count(*) as WTCount from dbo.WorkTaskComments W group by W.WorkTaskId) WTC " +
                    "ORDER BY WTC.WTCount DESC";
            workTaskComment = await connection.QueryFirstAsync<string>(query);
        }
        else
            workTaskComment = await connection.QueryFirstAsync<string>(query, new { userId });
        
        return await DetailsAsync(workTaskComment);
    }

    public async Task<PaginatedList<WorkTask>> WorkTasksForUserAsync(string userIdentificator,
        int pageIndex = 1, int pageSize = 10, string query = "")
    {
        await using var connection = new SqlConnection(connectionString);
        var sqlQuery =
            "SELECT U.UserId as ItsUserId, U.FullName, U.Email FROM Users U WHERE U.UserId=@userIdentificator;" +
            "SELECT T.WorkTaskId, T.IsPublic,T.IsCompleted, T.StartDate as [Start], T.EndDate as [End], T.Description, C.CategoryId, C.Name, " +
            "T.UserId as ItsUserId, FF.TagName  " +
            " FROM WorkTasks T JOIN WorkTask2Tags FF on FF.WorkTaskId=T.WorkTaskId " +
            " JOIN Categories C on C.CategoryId=T.CategoryId " +
            " WHERE T.UserId=@userIdentificator";

        if (!string.IsNullOrEmpty(query)) sqlQuery += $" AND T.Description LIKE '%{query}%'";

        sqlQuery += " ORDER BY T.IsCompleted DESC";

        var grid = await connection.QueryMultipleAsync(sqlQuery, new { userIdentificator });
        var user = await grid.ReadSingleAsync<ItsUser>();
        var lookup = new Dictionary<string, WorkTask>();

        grid.Read<WorkTask, Category, Tag, WorkTask>((workTask, category, currentTag) =>
        {
            workTask.User = user;
            workTask.Category = category;

            if (!lookup.TryGetValue(workTask.WorkTaskId, out _))
                lookup.Add(workTask.WorkTaskId, workTask);

            lookup[workTask.WorkTaskId].Tags.Add(currentTag);
            return workTask;
        }, splitOn: "CategoryId,TagName");

        return PaginatedList<WorkTask>.Create(lookup.Values.ToList(), pageIndex, pageSize, query);
    }

    public async Task<PaginatedList<WorkTask>> SearchAsync(int pageIndex = 1,
        int pageSize = 10,
        bool isPublic = true,
        string query = "")
    {
        await using var connection = new SqlConnection(connectionString);
        var sqlQuery =
            "SELECT T.WorkTaskId, T.IsCompleted,T.IsPublic, T.StartDate as [Start], T.EndDate as [End], T.Description, C.CategoryId, C.Name, " +
            "T.UserId as ItsUserId, FF.TagName  " +
            " FROM WorkTasks T JOIN WorkTask2Tags FF on FF.WorkTaskId=T.WorkTaskId " +
            " JOIN Categories C on C.CategoryId=T.CategoryId ";

        if (isPublic) sqlQuery += " WHERE T.IsPublic=1";

        if (!string.IsNullOrEmpty(query) && isPublic) sqlQuery += $" AND T.Description LIKE '%{query}%'";
        if (!string.IsNullOrEmpty(query) && !isPublic) sqlQuery += $" WHERE T.Description LIKE '%{query}%'";

        sqlQuery += " ORDER BY T.UserId, T.IsCompleted DESC";

        var grid = await connection.QueryMultipleAsync(sqlQuery);
        var lookup = new Dictionary<string, WorkTask>();

        grid.Read<WorkTask, Category, ItsUser, Tag, WorkTask>((workTask, category, user, currentTag) =>
        {
            workTask.User = user;
            workTask.Category = category;

            if (!lookup.TryGetValue(workTask.WorkTaskId, out _))
                lookup.Add(workTask.WorkTaskId, workTask);

            lookup[workTask.WorkTaskId].Tags.Add(currentTag);
            return workTask;
        }, splitOn: "CategoryId,ItsUserId,TagName");

        return PaginatedList<WorkTask>.Create(lookup.Values.ToList(), pageIndex, pageSize, query);
    }

    public async Task<PaginatedList<WorkTask>> SearchCompletedAsync(int pageIndex = 1, int pageSize = 10,
        string query = "")
    {
        var results = await SearchAsync(pageIndex, pageSize, false, query);
        var workTasks = results.Where(d => d.IsCompleted).ToList();
        return PaginatedList<WorkTask>.Create(workTasks, pageIndex, pageSize, query);
    }

    public async Task<bool> CompleteTaskAsync(string workTaskId)
    {
        await using var connection = new SqlConnection(connectionString);
        var sqlQuery =
            "UPDATE WorkTasks SET IsCompleted=1 WHERE WorkTaskId=@workTaskId";
        return await connection.ExecuteAsync(sqlQuery, new { workTaskId }) > 0;
    }

    public Task<PaginatedList<WorkTask>> GetTasksFromAsync(DateTime from, DateTime to)
    {
        throw new NotImplementedException();
    }
}