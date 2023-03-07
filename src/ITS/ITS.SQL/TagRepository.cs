using System.Data.SqlClient;
using Dapper;
using ITS.Interfaces;
using ITS.Models;

namespace ITS.SQL;

public class TagRepository : BaseRepository<Tag>, ITagRepository
{
    public TagRepository(string connectionString) : base(connectionString)
    {
    }

    public async Task<List<Tag>> GetAllAsync()
    {
        await using var connection = new SqlConnection(connectionString);
        var tags = await connection.QueryAsync<Tag>("SELECT T.TagName FROM Tags T");
        return tags.ToList();
    }
}