using System.Data.SqlClient;
using Dapper;
using ITS.Interfaces;
using ITS.Models;

namespace ITS.SQL;

public class WorkTaskCommentRepository : BaseRepository<WorkTaskComment>, IWorkTaskCommentRepository
{
    public WorkTaskCommentRepository(string connectionString) : base(connectionString)
    {
    }

    public override async Task<WorkTaskComment> InsertAsync(WorkTaskComment entity)
    {
        await using var connection = new SqlConnection(connectionString);
        entity.StartDate = DateTime.Now;

        var item = await connection.ExecuteScalarAsync(
            "INSERT INTO WorkTaskComments(UserId,WorkTaskId,Comment,StartDate)VALUES" +
            "(@userId,@workTaskId,@comment,@startDate);" +
            "SELECT CAST(SCOPE_IDENTITY() as bigint)",
            new
            {
                workTaskId = entity.AssignedTask.WorkTaskId,
                comment = entity.Comment,
                startDate = entity.StartDate,
                userId = entity.User.ItsUserId
            });

        var workCommentId = Convert.ToInt64(item);
        entity.WorkTaskCommentId = workCommentId.ToString();
        return entity;
    }

    public override async Task<bool> DeleteAsync(string entityId)
    {
        await using var connection = new SqlConnection(connectionString);
        var item = await connection.ExecuteAsync(
            "DELETE FROM WorkTaskComments WHERE WorkTaskCommentId=@entityId", new { entityId });

        return item > 0;
    }

    public async Task<List<WorkTaskComment>> GetCommentsForWorkTaskAsync(string workTaskId)
    {
        await using var connection = new SqlConnection(connectionString);
        var query =
            "SELECT W.WorkTaskId, W.StartDate as [Start], W.EndDate as [End],W.IsPublic, W.Description, W.CategoryId," +
            "W.UserId as ItsUserId FROM WorkTasks W WHERE W.WorkTaskId=@workTaskId;" +
            "SELECT U.FullName,U.UserId as ItsUserId, T.Email FROM Users U JOIN WorkTasks FFT on FFT.UserId=U.UserId WHERE FFT.WorkTaskId=@workTaskId;" +
            "SELECT C.* FROM Category C JOIN WorkTasks FF on FF.CategoryId=C.CategoryId WHERE FF.WorkTaskId=@workTaskId;" +
            "SELECT F.* FROM Tags F JOIN WorkTask2Tags WT on WT.TagName=F.TagName WHERE WT.WorkTaskId=@workTaskId;";

        var result = await connection.QueryMultipleAsync(query, new { workTaskId });
        var workTask = await result.ReadSingleAsync<WorkTask>();
        workTask.User = await result.ReadSingleAsync<ItsUser>();
        workTask.Category = await result.ReadSingleAsync<Category>();
        workTask.Tags = (await result.ReadAsync<Tag>()).ToList();

        query =
            "SELECT WTC.WorkTaskCommentId, WTC.Comment,WTC.StartDate, WTC.UserId as ItsUserId,  " +
            "U.FullName,U.Email,WTC.WorkTaskId " +
            "FROM WorkTaskComments WTC " +
            "JOIN dbo.Users U on WTC.UserId = U.UserId " +
            "WHERE WTC.WorkTaskId=@workTaskId ORDER BY WTC.StartDate DESC;" +
            "SELECT U.FullName,U.UserId as ItsUserId, U.Email FROM Users U JOIN WorkTaskComments FFT on FFT.UserId=U.UserId WHERE FFT.WorkTaskId=@workTaskId";

        var grid = await connection.QueryMultipleAsync(query, new { workTaskId });
        var lookup = new Dictionary<string, WorkTaskComment>();

        grid.Read<WorkTaskComment, ItsUser, WorkTaskComment>((workTaskComment, user) =>
        {
            workTaskComment.User = user;
            workTaskComment.AssignedTask = workTask;

            if (!lookup.TryGetValue(workTaskComment.WorkTaskCommentId, out _))
                lookup.Add(workTaskComment.WorkTaskCommentId, workTaskComment);

            return workTaskComment;
        }, splitOn: "ItsUserId");

        return lookup.Values.ToList();
    }
}